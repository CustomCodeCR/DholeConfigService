using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Config.Contracts.Catalogs;

namespace Dhole.Config.Application.CatalogGroups.GetCatalogGroups;

public sealed record GetCatalogGroupsQuery(PageRequest Page, string? Search, bool? IsActive)
    : IQuery<PagedResult<CatalogGroupDto>>;
