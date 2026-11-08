using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Dhole.Config.Persistence.Messaging;

/// <summary>
/// F6.05: Config/SRM invalidations only; never transport catalog metadata,
/// supplier details, contacts, access keys or changes to privilege sets.
/// </summary>
public static class ConfigRealtimeInvalidation
{
    public const string ReleaseId = "rt-20261107-01";
    public const string StreamName = "dhole.config.realtime.v1";

    public static bool IsEnabled(IConfiguration? config) =>
        config?.GetValue<bool>("Features:UniversalRealtimeEnabled") == true &&
        config.GetValue<bool>("Realtime:Outbox:Enabled") &&
        config.GetValue<bool>("Realtime:BatchApproved") &&
        string.Equals(config["Realtime:ReleaseId"], ReleaseId, StringComparison.Ordinal);

    public static string EventType(string resourceType) => resourceType switch
    {
        "catalog-item" => "config.catalog-item.changed.v1",
        "catalog-group" => "config.catalog-group.changed.v1",
        _ => throw new ArgumentException("Unknown Config realtime resource.", nameof(resourceType))
    };

    public static string Serialize(Guid eventId, Guid entityId, string kind, long version,
        DateTimeOffset at, string correlationId)
    {
        if (eventId == Guid.Empty || entityId == Guid.Empty ||
            version <= 0 || at.Offset != TimeSpan.Zero ||
            string.IsNullOrWhiteSpace(correlationId))
            throw new ArgumentException("Invalid Config realtime envelope.");
        var eventType = EventType(kind);
        return JsonSerializer.Serialize(new
        {
            eventId,
            eventType,
            schemaVersion = 1,
            aggregateType = kind,
            aggregateId = entityId.ToString("D"),
            aggregateVersion = version,
            occurredAtUtc = at,
            producer = "DholeConfigService",
            correlationId,
            audienceRef = $"config:{kind}:{entityId:D}",
            payload = new { changedFields = new[] { "catalog" } }
        });
    }
}
