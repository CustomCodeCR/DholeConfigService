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
        app.MapGet("/api/config/public/origin-offices/{polCode}", GetByPolAsync)
            .WithTags("Public origin offices")
            .AllowAnonymous();

        return app;
    }

    private static async Task<IResult> GetByPolAsync(
        string polCode,
        string? shipmentMode,
        string? route,
        ServiceDbContext db,
        CancellationToken cancellationToken)
    {
        var normalizedPol = (polCode ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalizedPol)) return Results.BadRequest();

        await using var connection = db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open) await connection.OpenAsync(cancellationToken);

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
                    UPPER(item.code) = UPPER('WHS_' || @pol)
                    OR EXISTS (
                        SELECT 1
                        FROM jsonb_array_elements_text(COALESCE(item.metadata_json->'polCodes', '[]'::jsonb)) pol(value)
                        WHERE UPPER(pol.value) = UPPER(@pol)
                    )
              )
            ORDER BY CASE WHEN UPPER(item.code) = UPPER('WHS_' || @pol) THEN 0 ELSE 1 END,
                     item.sort_order,
                     item.name
            LIMIT 1;
            """;
        Add(command, "pol", normalizedPol);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return Results.NotFound();

        var id = reader.GetGuid(0);
        var name = reader.GetString(1);
        var code = reader.GetString(2);
        var metadataJson = reader.IsDBNull(3) ? "{}" : reader.GetString(3);
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(metadataJson) ? "{}" : metadataJson);
        var root = document.RootElement;

        var contacts = ReadContacts(root, shipmentMode, route);
        var photos = ReadPhotos(root);

        return Results.Ok(new
        {
            id,
            name,
            code,
            polCode = normalizedPol,
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

    private static PublicContact[] ReadContacts(JsonElement root, string? shipmentMode, string? route)
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
            if (!string.IsNullOrWhiteSpace(legacyName) || !string.IsNullOrWhiteSpace(legacyEmail) || !string.IsNullOrWhiteSpace(legacyPhone))
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

        if (contacts.Count == 0) return [];

        var requestedShipmentMode = Normalize(shipmentMode);
        var requestedRoute = Normalize(route);
        if (string.IsNullOrWhiteSpace(requestedShipmentMode) && string.IsNullOrWhiteSpace(requestedRoute))
            return contacts.OrderByDescending(x => x.IsPrimary).ToArray();

        int Score(PublicContact contact)
        {
            var hasRouteRules = contact.Routes.Length > 0;
            var hasShipmentRules = contact.ShipmentModes.Length > 0;
            var routeMatch = !string.IsNullOrWhiteSpace(requestedRoute)
                && contact.Routes.Any(value => RouteMatches(value, requestedRoute));
            var shipmentMatch = !string.IsNullOrWhiteSpace(requestedShipmentMode)
                && contact.ShipmentModes.Any(value => Normalize(value) == requestedShipmentMode);

            if (routeMatch && shipmentMatch) return 40;
            if (routeMatch && !hasShipmentRules) return 30;
            if (shipmentMatch && !hasRouteRules) return 20;
            if (!hasRouteRules && !hasShipmentRules) return 10;
            return 0;
        }

        var ranked = contacts
            .Select(contact => new { Contact = contact, Score = Score(contact) })
            .Where(item => item.Score > 0)
            .ToArray();

        if (ranked.Length == 0) return contacts.OrderByDescending(x => x.IsPrimary).ToArray();
        var maxScore = ranked.Max(item => item.Score);
        return ranked
            .Where(item => item.Score == maxScore)
            .Select(item => item.Contact)
            .OrderByDescending(item => item.IsPrimary)
            .ToArray();
    }

    private static bool RouteMatches(string configured, string requested)
    {
        var left = Normalize(configured).Replace("→", "-").Replace("_", "-").Replace(" ", string.Empty);
        var right = Normalize(requested).Replace("→", "-").Replace("_", "-").Replace(" ", string.Empty);
        return left == right || right.Contains(left, StringComparison.OrdinalIgnoreCase) || left.Contains(right, StringComparison.OrdinalIgnoreCase);
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

    private static string Normalize(string? value) => (value ?? string.Empty).Trim().ToUpperInvariant();

    private static void Add(DbCommand command, string name, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = $"@{name}";
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }

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
