using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Config.Application.Abstractions.Repositories;
using Dhole.Config.Contracts.Catalogs;

namespace Dhole.Config.Application.CatalogGroups.GetCatalogGroups;

public sealed class GetCatalogGroupsQueryHandler(ICatalogGroupRepository catalogGroups)
    : IQueryHandler<GetCatalogGroupsQuery, PagedResult<CatalogGroupDto>>
{
    public Task<PagedResult<CatalogGroupDto>> HandleAsync(
        GetCatalogGroupsQuery query,
        CancellationToken cancellationToken = default
    )
    {
        return catalogGroups.GetPagedAsync(
            query.Page,
            query.Search,
            query.IsActive,
            cancellationToken
        );
    }
}
