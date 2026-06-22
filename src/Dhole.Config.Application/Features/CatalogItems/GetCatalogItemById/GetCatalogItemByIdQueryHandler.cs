using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Config.Application.Abstractions.Repositories;
using Dhole.Config.Contracts.Catalogs;
using Dhole.Config.Domain.Shared;

namespace Dhole.Config.Application.CatalogItems.GetCatalogItemById;

public sealed class GetCatalogItemByIdQueryHandler(
    ICatalogItemRepository catalogItems,
    ICatalogGroupRepository catalogGroups
) : IQueryHandler<GetCatalogItemByIdQuery, Result<CatalogItemDto>>
{
    public async Task<Result<CatalogItemDto>> HandleAsync(
        GetCatalogItemByIdQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var catalogItem = await catalogItems.GetByIdAsync(query.Id, cancellationToken);

        if (catalogItem is null || catalogItem.IsDeleted)
        {
            return Result.Failure<CatalogItemDto>(ConfigErrors.CatalogItemNotFound);
        }

        var catalogGroup = await catalogGroups.GetByIdAsync(
            catalogItem.CatalogGroupId,
            cancellationToken
        );

        if (catalogGroup is null || catalogGroup.IsDeleted)
        {
            return Result.Failure<CatalogItemDto>(ConfigErrors.CatalogGroupNotFound);
        }

        return Result.Success(
            new CatalogItemDto(
                catalogItem.Id,
                catalogItem.CatalogGroupId,
                catalogGroup.Code,
                catalogGroup.Slug,
                catalogItem.Code,
                catalogItem.Slug,
                catalogItem.Name,
                catalogItem.Description,
                catalogItem.Value,
                catalogItem.MetadataJson,
                catalogItem.SortOrder,
                catalogItem.IsSystem,
                catalogItem.IsActive,
                catalogItem.CreatedAtUtc,
                catalogItem.UpdatedAtUtc
            )
        );
    }
}
