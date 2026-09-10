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

        IReadOnlyCollection<CatalogItemSelectDto>? items = null;

        if (canUseCache)
        {
            items = await cache.GetCatalogItemsSelectByGroupSlugAsync(
                query.CatalogGroupSlug!,
                cancellationToken
            );
        }

        items ??= await catalogItems.GetForSelectAsync(
            query.CatalogGroupId,
            query.CatalogGroupSlug,
            query.Search,
            cancellationToken
        );

        // Value is the only business/display value exposed by Config selects.
        // Always rehydrate it from the canonical catalog row, including cache hits,
        // so stale cache entries can never expose Code or Slug as the visible value.
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
                        return item with { Value = string.Empty };
                    }

                    return item with
                    {
                        Value = canonical.Value?.Trim() ?? string.Empty,
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
