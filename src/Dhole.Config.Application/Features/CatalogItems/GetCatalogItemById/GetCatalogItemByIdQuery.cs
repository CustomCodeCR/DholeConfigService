using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Config.Contracts.Catalogs;

namespace Dhole.Config.Application.CatalogItems.GetCatalogItemById;

public sealed record GetCatalogItemByIdQuery(Guid Id) : IQuery<Result<CatalogItemDto>>;
