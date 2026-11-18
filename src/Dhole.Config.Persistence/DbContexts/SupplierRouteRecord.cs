namespace Dhole.Config.Persistence.DbContexts;

/// <summary>SRM route coverage. Each record represents the Cartesian coverage of its origins and destinations.</summary>
public sealed class SupplierRouteRecord
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public string TransportMode { get; set; } = "";
    public Guid[] OriginIds { get; set; } = [];
    public Guid[] DestinationIds { get; set; } = [];
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public string CreatedById { get; set; } = "";
    public string UpdatedById { get; set; } = "";
}

/// <summary>Immutable route mutation history for audit and incident review.</summary>
public sealed class SupplierRouteAuditRecord
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public Guid RouteId { get; set; }
    public string Action { get; set; } = "";
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string ActorId { get; set; } = "";
}
