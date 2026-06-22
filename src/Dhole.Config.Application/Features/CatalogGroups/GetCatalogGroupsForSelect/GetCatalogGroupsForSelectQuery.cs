using CustomCodeFramework.Cqrs.Queries;
using Dhole.Config.Contracts.Catalogs;

namespace Dhole.Config.Application.CatalogGroups.GetCatalogGroupsForSelect;

public sealed record GetCatalogGroupsForSelectQuery(string? Search)
    : IQuery<IReadOnlyCollection<CatalogGroupSelectDto>>;
