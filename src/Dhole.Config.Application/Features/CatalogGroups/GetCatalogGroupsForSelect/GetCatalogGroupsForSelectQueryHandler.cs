using CustomCodeFramework.Cqrs.Queries;
using Dhole.Config.Application.Abstractions.Cache;
using Dhole.Config.Application.Abstractions.Repositories;
using Dhole.Config.Contracts.Catalogs;

namespace Dhole.Config.Application.CatalogGroups.GetCatalogGroupsForSelect;

public sealed class GetCatalogGroupsForSelectQueryHandler(
    ICatalogGroupRepository catalogGroups,
    IConfigCacheService cache
) : IQueryHandler<GetCatalogGroupsForSelectQuery, IReadOnlyCollection<CatalogGroupSelectDto>>
{
    public async Task<IReadOnlyCollection<CatalogGroupSelectDto>> HandleAsync(
        GetCatalogGroupsForSelectQuery query,
        CancellationToken cancellationToken = default
    )
    {
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            return await catalogGroups.GetForSelectAsync(query.Search, cancellationToken);
        }

        var cached = await cache.GetCatalogGroupsSelectAsync(cancellationToken);

        if (cached is not null)
        {
            return cached;
        }

        var items = await catalogGroups.GetForSelectAsync(null, cancellationToken);

        await cache.SetCatalogGroupsSelectAsync(items, cancellationToken: cancellationToken);

        return items;
    }
}
