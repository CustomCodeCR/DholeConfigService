using Dhole.Config.Application.Abstractions.Cache;

namespace Dhole.Config.Worker.Streams;

internal sealed class CatalogItemInactivatedStreamHandler(
    IConfigCacheService cache,
    ILogger<CatalogItemInactivatedStreamHandler> logger
) : ConfigCacheInvalidationStreamHandlerBase(cache, logger)
{
    public override string MessageType => "config.catalog-item.inactivated";
}
