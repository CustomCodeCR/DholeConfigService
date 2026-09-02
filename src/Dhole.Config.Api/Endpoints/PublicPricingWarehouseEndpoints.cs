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

        var contacts = ReadContacts(root);
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

    private static object[] ReadContacts(JsonElement root)
    {
        if (root.TryGetProperty("contactDirectory", out var directory) && directory.ValueKind == JsonValueKind.Array)
        {
            return directory.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.Object && ReadBool(item, "isActive", true))
                .Select(item => (object)new
                {
                    name = ReadString(item, "name") ?? string.Empty,
                    phone = ReadString(item, "phone") ?? string.Empty,
                    email = ReadString(item, "email") ?? string.Empty,
                    role = ReadString(item, "role") ?? string.Empty,
                    isPrimary = ReadBool(item, "isPrimary", false),
                    modalities = ReadStringArray(item, "modalities"),
                    shipmentModes = ReadStringArray(item, "shipmentModes"),
                    routes = ReadStringArray(item, "routes")
                })
                .ToArray();
        }

        var legacyName = ReadString(root, "contacts") ?? ReadString(root, "contact");
        var legacyEmail = ReadString(root, "email");
        var legacyPhone = ReadString(root, "phone");
        if (string.IsNullOrWhiteSpace(legacyName) && string.IsNullOrWhiteSpace(legacyEmail) && string.IsNullOrWhiteSpace(legacyPhone))
            return [];

        return [new { name = legacyName ?? string.Empty, phone = legacyPhone ?? string.Empty, email = legacyEmail ?? string.Empty, role = string.Empty, isPrimary = true, modalities = Array.Empty<string>(), shipmentModes = Array.Empty<string>(), routes = Array.Empty<string>() }];
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
                    publicContentPath = $"/api/storage/api/v1/storage/files/{Uri.EscapeDataString(storageId)}/public-content"
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
                    publicContentPath = $"/api/storage/api/v1/storage/files/{Uri.EscapeDataString(storageId)}/public-content"
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

    private static void Add(DbCommand command, string name, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = $"@{name}";
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }
}
