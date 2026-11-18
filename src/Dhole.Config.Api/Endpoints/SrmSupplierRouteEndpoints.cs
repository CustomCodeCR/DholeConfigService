using System.Security.Claims;
using System.Text.Json;
using CustomCodeFramework.Messaging.Outbox;
using Dhole.Config.Api.Authorization;
using Dhole.Config.Application.Abstractions.Repositories;
using Dhole.Config.Persistence.DbContexts;
using Dhole.Config.Persistence.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Config.Api.Endpoints;

/// <summary>
/// SRM route coverage is durable and independent of editable supplier metadata.
/// Every origin/destination pair in one record is a valid serviceable combination.
/// </summary>
internal static class SrmSupplierRouteEndpoints
{
    internal sealed record RouteInput(string TransportMode, Guid[] OriginIds, Guid[] DestinationIds);
    private static readonly string[] AllowedModes = ["maritime", "air", "land"];

    public static RouteGroupBuilder MapSrmSupplierRouteEndpoints(this RouteGroupBuilder routes)
    {
        routes.MapGet("/suppliers/{supplierId:guid}/routes", Read)
            .RequireScope("srm.supplier.read");
        routes.MapPost("/suppliers/{supplierId:guid}/routes", Create)
            .RequireScope("srm.supplier.update");
        routes.MapPut("/suppliers/{supplierId:guid}/routes/{routeId:guid}", Update)
            .RequireScope("srm.supplier.update");
        routes.MapDelete("/suppliers/{supplierId:guid}/routes/{routeId:guid}", Delete)
            .RequireScope("srm.supplier.update");

        // Pricing can discover coverage without receiving contacts, fiscal data or banking information.
        routes.MapGet("/pricing-supplier-coverage", PricingCoverage)
            .RequireScope("pricing.cost.view");
        return routes;
    }

    private static async Task<bool> SupplierExists(
        Guid supplierId, ICatalogGroupRepository groups, ICatalogItemRepository items,
        CancellationToken ct)
    {
        var group = await groups.GetBySlugAsync("srm-suppliers", ct);
        var supplier = await items.GetByIdAsync(supplierId, ct);
        return group is not null && supplier is not null && !supplier.IsDeleted &&
            supplier.CatalogGroupId == group.Id;
    }

    private static async Task<IResult?> Validate(
        RouteInput input, ICatalogGroupRepository groups, ICatalogItemRepository items,
        CancellationToken ct)
    {
        var mode = input.TransportMode?.Trim().ToLowerInvariant() ?? "";
        if (!AllowedModes.Contains(mode))
            return Results.BadRequest(new { code = "srm_route_invalid_mode" });
        if (input.OriginIds is null || input.DestinationIds is null ||
            input.OriginIds.Length is < 1 or > 30 ||
            input.DestinationIds.Length is < 1 or > 30 ||
            input.OriginIds.Length * input.DestinationIds.Length > 400 ||
            input.OriginIds.Any(id => id == Guid.Empty) ||
            input.DestinationIds.Any(id => id == Guid.Empty) ||
            input.OriginIds.Distinct().Count() != input.OriginIds.Length ||
            input.DestinationIds.Distinct().Count() != input.DestinationIds.Length)
            return Results.BadRequest(new { code = "srm_route_invalid_points" });

        foreach (var (locationIds, groupSlugs) in new[]
        {
            (input.OriginIds, new[] { "pol" }),
            (input.DestinationIds, new[] { "poe", "pod" })
        })
        {
            var groupIds = new HashSet<Guid>();
            foreach (var slug in groupSlugs)
            {
                var group = await groups.GetBySlugAsync(slug, ct);
                if (group is not null && !group.IsDeleted && group.IsActive) groupIds.Add(group.Id);
            }

            foreach (var id in locationIds)
            {
                var location = await items.GetByIdAsync(id, ct);
                if (location is null || location.IsDeleted || !location.IsActive ||
                    !groupIds.Contains(location.CatalogGroupId))
                    return Results.BadRequest(new { code = "srm_route_point_not_in_catalog", id });
                try
                {
                    using var document = JsonDocument.Parse(location.MetadataJson ?? "{}");
                    var metadata = document.RootElement;
                    var transport = metadata.TryGetProperty("transportMode", out var transportNode) &&
                        transportNode.ValueKind == JsonValueKind.String
                        ? transportNode.GetString()?.ToLowerInvariant() : null;
                    var terminal = metadata.TryGetProperty("terminalType", out var terminalNode) &&
                        terminalNode.ValueKind == JsonValueKind.String
                        ? terminalNode.GetString()?.ToUpperInvariant() : null;
                    if (transport is "maritime" or "land" or "air" && transport != mode)
                        return Results.BadRequest(new { code = "srm_route_mode_point_mismatch", id });
                    // Older entries can lack a mode but have a known terminal type.
                    if (transport is null && terminal is "APT" or "CY" or "SD" &&
                        (terminal == "APT" ? "air" : terminal == "CY" ? "maritime" : "land") != mode)
                        return Results.BadRequest(new { code = "srm_route_mode_point_mismatch", id });
                }
                catch (JsonException)
                {
                    return Results.BadRequest(new { code = "srm_route_invalid_location_metadata", id });
                }
            }
        }
        return null;
    }

    private static string Actor(HttpContext context)
    {
        var userId = context.User.FindFirst("sub")?.Value ??
            context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
        return userId.Length > 160 ? userId[..160] : userId;
    }

    private static void RecordMutation(ServiceDbContext db, IConfiguration configuration,
        HttpContext context, Guid supplierId, Guid routeId,
        string action, SupplierRouteRecord? before, SupplierRouteRecord? after)
    {
        db.SupplierRouteAudits.Add(new SupplierRouteAuditRecord
        {
            Id = Guid.NewGuid(), SupplierId = supplierId, RouteId = routeId, Action = action,
            BeforeJson = before is null ? null : JsonSerializer.Serialize(before),
            AfterJson = after is null ? null : JsonSerializer.Serialize(after),
            CreatedAtUtc = DateTime.UtcNow, ActorId = Actor(context)
        });
        if (!ConfigRealtimeInvalidation.IsEnabled(configuration)) return;
        var eventId = Guid.NewGuid();
        var correlationId = Guid.NewGuid().ToString();
        db.OutboxMessages.Add(new OutboxMessage
        {
            EventId = eventId,
            EventType = ConfigRealtimeInvalidation.EventType("srm-supplier"),
            EventName = "srm.supplier.changed.v1",
            SourceService = "DholeConfigService",
            PayloadJson = ConfigRealtimeInvalidation.Serialize(eventId, supplierId,
                "srm-supplier", ConfigRealtimeInvalidation.NextVersionHint(),
                DateTimeOffset.UtcNow, correlationId),
            CorrelationId = correlationId,
            Status = OutboxMessageStatus.Pending, RetryCount = 0,
            CreatedAtUtc = DateTime.UtcNow
        });
    }

    private static async Task<IResult> Read(
        Guid supplierId, ICatalogGroupRepository groups, ICatalogItemRepository items,
        ServiceDbContext db, HttpContext context, CancellationToken ct)
    {
        context.Response.Headers["Cache-Control"] = "no-store";
        if (!await SupplierExists(supplierId, groups, items, ct))
            return Results.NotFound(new { code = "srm_supplier_not_found" });
        return Results.Ok(await db.SupplierRoutes.AsNoTracking()
            .Where(x => x.SupplierId == supplierId)
            .OrderBy(x => x.TransportMode).ThenBy(x => x.CreatedAtUtc)
            .ToListAsync(ct));
    }

    private static async Task<IResult> Create(
        Guid supplierId, RouteInput input,
        ICatalogGroupRepository groups, ICatalogItemRepository items,
        ServiceDbContext db, IConfiguration configuration,
        HttpContext context, CancellationToken ct)
    {
        if (!await SupplierExists(supplierId, groups, items, ct))
            return Results.NotFound(new { code = "srm_supplier_not_found" });
        var invalid = await Validate(input, groups, items, ct);
        if (invalid is not null) return invalid;
        var mode = input.TransportMode.Trim().ToLowerInvariant();
        var originIds = input.OriginIds.Order().ToArray();
        var destinationIds = input.DestinationIds.Order().ToArray();
        var existing = await db.SupplierRoutes.AsNoTracking()
            .Where(x => x.SupplierId == supplierId && x.TransportMode == mode).ToListAsync(ct);
        if (existing.Any(x => x.OriginIds.Order().SequenceEqual(originIds) &&
                              x.DestinationIds.Order().SequenceEqual(destinationIds)))
            return Results.Conflict(new { code = "srm_duplicate_supplier_route" });
        var now = DateTime.UtcNow;
        var route = new SupplierRouteRecord
        {
            Id = Guid.NewGuid(), SupplierId = supplierId, TransportMode = mode,
            OriginIds = originIds, DestinationIds = destinationIds,
            CreatedAtUtc = now, UpdatedAtUtc = now,
            CreatedById = Actor(context), UpdatedById = Actor(context)
        };
        db.SupplierRoutes.Add(route);
        RecordMutation(db, configuration, context, supplierId, route.Id, "create", null, route);
        await db.SaveChangesAsync(ct);
        return Results.Created($"/api/config/srm/suppliers/{supplierId}/routes/{route.Id}", route);
    }

    private static async Task<IResult> Update(
        Guid supplierId, Guid routeId, RouteInput input,
        ICatalogGroupRepository groups, ICatalogItemRepository items,
        ServiceDbContext db, IConfiguration configuration,
        HttpContext context, CancellationToken ct)
    {
        if (!await SupplierExists(supplierId, groups, items, ct))
            return Results.NotFound(new { code = "srm_supplier_not_found" });
        var route = await db.SupplierRoutes.SingleOrDefaultAsync(
            x => x.Id == routeId && x.SupplierId == supplierId, ct);
        if (route is null) return Results.NotFound(new { code = "srm_route_not_found" });
        var invalid = await Validate(input, groups, items, ct);
        if (invalid is not null) return invalid;
        var before = new SupplierRouteRecord
        {
            Id = route.Id, SupplierId = route.SupplierId, TransportMode = route.TransportMode,
            OriginIds = [.. route.OriginIds], DestinationIds = [.. route.DestinationIds],
            CreatedAtUtc = route.CreatedAtUtc, UpdatedAtUtc = route.UpdatedAtUtc,
            CreatedById = route.CreatedById, UpdatedById = route.UpdatedById
        };
        route.TransportMode = input.TransportMode.Trim().ToLowerInvariant();
        route.OriginIds = input.OriginIds.Order().ToArray();
        route.DestinationIds = input.DestinationIds.Order().ToArray();
        route.UpdatedAtUtc = DateTime.UtcNow;
        route.UpdatedById = Actor(context);
        RecordMutation(db, configuration, context, supplierId, routeId, "update", before, route);
        await db.SaveChangesAsync(ct);
        return Results.Ok(route);
    }

    private static async Task<IResult> Delete(
        Guid supplierId, Guid routeId,
        ICatalogGroupRepository groups, ICatalogItemRepository items,
        ServiceDbContext db, IConfiguration configuration,
        HttpContext context, CancellationToken ct)
    {
        if (!await SupplierExists(supplierId, groups, items, ct))
            return Results.NotFound(new { code = "srm_supplier_not_found" });
        var route = await db.SupplierRoutes.SingleOrDefaultAsync(
            x => x.SupplierId == supplierId && x.Id == routeId, ct);
        if (route is null) return Results.NotFound(new { code = "srm_route_not_found" });
        RecordMutation(db, configuration, context, supplierId, routeId, "delete", route, null);
        db.SupplierRoutes.Remove(route);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> PricingCoverage(
        string? transportMode, Guid? originId, Guid? destinationId,
        ICatalogGroupRepository groups, ServiceDbContext db, CancellationToken ct)
    {
        var mode = transportMode?.Trim().ToLowerInvariant() ?? "";
        if (!AllowedModes.Contains(mode) || originId is null || destinationId is null ||
            originId == Guid.Empty || destinationId == Guid.Empty)
            return Results.BadRequest(new { code = "srm_route_filter_required" });
        var group = await groups.GetBySlugAsync("srm-suppliers", ct);
        if (group is null) return Results.Ok(Array.Empty<object>());
        var matches = await db.SupplierRoutes.AsNoTracking()
            .Where(x => x.TransportMode == mode &&
                x.OriginIds.Contains(originId.Value) && x.DestinationIds.Contains(destinationId.Value))
            .ToListAsync(ct);
        var ids = matches.Select(x => x.SupplierId).Distinct().ToArray();
        var suppliers = await db.CatalogItems.AsNoTracking()
            .Where(x => ids.Contains(x.Id) && x.CatalogGroupId == group.Id && x.IsActive && !x.IsDeleted)
            .Select(x => new { x.Id, x.Name }).ToListAsync(ct);
        var names = suppliers.ToDictionary(x => x.Id, x => x.Name);
        return Results.Ok(matches.Where(x => names.ContainsKey(x.SupplierId))
            .Select(x => new { x.SupplierId, SupplierName = names[x.SupplierId],
                RouteId = x.Id, x.TransportMode, x.OriginIds, x.DestinationIds }));
    }
}
