using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Config.Contracts.Catalogs;

namespace Dhole.Config.Application.CatalogItems.GetCatalogItems;

public sealed record GetCatalogItemsQuery(
    PageRequest Page,
    Guid? CatalogGroupId,
    string? Search,
    bool? IsActive
) : IQuery<PagedResult<CatalogItemDto>>;
