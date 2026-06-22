using Dhole.Config.Application.Abstractions.Cache;

namespace Dhole.Config.Worker.Streams;

internal sealed class CatalogGroupInactivatedStreamHandler(
    IConfigCacheService cache,
    ILogger<CatalogGroupInactivatedStreamHandler> logger
) : ConfigCacheInvalidationStreamHandlerBase(cache, logger)
{
    public override string MessageType => "config.catalog-group.inactivated";
}
