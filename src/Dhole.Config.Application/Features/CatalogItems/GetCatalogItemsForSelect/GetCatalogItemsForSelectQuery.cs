using CustomCodeFramework.Cqrs.Queries;
using Dhole.Config.Contracts.Catalogs;

namespace Dhole.Config.Application.CatalogItems.GetCatalogItemsForSelect;

public sealed record GetCatalogItemsForSelectQuery(
    Guid? CatalogGroupId,
    string? CatalogGroupSlug,
    string? Search
) : IQuery<IReadOnlyCollection<CatalogItemSelectDto>>;
