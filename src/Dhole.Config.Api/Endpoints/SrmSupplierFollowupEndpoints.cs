using System.Security.Claims;
using CustomCodeFramework.Messaging.Outbox;
using Dhole.Config.Api.Authorization;
using Dhole.Config.Application.Abstractions.Repositories;
using Dhole.Config.Persistence.DbContexts;
using Dhole.Config.Persistence.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Config.Api.Endpoints;

/// <summary>Immutable and actor-attributed SRM supplier followup log.</summary>
internal static class SrmSupplierFollowupEndpoints
{
    internal sealed record FollowupInput(string? Kind, string? Title, string? Details,
        DateTimeOffset? NextActionAt);

    public static RouteGroupBuilder MapSrmSupplierFollowupEndpoints(this RouteGroupBuilder routes)
    {
        routes.MapGet("/suppliers/{supplierId:guid}/followups", Read)
            .RequireScope("srm.supplier.read");
        routes.MapPost("/suppliers/{supplierId:guid}/followups", Add)
            .RequireScope("srm.supplier.update");
        return routes;
    }

    private static async Task<bool> SupplierExists(
        Guid supplierId, ICatalogGroupRepository groups, ICatalogItemRepository items,
        CancellationToken ct)
    {
        var group = await groups.GetBySlugAsync("srm-suppliers", ct);
        var item = await items.GetByIdAsync(supplierId, ct);
        return group is not null && item is not null && !item.IsDeleted &&
            item.CatalogGroupId == group.Id;
    }

    private static async Task<IResult> Read(
        Guid supplierId, ICatalogGroupRepository groups, ICatalogItemRepository items,
        ServiceDbContext db, HttpContext context, CancellationToken ct)
    {
        context.Response.Headers["Cache-Control"] = "no-store";
        if (!await SupplierExists(supplierId, groups, items, ct))
            return Results.NotFound(new { code = "srm_supplier_not_found" });
        var entries = await db.SupplierFollowups.AsNoTracking()
            .Where(row => row.SupplierId == supplierId)
            .OrderByDescending(row => row.CreatedAtUtc).ThenByDescending(row => row.Id)
            .Take(250).ToListAsync(ct);
        return Results.Ok(entries);
    }

    private static async Task<IResult> Add(
        Guid supplierId, FollowupInput input,
        ICatalogGroupRepository groups, ICatalogItemRepository items,
        ServiceDbContext db, IConfiguration configuration, HttpContext context,
        CancellationToken ct)
    {
        if (!await SupplierExists(supplierId, groups, items, ct))
            return Results.NotFound(new { code = "srm_supplier_not_found" });
        var kind = (input.Kind ?? "").Trim().ToLowerInvariant();
        var title = (input.Title ?? "").Trim();
        var details = (input.Details ?? "").Trim();
        if (kind is not ("call" or "email" or "meeting" or "visit" or "note" or "other") ||
            title.Length is < 1 or > 200 || details.Length > 4000)
            return Results.BadRequest(new { code = "srm_invalid_followup" });
        var userId = context.User.FindFirst("sub")?.Value ??
            context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
        var actorName = context.User.Identity?.Name ??
            context.User.FindFirst("preferred_username")?.Value ?? userId;
        var entry = new SupplierFollowupRecord
        {
            Id = Guid.NewGuid(),
            SupplierId = supplierId,
            Kind = kind,
            Title = title,
            Details = details,
            NextActionAtUtc = input.NextActionAt?.UtcDateTime,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedById = userId.Length > 160 ? userId[..160] : userId,
            CreatedByName = actorName.Length > 200 ? actorName[..200] : actorName
        };
        db.SupplierFollowups.Add(entry);
        if (ConfigRealtimeInvalidation.IsEnabled(configuration))
        {
            var eventId = Guid.NewGuid();
            var correlationId = Guid.NewGuid().ToString();
            var eventType = ConfigRealtimeInvalidation.EventType("srm-supplier");
            db.OutboxMessages.Add(new OutboxMessage
            {
                EventId = eventId,
                EventType = eventType,
                EventName = "srm.supplier.changed.v1",
                SourceService = "DholeConfigService",
                PayloadJson = ConfigRealtimeInvalidation.Serialize(
                    eventId, supplierId, "srm-supplier",
                    ConfigRealtimeInvalidation.NextVersionHint(), DateTimeOffset.UtcNow, correlationId),
                CorrelationId = correlationId,
                Status = OutboxMessageStatus.Pending,
                RetryCount = 0,
                CreatedAtUtc = DateTime.UtcNow
            });
        }
        await db.SaveChangesAsync(ct);
        return Results.Created($"/api/config/srm/suppliers/{supplierId}/followups/{entry.Id}", entry);
    }
}
