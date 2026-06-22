using CustomCodeFramework.Cqrs.Queries;
using Dhole.Config.Contracts.Catalogs;

namespace Dhole.Config.Application.CatalogItems.GetCatalogItemsByGroupSlug;

public sealed record GetCatalogItemsByGroupSlugQuery(string CatalogGroupSlug)
    : IQuery<IReadOnlyCollection<CatalogItemDto>>;
