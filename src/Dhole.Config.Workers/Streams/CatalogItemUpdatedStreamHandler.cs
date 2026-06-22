using Dhole.Config.Application.Abstractions.Cache;

namespace Dhole.Config.Worker.Streams;

internal sealed class CatalogItemUpdatedStreamHandler(
    IConfigCacheService cache,
    ILogger<CatalogItemUpdatedStreamHandler> logger
) : ConfigCacheInvalidationStreamHandlerBase(cache, logger)
{
    public override string MessageType => "config.catalog-item.updated";
}
