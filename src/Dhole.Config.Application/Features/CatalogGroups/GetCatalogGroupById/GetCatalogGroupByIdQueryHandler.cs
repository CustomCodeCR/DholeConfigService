using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Config.Application.Abstractions.Repositories;
using Dhole.Config.Contracts.Catalogs;
using Dhole.Config.Domain.Shared;

namespace Dhole.Config.Application.CatalogGroups.GetCatalogGroupById;

public sealed class GetCatalogGroupByIdQueryHandler(ICatalogGroupRepository catalogGroups)
    : IQueryHandler<GetCatalogGroupByIdQuery, Result<CatalogGroupDto>>
{
    public async Task<Result<CatalogGroupDto>> HandleAsync(
        GetCatalogGroupByIdQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var catalogGroup = await catalogGroups.GetByIdAsync(query.Id, cancellationToken);

        if (catalogGroup is null || catalogGroup.IsDeleted)
        {
            return Result.Failure<CatalogGroupDto>(ConfigErrors.CatalogGroupNotFound);
        }

        return Result.Success(
            new CatalogGroupDto(
                catalogGroup.Id,
                catalogGroup.Code,
                catalogGroup.Slug,
                catalogGroup.Name,
                catalogGroup.Description,
                catalogGroup.MetadataJson,
                catalogGroup.IsSystem,
                catalogGroup.IsActive,
                catalogGroup.CreatedAtUtc,
                catalogGroup.UpdatedAtUtc
            )
        );
    }
}
