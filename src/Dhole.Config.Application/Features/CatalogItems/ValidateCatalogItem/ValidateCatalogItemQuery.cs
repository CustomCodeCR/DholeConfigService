using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Config.Contracts.Catalogs;

namespace Dhole.Config.Application.CatalogItems.ValidateCatalogItem;

public sealed record ValidateCatalogItemQuery(string CatalogGroupSlug, string CatalogItemSlug)
    : IQuery<Result<ValidateCatalogItemResponse>>;
