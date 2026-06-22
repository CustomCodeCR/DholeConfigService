using Dhole.Config.Application.Abstractions.Cache;

namespace Dhole.Config.Worker.Streams;

internal sealed class CatalogItemSortOrderChangedStreamHandler(
    IConfigCacheService cache,
    ILogger<CatalogItemSortOrderChangedStreamHandler> logger
) : ConfigCacheInvalidationStreamHandlerBase(cache, logger)
{
    public override string MessageType => "config.catalog-item.sort-order-changed";
}
