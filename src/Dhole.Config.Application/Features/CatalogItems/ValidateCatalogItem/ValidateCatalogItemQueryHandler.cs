using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Config.Application.Abstractions.Repositories;
using Dhole.Config.Contracts.Catalogs;

namespace Dhole.Config.Application.CatalogItems.ValidateCatalogItem;

public sealed class ValidateCatalogItemQueryHandler(ICatalogItemRepository catalogItems)
    : IQueryHandler<ValidateCatalogItemQuery, Result<ValidateCatalogItemResponse>>
{
    public async Task<Result<ValidateCatalogItemResponse>> HandleAsync(
        ValidateCatalogItemQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var item = await catalogItems.GetLookupAsync(
            query.CatalogGroupSlug,
            query.CatalogItemSlug,
            cancellationToken
        );

        if (item is null || !item.IsActive)
        {
            return Result.Success(
                new ValidateCatalogItemResponse(
                    false,
                    query.CatalogGroupSlug,
                    query.CatalogItemSlug,
                    null
                )
            );
        }

        return Result.Success(
            new ValidateCatalogItemResponse(
                true,
                query.CatalogGroupSlug,
                query.CatalogItemSlug,
                item
            )
        );
    }
}
