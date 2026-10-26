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
