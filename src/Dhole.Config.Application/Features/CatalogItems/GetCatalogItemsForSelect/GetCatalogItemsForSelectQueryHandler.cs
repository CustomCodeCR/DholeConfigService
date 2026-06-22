using CustomCodeFramework.Cqrs.Queries;
using Dhole.Config.Application.Abstractions.Cache;
using Dhole.Config.Application.Abstractions.Repositories;
using Dhole.Config.Contracts.Catalogs;

namespace Dhole.Config.Application.CatalogItems.GetCatalogItemsForSelect;

public sealed class GetCatalogItemsForSelectQueryHandler(
    ICatalogItemRepository catalogItems,
    IConfigCacheService cache
) : IQueryHandler<GetCatalogItemsForSelectQuery, IReadOnlyCollection<CatalogItemSelectDto>>
{
    public async Task<IReadOnlyCollection<CatalogItemSelectDto>> HandleAsync(
        GetCatalogItemsForSelectQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var canUseCache =
            query.CatalogGroupId is null
            && !string.IsNullOrWhiteSpace(query.CatalogGroupSlug)
            && string.IsNullOrWhiteSpace(query.Search);

        if (canUseCache)
        {
            var cached = await cache.GetCatalogItemsSelectByGroupSlugAsync(
                query.CatalogGroupSlug!,
                cancellationToken
            );

            if (cached is not null)
            {
                return cached;
            }
        }

        var items = await catalogItems.GetForSelectAsync(
            query.CatalogGroupId,
            query.CatalogGroupSlug,
            query.Search,
            cancellationToken
        );

        if (canUseCache)
        {
            await cache.SetCatalogItemsSelectByGroupSlugAsync(
                query.CatalogGroupSlug!,
                items,
                cancellationToken: cancellationToken
            );
        }

        return items;
    }
}
