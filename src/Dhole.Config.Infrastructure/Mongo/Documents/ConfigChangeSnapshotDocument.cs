using CustomCodeFramework.Mongo.Abstractions;
using CustomCodeFramework.Mongo.Collections;

namespace Dhole.Config.Infrastructure.Mongo.Documents;

[MongoCollectionName("config_change_snapshots")]
public sealed class ConfigChangeSnapshotDocument : IReadModel
{
    public string Id { get; init; } = Guid.NewGuid().ToString();

    public string EventId { get; init; } = default!;

    public string EventName { get; init; } = default!;

    public string EntityType { get; init; } = default!;

    public string EntityId { get; init; } = default!;

    public string EntityCode { get; init; } = default!;

    public string EntitySlug { get; init; } = default!;

    public string? CatalogGroupId { get; init; }

    public string? CatalogGroupCode { get; init; }

    public string? CatalogGroupSlug { get; init; }

    public string Action { get; init; } = default!;

    public string? PreviousValueJson { get; init; }

    public string? NewValueJson { get; init; }

    public string? ChangedBy { get; init; }

    public DateTime ChangedAtUtc { get; init; }

    public string? CorrelationId { get; init; }

    public string SourceService { get; init; } = "DholeConfigService";
}
