using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Config.Application.Abstractions.Repositories;
using Dhole.Config.Contracts.Catalogs;

namespace Dhole.Config.Application.CatalogItems.GetCatalogItems;

public sealed class GetCatalogItemsQueryHandler(ICatalogItemRepository catalogItems)
    : IQueryHandler<GetCatalogItemsQuery, PagedResult<CatalogItemDto>>
{
    public Task<PagedResult<CatalogItemDto>> HandleAsync(
        GetCatalogItemsQuery query,
        CancellationToken cancellationToken = default
    )
    {
        return catalogItems.GetPagedAsync(
            query.Page,
            query.CatalogGroupId,
            query.Search,
            query.IsActive,
            cancellationToken
        );
    }
}
