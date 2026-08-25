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

        // CatalogItemSelectDto.Value historically contained the slug. Pricing, however,
        // uses the catalog item's configured Value as the business value sent in rates
        // (for example Incoterms). Keep Slug available in its dedicated property and
        // hydrate Value from the canonical Config item whenever the group is known.
        if (!string.IsNullOrWhiteSpace(query.CatalogGroupSlug) && items.Count > 0)
        {
            var canonicalItems = await catalogItems.GetActiveByGroupSlugAsync(
                query.CatalogGroupSlug!,
                cancellationToken
            );
            var canonicalById = canonicalItems.ToDictionary(item => item.Id);

            items = items
                .Select(item =>
                {
                    if (!canonicalById.TryGetValue(item.Id, out var canonical))
                    {
                        return item;
                    }

                    return item with
                    {
                        Value = string.IsNullOrWhiteSpace(canonical.Value)
                            ? item.Slug
                            : canonical.Value.Trim(),
                    };
                })
                .ToArray();
        }

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
