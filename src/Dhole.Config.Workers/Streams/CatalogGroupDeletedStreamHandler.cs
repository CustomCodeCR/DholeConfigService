using Dhole.Config.Application.Abstractions.Cache;

namespace Dhole.Config.Worker.Streams;

internal sealed class CatalogGroupDeletedStreamHandler(
    IConfigCacheService cache,
    ILogger<CatalogGroupDeletedStreamHandler> logger
) : ConfigCacheInvalidationStreamHandlerBase(cache, logger)
{
    public override string MessageType => "config.catalog-group.deleted";
}
