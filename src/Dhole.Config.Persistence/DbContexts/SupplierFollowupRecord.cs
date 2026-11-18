namespace Dhole.Config.Persistence.DbContexts;

/// <summary>
/// Append-only supplier contact history. Separate from mutable supplier catalog metadata.
/// The author and creation date are always assigned by the authenticated API.
/// </summary>
public sealed class SupplierFollowupRecord
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public string Kind { get; set; } = "";
    public string Title { get; set; } = "";
    public string Details { get; set; } = "";
    public DateTime? NextActionAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string CreatedById { get; set; } = "";
    public string CreatedByName { get; set; } = "";
}
