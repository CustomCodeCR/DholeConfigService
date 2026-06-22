using Dhole.Config.Application.Abstractions.Cache;

namespace Dhole.Config.Worker.Streams;

internal sealed class CatalogGroupCreatedStreamHandler(
    IConfigCacheService cache,
    ILogger<CatalogGroupCreatedStreamHandler> logger
) : ConfigCacheInvalidationStreamHandlerBase(cache, logger)
{
    public override string MessageType => "config.catalog-group.created";
}
