using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Config.Contracts.Catalogs;

namespace Dhole.Config.Application.CatalogGroups.GetCatalogGroupById;

public sealed record GetCatalogGroupByIdQuery(Guid Id) : IQuery<Result<CatalogGroupDto>>;
