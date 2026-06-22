using CustomCodeFramework.Mongo.Abstractions;
using Dhole.Config.Application.Abstractions.Mongo;
using Dhole.Config.Infrastructure.Mongo.Documents;

namespace Dhole.Config.Infrastructure.Mongo;

public sealed class ConfigChangeSnapshotWriter(IMongoContext mongoContext)
    : IConfigChangeSnapshotWriter
{
    public Task WriteAsync(
        Guid eventId,
        string eventName,
        string entityType,
        Guid entityId,
        string entityCode,
        string entitySlug,
        Guid? catalogGroupId,
        string? catalogGroupCode,
        string? catalogGroupSlug,
        string action,
        string? previousValueJson,
        string? newValueJson,
        Guid? changedBy,
        DateTime changedAtUtc,
        Guid? correlationId,
        CancellationToken cancellationToken = default
    )
    {
        var document = new ConfigChangeSnapshotDocument
        {
            EventId = eventId.ToString(),
            EventName = eventName,

            EntityType = entityType,
            EntityId = entityId.ToString(),
            EntityCode = entityCode,
            EntitySlug = entitySlug,

            CatalogGroupId = catalogGroupId?.ToString(),
            CatalogGroupCode = catalogGroupCode,
            CatalogGroupSlug = catalogGroupSlug,

            Action = action,
            PreviousValueJson = previousValueJson,
            NewValueJson = newValueJson,

            ChangedBy = changedBy?.ToString(),
            ChangedAtUtc = changedAtUtc,
            CorrelationId = correlationId?.ToString(),
        };

        return mongoContext
            .GetCollection<ConfigChangeSnapshotDocument>()
            .InsertOneAsync(document, cancellationToken: cancellationToken);
    }
}
