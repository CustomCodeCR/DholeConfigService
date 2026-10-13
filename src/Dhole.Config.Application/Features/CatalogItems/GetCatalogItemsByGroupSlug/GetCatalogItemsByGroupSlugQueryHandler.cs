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
        // WHS are also maintained through SQL seed migrations; always read the current directory.
        if (string.Equals(query.CatalogGroupSlug, "pricing-warehouses", StringComparison.OrdinalIgnoreCase))
        {
            return await catalogItems.GetActiveByGroupSlugAsync(query.CatalogGroupSlug, cancellationToken);
        }

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
