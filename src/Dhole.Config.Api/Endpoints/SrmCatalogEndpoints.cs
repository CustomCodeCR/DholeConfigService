using System.Globalization;
using System.Text.Json;
using CustomCodeFramework.Api.Responses;
using CustomCodeFramework.Cqrs.Dispatching;
using Dhole.Config.Api.Authorization;
using Dhole.Config.Api.Extensions;
using Dhole.Config.Application.Abstractions.Repositories;
using Dhole.Config.Application.CatalogGroups.CreateCatalogGroup;
using Dhole.Config.Application.CatalogItems.CreateCatalogItem;
using Dhole.Config.Application.CatalogItems.GetCatalogItemsByGroupSlug;
using Dhole.Config.Application.CatalogItems.UpdateCatalogItem;
using Dhole.Config.Contracts.Catalogs;

namespace Dhole.Config.Api.Endpoints;

/// <summary>
/// Dedicated SRM façade backed by the SAME Config catalog tables.
/// No generic Config permissions are exposed through these fixed-scope routes.
/// </summary>
public static class SrmCatalogEndpoints
{
    private const string Suppliers = "srm-suppliers";
    private const string Evaluations = "srm-evaluations";
    private const string Contracts = "srm-contracts";

    public static IEndpointRouteBuilder MapSrmCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        var route = app.MapGroup("/api/config/srm")
            .WithTags("SRM")
            .RequireAuthorization();

        route.MapGet("/suppliers", (IQueryDispatcher queries, CancellationToken ct) =>
            ReadAsync(Suppliers, queries, ct))
            .RequireScope("srm.supplier.read");
        // Read-only discovery of legacy Pricing carrier/agent directories.
        route.MapGet("/legacy-partners", LegacyPartnersAsync)
            .RequireScope("srm.supplier.read");
        // Non-destructive import with stable origin key; Pricing keeps its old code
        // until a separate contract migration and reconciliation are completed.
        route.MapPost("/legacy-partners/{source}/{legacyId:guid}/import", ImportLegacyAsync)
            .RequireScope("srm.supplier.create");
        route.MapGet("/evaluations", (IQueryDispatcher queries, CancellationToken ct) =>
            ReadAsync(Evaluations, queries, ct))
            .RequireScope("srm.performance.read");
        route.MapGet("/contracts", (IQueryDispatcher queries, CancellationToken ct) =>
            ReadAsync(Contracts, queries, ct))
            .RequireScope("srm.contract.read");

        route.MapPost("/suppliers", (
                SrmRecordRequest input, IQueryDispatcher queries, ICommandDispatcher commands,
                ICatalogGroupRepository groups, ICatalogItemRepository items, HttpContext context,
                CancellationToken ct) =>
            CreateAsync(Suppliers, "Proveedores SRM", input, commands, groups, items, context, ct))
            .RequireScope("srm.supplier.create");

        route.MapPost("/evaluations", (
                SrmRecordRequest input, ICommandDispatcher commands,
                ICatalogGroupRepository groups, ICatalogItemRepository items, HttpContext context,
                CancellationToken ct) =>
            CreateAsync(Evaluations, "Evaluaciones SRM", input, commands, groups, items, context, ct))
            .RequireScope("srm.performance.manage");

        route.MapPost("/contracts", (
                SrmRecordRequest input, ICommandDispatcher commands,
                ICatalogGroupRepository groups, ICatalogItemRepository items, HttpContext context,
                CancellationToken ct) =>
            CreateAsync(Contracts, "Contratos SRM", input, commands, groups, items, context, ct))
            .RequireScope("srm.contract.manage");

        route.MapPut("/suppliers/{supplierId:guid}", (
                Guid supplierId, SrmRecordRequest input, ICommandDispatcher commands,
                ICatalogGroupRepository groups, ICatalogItemRepository items, HttpContext context,
                CancellationToken ct) =>
            UpdateSupplierAsync(supplierId, input, commands, groups, items, context, ct))
            .RequireScope("srm.supplier.update");

        return app;
    }

    private sealed record LegacyPartnerDto(
        string SourceGroup, Guid Id, string Code, string Name, bool Imported);

    private static Guid? LegacyLink(CatalogItemDto row)
    {
        if (string.IsNullOrWhiteSpace(row.MetadataJson)) return null;
        try
        {
            using var document = JsonDocument.Parse(row.MetadataJson);
            if (document.RootElement.TryGetProperty("legacyConfigId", out var node) &&
                node.ValueKind == JsonValueKind.String &&
                Guid.TryParse(node.GetString(), out var id)) return id;
        }
        catch (JsonException) { /* Legacy SRM records may not have structured metadata. */ }
        return null;
    }

    private static async Task<IResult> LegacyPartnersAsync(
        IQueryDispatcher queries, CancellationToken ct)
    {
        var srms = await queries.DispatchAsync(new GetCatalogItemsByGroupSlugQuery(Suppliers), ct);
        var imported = srms.Select(LegacyLink).Where(x => x.HasValue)
            .Select(x => x!.Value).ToHashSet();

        var result = new List<LegacyPartnerDto>();
        foreach (var source in new[] { "carriers", "agents" })
        {
            var entries = await queries.DispatchAsync(
                new GetCatalogItemsByGroupSlugQuery(source), ct);
            result.AddRange(entries.Select(x => new LegacyPartnerDto(
                source, x.Id, x.Code, x.Name, imported.Contains(x.Id))));
        }
        return Results.Ok(result.OrderBy(x => x.SourceGroup)
            .ThenBy(x => x.Name).ToArray());
    }

    private static async Task<IResult> ImportLegacyAsync(
        string source, Guid legacyId, IQueryDispatcher queries,
        ICommandDispatcher commands, ICatalogGroupRepository groups,
        ICatalogItemRepository items, HttpContext context, CancellationToken ct)
    {
        if (source is not ("carriers" or "agents") || legacyId == Guid.Empty)
            return Results.BadRequest(new { code = "srm_invalid_legacy_source" });
        var originGroup = await groups.GetBySlugAsync(source, ct);
        var origin = await items.GetByIdAsync(legacyId, ct);
        if (originGroup is null || origin is null || origin.IsDeleted ||
            !origin.IsActive || origin.CatalogGroupId != originGroup.Id)
            return Results.NotFound(new { code = "srm_legacy_record_not_found" });

        var srms = await queries.DispatchAsync(new GetCatalogItemsByGroupSlugQuery(Suppliers), ct);
        var match = srms.FirstOrDefault(x => LegacyLink(x) == legacyId);
        if (match is not null)
            return Results.Ok(new { supplierId = match.Id, alreadyImported = true });

        // Do not merge companies just because their names resemble each other.
        // Requires a human review where a provider already exists under another ID.
        if (srms.Any(x => string.Equals(x.Name, origin.Name, StringComparison.OrdinalIgnoreCase)))
            return Results.Conflict(new { code = "srm_supplier_name_collision_review_required" });

        var metadata = JsonSerializer.Serialize(new {
            supplierType = source == "carriers" ? "carrier" : "agent",
            legacyConfigId = origin.Id,
            legacyConfigGroup = source,
            legacyCode = origin.Code,
            migratedAtUtc = DateTime.UtcNow.ToString("O"),
        });
        // Existing Config carrier/agent records must remain available to Pricing.
        // This creates the audited SRM supplier with a stable back-reference.
        var request = new SrmRecordRequest(
            origin.Name, origin.Description, origin.Value ?? origin.Name,
            metadata, srms.Count + 1);
        return await CreateAsync(Suppliers, "Proveedores SRM", request,
            commands, groups, items, context, ct);
    }

    private static async Task<IResult> ReadAsync(
        string slug, IQueryDispatcher queries, CancellationToken ct)
    {
        var records = await queries.DispatchAsync(new GetCatalogItemsByGroupSlugQuery(slug), ct);
        return Results.Ok(ApiResponse<IReadOnlyCollection<CatalogItemDto>>.Ok(records));
    }

    private static async Task<IResult> CreateAsync(
        string slug, string groupName, SrmRecordRequest request,
        ICommandDispatcher commands, ICatalogGroupRepository groups,
        ICatalogItemRepository items, HttpContext context, CancellationToken ct)
    {
        var invalid = await ValidateAsync(slug, request, groups, items, ct);
        if (invalid is not null) return invalid;

        var group = await groups.GetBySlugAsync(slug, ct);
        if (group is null)
        {
            // Only this predefined SRM catalog may be provisioned; the caller
            // cannot choose a slug or mutate arbitrary configuration.
            await commands.DispatchAsync(
                new CreateCatalogGroupCommand(
                    groupName, slug, "Datos independientes del módulo SRM",
                    null, false, context.GetCurrentUserId()), ct);
            group = await groups.GetBySlugAsync(slug, ct);
        }

        if (group is null || group.IsDeleted || !group.IsActive)
            return Results.Conflict(new { code = "srm_catalog_unavailable", slug });

        var result = await commands.DispatchAsync(
            new CreateCatalogItemCommand(
                group.Id,
                request.Name.Trim(),
                null,
                request.Description?.Trim(),
                request.Value?.Trim(),
                request.MetadataJson,
                request.SortOrder,
                false,
                context.GetCurrentUserId()),
            ct);
        return EndpointResults.FromResult(result, context);
    }

    private static async Task<IResult> UpdateSupplierAsync(
        Guid id, SrmRecordRequest request,
        ICommandDispatcher commands, ICatalogGroupRepository groups,
        ICatalogItemRepository items, HttpContext context, CancellationToken ct)
    {
        var group = await groups.GetBySlugAsync(Suppliers, ct);
        var existing = await items.GetByIdAsync(id, ct);
        if (group is null || existing is null || existing.IsDeleted ||
            existing.CatalogGroupId != group.Id)
            return Results.NotFound(new { code = "srm_supplier_not_found" });

        var invalid = await ValidateAsync(Suppliers, request, groups, items, ct);
        if (invalid is not null) return invalid;

        var result = await commands.DispatchAsync(
            new UpdateCatalogItemCommand(
                id, request.Name.Trim(), request.Description?.Trim(),
                request.Value?.Trim(), request.MetadataJson,
                request.SortOrder, context.GetCurrentUserId()), ct);
        return EndpointResults.FromResult(result, context);
    }

    private static async Task<IResult?> ValidateAsync(
        string slug, SrmRecordRequest request,
        ICatalogGroupRepository groups, ICatalogItemRepository items, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > 220 ||
            request.Description?.Length > 300)
            return Results.BadRequest(new { code = "srm_invalid_record" });

        JsonDocument? doc = null;
        try
        {
            if (string.IsNullOrWhiteSpace(request.MetadataJson))
                return Results.BadRequest(new { code = "srm_metadata_required" });

            doc = JsonDocument.Parse(request.MetadataJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
                return Results.BadRequest(new { code = "srm_invalid_metadata" });

            var metadata = doc.RootElement;
            if (slug == Suppliers)
            {
                if (!metadata.TryGetProperty("supplierType", out var supplierType) ||
                    supplierType.ValueKind != JsonValueKind.String ||
                    string.IsNullOrWhiteSpace(supplierType.GetString()))
                    return Results.BadRequest(new { code = "srm_supplier_type_required" });
                if (metadata.TryGetProperty("countryId", out var countryId) &&
                    countryId.ValueKind is not (JsonValueKind.Null or JsonValueKind.Undefined))
                {
                    if (countryId.ValueKind != JsonValueKind.String ||
                        !Guid.TryParse(countryId.GetString(), out var parsedCountry))
                        return Results.BadRequest(new { code = "srm_invalid_country" });
                    var countryGroup = await groups.GetBySlugAsync("countries", ct);
                    var country = await items.GetByIdAsync(parsedCountry, ct);
                    if (countryGroup is null || country is null ||
                        country.IsDeleted || !country.IsActive ||
                        country.CatalogGroupId != countryGroup.Id)
                        return Results.BadRequest(new { code = "srm_invalid_country" });
                }
                foreach (var (property, limit) in new[] {
                    ("taxId", 80), ("website", 200), ("contactName", 140),
                    ("contactEmail", 180), ("contactPhone", 40) })
                {
                    if (metadata.TryGetProperty(property, out var value) &&
                        value.ValueKind is not (JsonValueKind.Null or JsonValueKind.Undefined) &&
                        (value.ValueKind != JsonValueKind.String ||
                         value.GetString()!.Length > limit))
                        return Results.BadRequest(new { code = "srm_invalid_supplier_field", property });
                }
                return null;
            }

            if (!metadata.TryGetProperty("supplierId", out var supplierId) ||
                supplierId.ValueKind != JsonValueKind.String ||
                !Guid.TryParse(supplierId.GetString(), out var parsedId))
                return Results.BadRequest(new { code = "srm_supplier_required" });

            var supplierGroup = await groups.GetBySlugAsync(Suppliers, ct);
            var supplier = await items.GetByIdAsync(parsedId, ct);
            if (supplierGroup is null || supplier is null || supplier.IsDeleted ||
                !supplier.IsActive || supplier.CatalogGroupId != supplierGroup.Id)
                return Results.BadRequest(new { code = "srm_supplier_not_found" });

            if (slug == Evaluations)
            {
                if (!metadata.TryGetProperty("score", out var score) ||
                    score.ValueKind != JsonValueKind.Number ||
                    !score.TryGetInt32(out var value) || value is < 1 or > 5)
                    return Results.BadRequest(new { code = "srm_invalid_score" });
            }
            else if (slug == Contracts)
            {
                if (!metadata.TryGetProperty("expiresAt", out var expiry) ||
                    expiry.ValueKind != JsonValueKind.String ||
                    !DateOnly.TryParseExact(expiry.GetString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                    return Results.BadRequest(new { code = "srm_invalid_expiry" });
                if (metadata.TryGetProperty("paymentDays", out var days) &&
                    (days.ValueKind != JsonValueKind.Number ||
                     !days.TryGetInt32(out var count) || count < 0 || count > 365))
                    return Results.BadRequest(new { code = "srm_invalid_payment_days" });
                if (metadata.TryGetProperty("currencyCode", out var currency) &&
                    (currency.ValueKind != JsonValueKind.String ||
                     currency.GetString() is not ("USD" or "CRC" or "EUR")))
                    return Results.BadRequest(new { code = "srm_invalid_currency" });
                foreach (var (property, limit) in new[] {
                    ("rateReference", 160), ("serviceCoverage", 180) })
                {
                    if (metadata.TryGetProperty(property, out var value) &&
                        value.ValueKind is not (JsonValueKind.Null or JsonValueKind.Undefined) &&
                        (value.ValueKind != JsonValueKind.String ||
                         value.GetString()!.Length > limit))
                        return Results.BadRequest(new { code = "srm_invalid_contract_field", property });
                }
            }

            return null;
        }
        catch (JsonException)
        {
            return Results.BadRequest(new { code = "srm_invalid_metadata" });
        }
        finally
        {
            doc?.Dispose();
        }
    }

    private sealed record SrmRecordRequest(
        string Name, string? Description, string? Value,
        string? MetadataJson, int SortOrder);
}
