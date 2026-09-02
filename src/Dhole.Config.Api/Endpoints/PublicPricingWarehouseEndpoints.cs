using System.Data;
using System.Data.Common;
using System.Text.Json;
using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Config.Api.Endpoints;

public static class PublicPricingWarehouseEndpoints
{
    public static IEndpointRouteBuilder MapPublicPricingWarehouseEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/config/public/origin-offices/{polLocator}", GetByPolAsync)
            .WithTags("Public origin offices")
            .AllowAnonymous();

        return app;
    }

    private static async Task<IResult> GetByPolAsync(
        string polLocator,
        string? polValue,
        string? polCode,
        Guid? polId,
        string? shipmentMode,
        string? route,
        ServiceDbContext db,
        CancellationToken cancellationToken)
    {
        var locator = string.IsNullOrWhiteSpace(polValue) ? polLocator : polValue;
        var normalizedLocator = Normalize(locator);
        var normalizedPolCode = Normalize(polCode);
        if (string.IsNullOrWhiteSpace(normalizedLocator) && !polId.HasValue) return Results.BadRequest();

        await using var connection = db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open) await connection.OpenAsync(cancellationToken);

        // New QR links carry the Pricing POL id; legacy links may only carry the port code/value.
        // Resolve the canonical POL value first and then search every compatible WHS candidate.
        var resolvedPolValue = await ResolvePolValueAsync(
            connection,
            polId,
            normalizedLocator,
            cancellationToken);

        if (string.IsNullOrWhiteSpace(resolvedPolValue)) return Results.NotFound();

        var resolvedPolCity = resolvedPolValue
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault() ?? resolvedPolValue;
        var polIdText = polId?.ToString("D") ?? string.Empty;

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT item.id, item.name, item.code, item.metadata_json
            FROM config."CatalogItems" item
            INNER JOIN config."CatalogGroups" catalog_group ON catalog_group.id = item.catalog_group_id
            WHERE catalog_group.slug = 'pricing-warehouses'
              AND catalog_group.is_deleted = FALSE
              AND item.is_deleted = FALSE
              AND item.is_active = TRUE
              AND (
                    UPPER(COALESCE(item.value, '')) = UPPER(@pol_value)
                    OR UPPER(COALESCE(item.value, '')) = UPPER(@pol_city)
                    OR UPPER(item.name) = UPPER(@pol_value)
                    OR UPPER(item.name) = UPPER(@pol_city)
                    OR UPPER(item.name) LIKE '%' || UPPER(@pol_city) || '%'
                    OR UPPER(item.code) = UPPER('WHS_' || @pol_value)
                    OR UPPER(item.code) = UPPER('WHS_' || @pol_city)
                    OR regexp_replace(UPPER(item.code), '[^A-Z0-9]+', '', 'g') =
                       regexp_replace(UPPER('WHS_' || @pol_city), '[^A-Z0-9]+', '', 'g')
                    OR UPPER(COALESCE(item.metadata_json->>'city', '')) = UPPER(@pol_city)
                    OR UPPER(COALESCE(item.metadata_json->>'polValue', '')) = UPPER(@pol_value)
                    OR UPPER(COALESCE(item.metadata_json->>'polValue', '')) = UPPER(@pol_city)
                    OR UPPER(COALESCE(item.metadata_json->>'polName', '')) = UPPER(@pol_value)
                    OR UPPER(COALESCE(item.metadata_json->>'polName', '')) = UPPER(@pol_city)
                    OR EXISTS (
                        SELECT 1
                        FROM jsonb_array_elements_text(COALESCE(item.metadata_json->'polValues', '[]'::jsonb)) pol(value)
                        WHERE UPPER(pol.value) = UPPER(@pol_value)
                           OR UPPER(pol.value) = UPPER(@pol_city)
                    )
                    OR EXISTS (
                        SELECT 1
                        FROM jsonb_array_elements_text(COALESCE(item.metadata_json->'polNames', '[]'::jsonb)) pol(value)
                        WHERE UPPER(pol.value) = UPPER(@pol_value)
                           OR UPPER(pol.value) = UPPER(@pol_city)
                    )
                    OR EXISTS (
                        SELECT 1
                        FROM jsonb_array_elements_text(COALESCE(item.metadata_json->'polCodes', '[]'::jsonb)) pol(value)
                        WHERE UPPER(pol.value) = UPPER(@pol_value)
                           OR UPPER(pol.value) = UPPER(@pol_city)
                           OR (@pol_code <> '' AND UPPER(pol.value) = UPPER(@pol_code))
                    )
                    OR (@pol_code <> '' AND UPPER(item.code) = UPPER('WHS_' || @pol_code))
                    OR (@pol_code <> '' AND UPPER(COALESCE(item.metadata_json->>'polCode', '')) = UPPER(@pol_code))
                    OR (@pol_id <> '' AND UPPER(COALESCE(item.metadata_json->>'polId', '')) = UPPER(@pol_id))
                    OR (@pol_id <> '' AND EXISTS (
                        SELECT 1
                        FROM jsonb_array_elements_text(COALESCE(item.metadata_json->'polIds', '[]'::jsonb)) pol(value)
                        WHERE UPPER(pol.value) = UPPER(@pol_id)
                    ))
              )
            ORDER BY
                CASE
                    WHEN @pol_id <> '' AND UPPER(COALESCE(item.metadata_json->>'polId', '')) = UPPER(@pol_id) THEN 0
                    WHEN UPPER(COALESCE(item.value, '')) = UPPER(@pol_value) THEN 1
                    WHEN UPPER(COALESCE(item.metadata_json->>'polValue', '')) = UPPER(@pol_value) THEN 2
                    WHEN @pol_code <> '' AND UPPER(COALESCE(item.metadata_json->>'polCode', '')) = UPPER(@pol_code) THEN 3
                    WHEN UPPER(COALESCE(item.value, '')) = UPPER(@pol_city) THEN 4
                    WHEN UPPER(COALESCE(item.metadata_json->>'city', '')) = UPPER(@pol_city) THEN 5
                    WHEN UPPER(item.name) = UPPER(@pol_city) THEN 6
                    WHEN UPPER(item.code) = UPPER('WHS_' || @pol_city) THEN 7
                    ELSE 8
                END,
                item.sort_order,
                item.name;
            """;
        Add(command, "pol_value", resolvedPolValue);
        Add(command, "pol_city", resolvedPolCity);
        Add(command, "pol_code", normalizedPolCode);
        Add(command, "pol_id", polIdText);

        var candidates = new List<WarehouseCandidate>();
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            var polOrder = 0;
            while (await reader.ReadAsync(cancellationToken))
            {
                candidates.Add(new WarehouseCandidate(
                    reader.GetGuid(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.IsDBNull(3) ? "{}" : reader.GetString(3),
                    polOrder++));
            }
        }

        if (candidates.Count == 0) return Results.NotFound();

        // The URL context is part of WHS resolution, not only contact resolution.
        // Example: pol=Qingdao, China + shipmentMode=Fcl +
        // route=Qingdao, China - Puerto Caldera, Costa Rica.
        // Explicit WHS rules win; existing contactDirectory rules are also considered so
        // current catalog data does not need to be duplicated at the WHS root.
        var rankedCandidates = candidates
            .Select(candidate => new
            {
                Candidate = candidate,
                Score = ScoreWarehouse(candidate.MetadataJson, shipmentMode, route),
            })
            .Where(item => item.Score >= 0)
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Candidate.PolOrder)
            .ToArray();

        if (rankedCandidates.Length == 0) return Results.NotFound();

        var selected = rankedCandidates[0].Candidate;
        using var document = ParseMetadata(selected.MetadataJson);
        var root = document.RootElement;

        var contacts = ReadContacts(root, shipmentMode, route);
        var photos = ReadPhotos(root);

        return Results.Ok(new
        {
            id = selected.Id,
            name = selected.Name,
            code = selected.Code,
            polId,
            polValue = resolvedPolValue,
            polCity = resolvedPolCity,
            polCode = normalizedPolCode,
            shipmentMode = string.IsNullOrWhiteSpace(shipmentMode) ? null : shipmentMode.Trim(),
            route = string.IsNullOrWhiteSpace(route) ? null : route.Trim(),
            address = ReadString(root, "address") ?? ReadString(root, "fullAddress") ?? string.Empty,
            city = ReadString(root, "city") ?? string.Empty,
            country = ReadString(root, "country") ?? string.Empty,
            latitude = ReadDecimal(root, "latitude"),
            longitude = ReadDecimal(root, "longitude"),
            contacts,
            photos,
            message = "Estos son los datos de Castro Fallas en origen."
        });
    }

    private static async Task<string> ResolvePolValueAsync(
        DbConnection connection,
        Guid? polId,
        string fallback,
        CancellationToken cancellationToken)
    {
        if (polId.HasValue && polId.Value != Guid.Empty)
        {
            await using var byId = connection.CreateCommand();
            byId.CommandText = """
                SELECT COALESCE(NULLIF(BTRIM(item.value), ''), NULLIF(BTRIM(item.name), ''), item.code)
                FROM config."CatalogItems" item
                INNER JOIN config."CatalogGroups" catalog_group ON catalog_group.id = item.catalog_group_id
                WHERE item.id = @pol_id
                  AND catalog_group.slug IN ('ports', 'pol')
                  AND catalog_group.is_deleted = FALSE
                  AND item.is_deleted = FALSE
                  AND item.is_active = TRUE
                LIMIT 1;
                """;
            Add(byId, "pol_id", polId.Value);

            var result = await byId.ExecuteScalarAsync(cancellationToken);
            var value = result is null || result is DBNull ? null : Convert.ToString(result);
            if (!string.IsNullOrWhiteSpace(value)) return value.Trim();
        }

        if (string.IsNullOrWhiteSpace(fallback)) return fallback;

        await using var byLocator = connection.CreateCommand();
        byLocator.CommandText = """
            SELECT COALESCE(NULLIF(BTRIM(item.value), ''), NULLIF(BTRIM(item.name), ''), item.code)
            FROM config."CatalogItems" item
            INNER JOIN config."CatalogGroups" catalog_group ON catalog_group.id = item.catalog_group_id
            WHERE catalog_group.slug IN ('ports', 'pol')
              AND catalog_group.is_deleted = FALSE
              AND item.is_deleted = FALSE
              AND item.is_active = TRUE
              AND (
                    UPPER(item.code) = UPPER(@locator)
                    OR UPPER(COALESCE(item.value, '')) = UPPER(@locator)
                    OR UPPER(item.name) = UPPER(@locator)
              )
            ORDER BY CASE WHEN UPPER(item.code) = UPPER(@locator) THEN 0 ELSE 1 END,
                     item.sort_order,
                     item.name
            LIMIT 1;
            """;
        Add(byLocator, "locator", fallback);

        var locatorResult = await byLocator.ExecuteScalarAsync(cancellationToken);
        var locatorValue = locatorResult is null || locatorResult is DBNull ? null : Convert.ToString(locatorResult);
        return string.IsNullOrWhiteSpace(locatorValue) ? fallback : locatorValue.Trim();
    }

    private static int ScoreWarehouse(string metadataJson, string? shipmentMode, string? route)
    {
        using var document = ParseMetadata(metadataJson);
        var root = document.RootElement;
        var requestedShipmentMode = Normalize(shipmentMode);
        var requestedRoute = Normalize(route);

        var configuredShipmentModes = ReadRuleValues(
            root,
            ["shipmentModes", "modalities"],
            ["shipmentMode", "modality"]);
        var configuredRoutes = ReadRuleValues(root, ["routes"], ["route"]);

        var hasShipmentRules = configuredShipmentModes.Length > 0;
        var hasRouteRules = configuredRoutes.Length > 0;
        var shipmentMatch = !string.IsNullOrWhiteSpace(requestedShipmentMode)
            && configuredShipmentModes.Any(value => Normalize(value) == requestedShipmentMode);
        var routeMatch = !string.IsNullOrWhiteSpace(requestedRoute)
            && configuredRoutes.Any(value => RouteMatches(value, requestedRoute));

        if (!string.IsNullOrWhiteSpace(requestedShipmentMode) && hasShipmentRules && !shipmentMatch)
            return -1;
        if (!string.IsNullOrWhiteSpace(requestedRoute) && hasRouteRules && !routeMatch)
            return -1;

        var contacts = ReadAllContacts(root);
        var hasContactRules = contacts.Any(contact => contact.Routes.Length > 0 || contact.ShipmentModes.Length > 0);
        var hasGenericContact = contacts.Any(contact => contact.Routes.Length == 0 && contact.ShipmentModes.Length == 0);
        var bestContactScore = contacts.Count == 0
            ? 0
            : contacts.Max(contact => ScoreContact(contact, requestedShipmentMode, requestedRoute));

        // When a WHS has no root rules but all of its contacts are constrained to another
        // route/mode, it is not a compatible candidate for this URL.
        if (!hasShipmentRules
            && !hasRouteRules
            && hasContactRules
            && !hasGenericContact
            && bestContactScore == 0
            && (!string.IsNullOrWhiteSpace(requestedShipmentMode) || !string.IsNullOrWhiteSpace(requestedRoute)))
        {
            return -1;
        }

        var score = 0;
        if (routeMatch) score += 400;
        else if (string.IsNullOrWhiteSpace(requestedRoute) || !hasRouteRules) score += 20;

        if (shipmentMatch) score += 200;
        else if (string.IsNullOrWhiteSpace(requestedShipmentMode) || !hasShipmentRules) score += 10;

        score += bestContactScore;
        return score;
    }

    private static PublicContact[] ReadContacts(JsonElement root, string? shipmentMode, string? route)
    {
        var contacts = ReadAllContacts(root);
        if (contacts.Count == 0) return [];

        var requestedShipmentMode = Normalize(shipmentMode);
        var requestedRoute = Normalize(route);
        if (string.IsNullOrWhiteSpace(requestedShipmentMode) && string.IsNullOrWhiteSpace(requestedRoute))
            return contacts.OrderByDescending(x => x.IsPrimary).ToArray();

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
                .Where(contact => contact.Routes.Length == 0 && contact.ShipmentModes.Length == 0)
                .OrderByDescending(contact => contact.IsPrimary)
                .ToArray();
        }

        var maxScore = ranked.Max(item => item.Score);
        return ranked
            .Where(item => item.Score == maxScore)
            .Select(item => item.Contact)
            .OrderByDescending(item => item.IsPrimary)
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
        var hasRouteRules = contact.Routes.Length > 0;
        var shipmentRules = contact.ShipmentModes.Length > 0 ? contact.ShipmentModes : contact.Modalities;
        var hasShipmentRules = shipmentRules.Length > 0;
        var routeMatch = !string.IsNullOrWhiteSpace(requestedRoute)
            && contact.Routes.Any(value => RouteMatches(value, requestedRoute));
        var shipmentMatch = !string.IsNullOrWhiteSpace(requestedShipmentMode)
            && shipmentRules.Any(value => Normalize(value) == requestedShipmentMode);

        if (routeMatch && shipmentMatch) return 40;
        if (routeMatch && !hasShipmentRules) return 30;
        if (shipmentMatch && !hasRouteRules) return 20;
        if (!hasRouteRules && !hasShipmentRules) return 10;
        return 0;
    }

    private static bool RouteMatches(string configured, string requested)
    {
        var leftParts = SplitRoute(configured);
        var rightParts = SplitRoute(requested);

        if (leftParts.Length == 2 && rightParts.Length == 2)
        {
            return EndpointMatches(leftParts[0], rightParts[0])
                && EndpointMatches(leftParts[1], rightParts[1]);
        }

        var left = NormalizeRouteEndpoint(configured);
        var right = NormalizeRouteEndpoint(requested);
        return left == right
            || right.Contains(left, StringComparison.OrdinalIgnoreCase)
            || left.Contains(right, StringComparison.OrdinalIgnoreCase);
    }

    private static string[] SplitRoute(string value)
    {
        var normalized = Normalize(value)
            .Replace("->", "-")
            .Replace("→", "-")
            .Replace("–", "-")
            .Replace("—", "-")
            .Replace(">", "-")
            .Replace("_", "-");

        var parts = normalized.Split('-', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2) return [];
        return [NormalizeRouteEndpoint(parts[0]), NormalizeRouteEndpoint(parts[1])];
    }

    private static bool EndpointMatches(string configured, string requested)
    {
        if (string.IsNullOrWhiteSpace(configured) || string.IsNullOrWhiteSpace(requested)) return false;
        return configured == requested
            || configured.Contains(requested, StringComparison.OrdinalIgnoreCase)
            || requested.Contains(configured, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeRouteEndpoint(string value)
    {
        return new string(Normalize(value).Where(char.IsLetterOrDigit).ToArray());
    }

    private static string[] ReadRuleValues(
        JsonElement root,
        IReadOnlyCollection<string> arrayProperties,
        IReadOnlyCollection<string> scalarProperties)
    {
        var values = new List<string>();
        foreach (var property in arrayProperties)
            values.AddRange(ReadStringArray(root, property));

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

    private static void Add(DbCommand command, string name, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = $"@{name}";
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }

    private sealed record WarehouseCandidate(
        Guid Id,
        string Name,
        string Code,
        string MetadataJson,
        int PolOrder);

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
