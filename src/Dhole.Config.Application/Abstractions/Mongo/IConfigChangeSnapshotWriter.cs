namespace Dhole.Config.Application.Abstractions.Mongo;

public interface IConfigChangeSnapshotWriter
{
    Task WriteAsync(
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
    );
}
