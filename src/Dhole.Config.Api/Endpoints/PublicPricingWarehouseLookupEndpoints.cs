using System.Data;
using System.Data.Common;
using System.Text.Json;
using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Config.Api.Endpoints;

public static class PublicPricingWarehouseLookupEndpoints
{
    private const string WarehouseCatalogSlug = "pricing-warehouses";

    public static IEndpointRouteBuilder MapPublicPricingWarehouseLookupEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/config/public/pricing-warehouses/resolve", ResolveAsync)
            .WithTags("Public pricing warehouses")
            .AllowAnonymous();

        return app;
    }

    private static async Task<IResult> ResolveAsync(
        string pol,
        string? shipmentMode,
        string? route,
        ServiceDbContext db,
        CancellationToken cancellationToken)
    {
        var requestedPol = pol.Trim();
        if (string.IsNullOrWhiteSpace(requestedPol)) return Results.BadRequest();

        var requestedCity = requestedPol
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault() ?? requestedPol;

        await using var connection = db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open) await connection.OpenAsync(cancellationToken);

        // This lookup intentionally starts from pricing-warehouses. It does not depend on
        // ports/pol first; the WHS catalog itself is the source of truth for the public QR page.
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT item.id,
                   item.name,
                   item.code,
                   COALESCE(item.value, ''),
                   COALESCE(item.metadata_json, '{}'::jsonb)::text,
                   item.sort_order
            FROM config."CatalogItems" item
            INNER JOIN config."CatalogGroups" catalog_group
                ON catalog_group.id = item.catalog_group_id
            WHERE catalog_group.slug = 'pricing-warehouses'
              AND catalog_group.is_deleted = FALSE
              AND catalog_group.is_active = TRUE
              AND item.is_deleted = FALSE
              AND item.is_active = TRUE
            ORDER BY item.sort_order, item.name;
            """;

        var candidates = new List<WarehouseCandidate>();
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                candidates.Add(new WarehouseCandidate(
                    reader.GetGuid(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetString(4),
                    reader.GetInt32(5)));
            }
        }

        if (candidates.Count == 0) return Results.NotFound();

        var selected = candidates
            .Select(candidate => new
            {
                Candidate = candidate,
                PolScore = ScorePol(candidate, requestedPol, requestedCity),
                RoutingScore = ScoreRouting(candidate.MetadataJson, shipmentMode, route),
            })
            .Where(item => item.PolScore > 0)
            .OrderByDescending(item => item.PolScore)
            .ThenByDescending(item => item.RoutingScore)
            .ThenBy(item => item.Candidate.SortOrder)
            .Select(item => item.Candidate)
            .FirstOrDefault();

        if (selected is null) return Results.NotFound();

        using var document = ParseMetadata(selected.MetadataJson);
        var root = document.RootElement;
        var contacts = ReadContacts(root, shipmentMode, route);
        var photos = ReadPhotos(root);

        return Results.Ok(new
        {
            id = selected.Id,
            name = selected.Name,
            code = selected.Code,
            polId = (Guid?)null,
            polValue = requestedPol,
            polCity = requestedCity,
            polCode = ReadBestPolCode(root, selected.Code, requestedCity),
            shipmentMode = string.IsNullOrWhiteSpace(shipmentMode) ? null : shipmentMode.Trim(),
            route = string.IsNullOrWhiteSpace(route) ? null : route.Trim(),
            address = ReadString(root, "address") ?? ReadString(root, "fullAddress") ?? string.Empty,
            city = ReadString(root, "city") ?? requestedCity,
            country = ReadString(root, "country") ?? ReadString(root, "countryCode") ?? string.Empty,
            schedule = ReadString(root, "schedule") ?? string.Empty,
            latitude = ReadDecimal(root, "latitude"),
            longitude = ReadDecimal(root, "longitude"),
            contacts,
            photos,
            sourceCatalog = WarehouseCatalogSlug,
            message = "Estos son los datos de Castro Fallas en origen."
        });
    }

    private static int ScorePol(WarehouseCandidate candidate, string requestedPol, string requestedCity)
    {
        var requested = Normalize(requestedPol);
        var city = Normalize(requestedCity);
        var compactCity = Compact(requestedCity);
        var score = 0;

        if (Normalize(candidate.Value) == requested) score = Math.Max(score, 1000);
        if (Normalize(candidate.Name) == requested) score = Math.Max(score, 980);
        if (Normalize(candidate.Value) == city) score = Math.Max(score, 930);
        if (Normalize(candidate.Name) == city) score = Math.Max(score, 920);

        var compactCode = Compact(candidate.Code);
        if (compactCode == $"WHS{compactCity}" || compactCode == compactCity)
            score = Math.Max(score, 900);

        using var document = ParseMetadata(candidate.MetadataJson);
        var root = document.RootElement;

        foreach (var property in new[] { "polValue", "polName", "city" })
        {
            var value = ReadString(root, property);
            if (Normalize(value) == requested) score = Math.Max(score, 970);
            else if (Normalize(value) == city) score = Math.Max(score, 910);
        }

        foreach (var property in new[] { "polValues", "polNames", "polCodes" })
        {
            foreach (var value in ReadStringArray(root, property))
            {
                var normalized = Normalize(value);
                var compact = Compact(value);
                if (normalized == requested) score = Math.Max(score, 960);
                else if (normalized == city || compact == compactCity) score = Math.Max(score, 890);
            }
        }

        if (score == 0)
        {
            if (Normalize(candidate.Name).Contains(city, StringComparison.OrdinalIgnoreCase)
                || Normalize(candidate.Value).Contains(city, StringComparison.OrdinalIgnoreCase))
            {
                score = 500;
            }
        }

        return score;
    }

    private static int ScoreRouting(string metadataJson, string? shipmentMode, string? route)
    {
        using var document = ParseMetadata(metadataJson);
        var root = document.RootElement;
        var requestedShipmentMode = Normalize(shipmentMode);
        var requestedRoute = Normalize(route);
        var score = 0;

        var shipmentModes = ReadRuleValues(root, ["shipmentModes", "modalities"], ["shipmentMode", "modality"]);
        if (!string.IsNullOrWhiteSpace(requestedShipmentMode))
        {
            if (shipmentModes.Any(value => Normalize(value) == requestedShipmentMode)) score += 80;
            else if (shipmentModes.Length > 0) score -= 20;
        }

        var routes = ReadRuleValues(root, ["routes"], ["route"]);
        if (!string.IsNullOrWhiteSpace(requestedRoute))
        {
            if (routes.Any(value => RouteMatches(value, requestedRoute))) score += 120;
            else if (routes.Length > 0) score -= 10;
        }

        var contacts = ReadAllContacts(root);
        if (contacts.Count > 0)
        {
            score += contacts.Max(contact => ScoreContact(contact, requestedShipmentMode, requestedRoute));
        }

        return score;
    }

    private static PublicContact[] ReadContacts(JsonElement root, string? shipmentMode, string? route)
    {
        var contacts = ReadAllContacts(root);
        if (contacts.Count == 0) return [];

        var requestedShipmentMode = Normalize(shipmentMode);
        var requestedRoute = Normalize(route);
        if (string.IsNullOrWhiteSpace(requestedShipmentMode) && string.IsNullOrWhiteSpace(requestedRoute))
            return contacts.OrderByDescending(contact => contact.IsPrimary).ToArray();

        var ranked = contacts
            .Select(contact => new
            {
                Contact = contact,
                Score = ScoreContact(contact, requestedShipmentMode, requestedRoute),
            })
            .Where(item => item.Score > 0)
            .ToArray();

        if (ranked.Length == 0)
        {
            return contacts
                .Where(contact => contact.Routes.Length == 0
                    && contact.ShipmentModes.Length == 0
                    && contact.Modalities.Length == 0)
                .OrderByDescending(contact => contact.IsPrimary)
                .ToArray();
        }

        var maxScore = ranked.Max(item => item.Score);
        return ranked
            .Where(item => item.Score == maxScore)
            .Select(item => item.Contact)
            .OrderByDescending(contact => contact.IsPrimary)
            .ToArray();
    }

    private static List<PublicContact> ReadAllContacts(JsonElement root)
    {
        var contacts = new List<PublicContact>();
        if (root.TryGetProperty("contactDirectory", out var directory) && directory.ValueKind == JsonValueKind.Array)
        {
            contacts.AddRange(directory.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.Object && ReadBool(item, "isActive", true))
                .Select(item => new PublicContact(
                    ReadString(item, "name") ?? string.Empty,
                    ReadString(item, "phone") ?? string.Empty,
                    ReadString(item, "email") ?? string.Empty,
                    ReadString(item, "role") ?? string.Empty,
                    ReadBool(item, "isPrimary", false),
                    ReadStringArray(item, "modalities"),
                    ReadStringArray(item, "shipmentModes"),
                    ReadStringArray(item, "routes"))));
        }

        if (contacts.Count == 0)
        {
            var legacyName = ReadString(root, "contacts") ?? ReadString(root, "contact");
            var legacyEmail = ReadString(root, "email");
            var legacyPhone = ReadString(root, "phone");
            if (!string.IsNullOrWhiteSpace(legacyName)
                || !string.IsNullOrWhiteSpace(legacyEmail)
                || !string.IsNullOrWhiteSpace(legacyPhone))
            {
                contacts.Add(new PublicContact(
                    legacyName ?? string.Empty,
                    legacyPhone ?? string.Empty,
                    legacyEmail ?? string.Empty,
                    string.Empty,
                    true,
                    [],
                    [],
                    []));
            }
        }

        return contacts;
    }

    private static int ScoreContact(PublicContact contact, string requestedShipmentMode, string requestedRoute)
    {
        var shipmentRules = contact.ShipmentModes.Length > 0 ? contact.ShipmentModes : contact.Modalities;
        var hasShipmentRules = shipmentRules.Length > 0;
        var hasRouteRules = contact.Routes.Length > 0;
        var shipmentMatch = !string.IsNullOrWhiteSpace(requestedShipmentMode)
            && shipmentRules.Any(value => Normalize(value) == requestedShipmentMode);
        var routeMatch = !string.IsNullOrWhiteSpace(requestedRoute)
            && contact.Routes.Any(value => RouteMatches(value, requestedRoute));

        if (routeMatch && shipmentMatch) return 40;
        if (routeMatch && !hasShipmentRules) return 30;
        if (shipmentMatch && !hasRouteRules) return 20;
        if (!hasShipmentRules && !hasRouteRules) return 10;
        return 0;
    }

    private static bool RouteMatches(string configured, string requested)
    {
        var configuredParts = SplitRoute(configured);
        var requestedParts = SplitRoute(requested);
        if (configuredParts.Length == 2 && requestedParts.Length == 2)
        {
            return EndpointMatches(configuredParts[0], requestedParts[0])
                && EndpointMatches(configuredParts[1], requestedParts[1]);
        }

        var left = Compact(configured);
        var right = Compact(requested);
        return left == right
            || left.Contains(right, StringComparison.OrdinalIgnoreCase)
            || right.Contains(left, StringComparison.OrdinalIgnoreCase);
    }

    private static string[] SplitRoute(string value)
    {
        var normalized = Normalize(value)
            .Replace("->", "-")
            .Replace("→", "-")
            .Replace("–", "-")
            .Replace("—", "-")
            .Replace(">", "-");

        var parts = normalized.Split('-', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length == 2 ? [Compact(parts[0]), Compact(parts[1])] : [];
    }

    private static bool EndpointMatches(string configured, string requested)
    {
        if (string.IsNullOrWhiteSpace(configured) || string.IsNullOrWhiteSpace(requested)) return false;
        return configured == requested
            || configured.Contains(requested, StringComparison.OrdinalIgnoreCase)
            || requested.Contains(configured, StringComparison.OrdinalIgnoreCase);
    }

    private static string[] ReadRuleValues(
        JsonElement root,
        IReadOnlyCollection<string> arrayProperties,
        IReadOnlyCollection<string> scalarProperties)
    {
        var values = new List<string>();
        foreach (var property in arrayProperties) values.AddRange(ReadStringArray(root, property));
        foreach (var property in scalarProperties)
        {
            var value = ReadString(root, property);
            if (!string.IsNullOrWhiteSpace(value)) values.Add(value);
        }

        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static object[] ReadPhotos(JsonElement root)
    {
        var result = new List<object>();
        if (root.TryGetProperty("images", out var images) && images.ValueKind == JsonValueKind.Array)
        {
            foreach (var image in images.EnumerateArray())
            {
                var storageId = ReadString(image, "storageId");
                if (string.IsNullOrWhiteSpace(storageId)) continue;
                result.Add(new
                {
                    storageId,
                    fileName = ReadString(image, "fileName") ?? string.Empty,
                    publicContentPath = $"/api/storage/api/v1/storage/files/{Uri.EscapeDataString(storageId)}/content"
                });
            }
        }

        if (result.Count == 0)
        {
            var storageId = ReadString(root, "imageStorageId");
            if (!string.IsNullOrWhiteSpace(storageId))
            {
                result.Add(new
                {
                    storageId,
                    fileName = ReadString(root, "imageFileName") ?? string.Empty,
                    publicContentPath = $"/api/storage/api/v1/storage/files/{Uri.EscapeDataString(storageId)}/content"
                });
            }
        }

        return result.ToArray();
    }

    private static string ReadBestPolCode(JsonElement root, string warehouseCode, string requestedCity)
    {
        var codes = ReadStringArray(root, "polCodes");
        var matching = codes.FirstOrDefault(code => Compact(code) == Compact(requestedCity));
        if (!string.IsNullOrWhiteSpace(matching)) return matching;
        if (codes.Length > 0) return codes[0];
        return warehouseCode.StartsWith("WHS_", StringComparison.OrdinalIgnoreCase)
            ? warehouseCode[4..]
            : warehouseCode;
    }

    private static string[] ReadStringArray(JsonElement root, string property)
    {
        if (!root.TryGetProperty(property, out var value) || value.ValueKind != JsonValueKind.Array) return [];
        return value.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString()?.Trim())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Cast<string>()
            .ToArray();
    }

    private static string? ReadString(JsonElement root, string property)
    {
        if (!root.TryGetProperty(property, out var value)) return null;
        return value.ValueKind == JsonValueKind.String ? value.GetString()?.Trim() : value.ToString().Trim();
    }

    private static decimal? ReadDecimal(JsonElement root, string property)
    {
        if (!root.TryGetProperty(property, out var value)) return null;
        if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var number)) return number;
        return decimal.TryParse(value.ToString(), out var parsed) ? parsed : null;
    }

    private static bool ReadBool(JsonElement root, string property, bool fallback)
    {
        if (!root.TryGetProperty(property, out var value)) return fallback;
        if (value.ValueKind is JsonValueKind.True or JsonValueKind.False) return value.GetBoolean();
        return bool.TryParse(value.ToString(), out var parsed) ? parsed : fallback;
    }

    private static JsonDocument ParseMetadata(string? metadataJson)
    {
        try
        {
            return JsonDocument.Parse(string.IsNullOrWhiteSpace(metadataJson) ? "{}" : metadataJson);
        }
        catch (JsonException)
        {
            return JsonDocument.Parse("{}");
        }
    }

    private static string Normalize(string? value) => (value ?? string.Empty).Trim().ToUpperInvariant();

    private static string Compact(string? value) => new((value ?? string.Empty)
        .ToUpperInvariant()
        .Where(char.IsLetterOrDigit)
        .ToArray());

    private sealed record WarehouseCandidate(
        Guid Id,
        string Name,
        string Code,
        string Value,
        string MetadataJson,
        int SortOrder);

    private sealed record PublicContact(
        string Name,
        string Phone,
        string Email,
        string Role,
        bool IsPrimary,
        string[] Modalities,
        string[] ShipmentModes,
        string[] Routes);
}
