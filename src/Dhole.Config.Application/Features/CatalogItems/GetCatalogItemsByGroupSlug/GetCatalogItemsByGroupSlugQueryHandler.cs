using CustomCodeFramework.Cqrs.Queries;
using Dhole.Config.Application.Abstractions.Cache;
using Dhole.Config.Application.Abstractions.Repositories;
using Dhole.Config.Contracts.Catalogs;

namespace Dhole.Config.Application.CatalogItems.GetCatalogItemsByGroupSlug;

public sealed class GetCatalogItemsByGroupSlugQueryHandler(
    ICatalogItemRepository catalogItems,
    IConfigCacheService cache
) : IQueryHandler<GetCatalogItemsByGroupSlugQuery, IReadOnlyCollection<CatalogItemDto>>
{
    public async Task<IReadOnlyCollection<CatalogItemDto>> HandleAsync(
        GetCatalogItemsByGroupSlugQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var cached = await cache.GetCatalogItemsByGroupSlugAsync(
            query.CatalogGroupSlug,
            cancellationToken
        );

        if (cached is not null)
        {
            return cached;
        }

        var items = await catalogItems.GetActiveByGroupSlugAsync(
            query.CatalogGroupSlug,
            cancellationToken
        );

        await cache.SetCatalogItemsByGroupSlugAsync(
            query.CatalogGroupSlug,
            items,
            cancellationToken: cancellationToken
        );

        return items;
    }
}
