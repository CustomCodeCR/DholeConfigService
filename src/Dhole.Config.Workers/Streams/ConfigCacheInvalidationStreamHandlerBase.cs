using System.Text.Json;
using CustomCodeFramework.Redis.Streams.Abstractions;
using CustomCodeFramework.Redis.Streams.Messages;
using Dhole.Config.Application.Abstractions.Cache;

namespace Dhole.Config.Worker.Streams;

internal abstract class ConfigCacheInvalidationStreamHandlerBase(
    IConfigCacheService cache,
    ILogger logger
) : IRedisStreamMessageHandler
{
    public abstract string MessageType { get; }

    public async Task HandleAsync(
        RedisStreamEnvelope envelope,
        CancellationToken cancellationToken = default
    )
    {
        var catalogGroupSlug = TryGetCatalogGroupSlug(envelope);

        if (string.IsNullOrWhiteSpace(catalogGroupSlug))
        {
            await cache.RemoveCatalogGroupsSelectAsync(cancellationToken);

            logger.LogWarning(
                "Config cache invalidation received {MessageType} with id {MessageId}, but no catalogGroupSlug was found. Only catalog groups select cache was removed.",
                envelope.MessageType,
                envelope.MessageId
            );

            return;
        }

        await cache.RemoveCatalogGroupCacheAsync(catalogGroupSlug, cancellationToken);

        logger.LogInformation(
            "Config cache invalidated for catalog group slug {CatalogGroupSlug}. Event: {MessageType}, MessageId: {MessageId}.",
            catalogGroupSlug,
            envelope.MessageType,
            envelope.MessageId
        );
    }

    private static string? TryGetCatalogGroupSlug(RedisStreamEnvelope envelope)
    {
        var payloadJson = envelope.PayloadJson;

        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            return null;
        }

        using var document = JsonDocument.Parse(payloadJson);
        var root = document.RootElement;

        return TryGetString(root, "catalogGroupSlug")
            ?? TryGetString(root, "CatalogGroupSlug")
            ?? TryGetString(root, "slug")
            ?? TryGetString(root, "Slug");
    }

    private static string? TryGetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.String ? property.GetString() : null;
    }
}
