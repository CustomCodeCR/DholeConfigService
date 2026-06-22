using Dhole.Config.Application.Abstractions.Cache;

namespace Dhole.Config.Worker.Streams;

internal sealed class CatalogGroupActivatedStreamHandler(
    IConfigCacheService cache,
    ILogger<CatalogGroupActivatedStreamHandler> logger
) : ConfigCacheInvalidationStreamHandlerBase(cache, logger)
{
    public override string MessageType => "config.catalog-group.activated";
}
