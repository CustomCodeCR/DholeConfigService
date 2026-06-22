using Dhole.Config.Application.Abstractions.Cache;

namespace Dhole.Config.Worker.Streams;

internal sealed class CatalogItemCreatedStreamHandler(
    IConfigCacheService cache,
    ILogger<CatalogItemCreatedStreamHandler> logger
) : ConfigCacheInvalidationStreamHandlerBase(cache, logger)
{
    public override string MessageType => "config.catalog-item.created";
}
