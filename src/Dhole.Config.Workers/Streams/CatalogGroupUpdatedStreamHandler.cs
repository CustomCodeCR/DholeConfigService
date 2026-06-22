using Dhole.Config.Application.Abstractions.Cache;

namespace Dhole.Config.Worker.Streams;

internal sealed class CatalogGroupUpdatedStreamHandler(
    IConfigCacheService cache,
    ILogger<CatalogGroupUpdatedStreamHandler> logger
) : ConfigCacheInvalidationStreamHandlerBase(cache, logger)
{
    public override string MessageType => "config.catalog-group.updated";
}
