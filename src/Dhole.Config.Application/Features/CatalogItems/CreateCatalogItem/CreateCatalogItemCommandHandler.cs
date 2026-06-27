using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Config.Application.Abstractions.Auditing;
using Dhole.Config.Application.Abstractions.Cache;
using Dhole.Config.Application.Abstractions.Codes;
using Dhole.Config.Application.Abstractions.Repositories;
using Dhole.Config.Application.Abstractions.Slugs;
using Dhole.Config.Application.Auditing;
using Dhole.Config.Domain.Catalogs.Entities;
using Dhole.Config.Domain.Shared;

namespace Dhole.Config.Application.CatalogItems.CreateCatalogItem;

public sealed class CreateCatalogItemCommandHandler(
    ICatalogGroupRepository catalogGroups,
    ICatalogItemRepository catalogItems,
    ICodeGenerator codeGenerator,
    ISlugGenerator slugGenerator,
    IConfigAuditService audit,
    IConfigCacheService cache,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateCatalogItemCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(
        CreateCatalogItemCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var catalogGroup = await catalogGroups.GetByIdAsync(
            command.CatalogGroupId,
            cancellationToken
        );

        if (catalogGroup is null || catalogGroup.IsDeleted)
        {
            return Result.Failure<Guid>(ConfigErrors.CatalogGroupNotFound);
        }

        if (!catalogGroup.IsActive)
        {
            return Result.Failure<Guid>(ConfigErrors.CatalogGroupInactive);
        }

        var code = await codeGenerator.GenerateCatalogItemCodeAsync(
            catalogGroup.Name,
            (value, ct) => catalogItems.ExistsByCodeAsync(catalogGroup.Id, value, ct),
            cancellationToken
        );

        var slug = string.IsNullOrWhiteSpace(command.Slug)
            ? await slugGenerator.GenerateUniqueCatalogItemSlugAsync(
                command.Name,
                (value, ct) => catalogItems.ExistsBySlugAsync(catalogGroup.Id, value, ct),
                cancellationToken
            )
            : slugGenerator.Generate(command.Slug);

        if (await catalogItems.ExistsBySlugAsync(catalogGroup.Id, slug, cancellationToken))
        {
            return Result.Failure<Guid>(ConfigErrors.CatalogItemSlugAlreadyExists);
        }

        if (await catalogItems.ExistsByNameAsync(catalogGroup.Id, command.Name, cancellationToken: cancellationToken))
        {
            return Result.Failure<Guid>(ConfigErrors.CatalogItemNameAlreadyExists);
        }

        var catalogItem = CatalogItem.Create(
            catalogGroup.Id,
            code,
            slug,
            command.Name,
            command.Description,
            command.Value,
            command.MetadataJson,
            command.SortOrder,
            command.IsSystem,
            command.CreatedBy
        );

        await catalogItems.AddAsync(catalogItem, cancellationToken);

        await audit.PublishAsync(
            new ConfigAuditEvent(
                EventType: ConfigAuditEventTypes.CatalogItemCreated,
                Action: ConfigAuditActions.Created,
                EntityType: ConfigAuditEntityTypes.CatalogItem,
                EntityId: catalogItem.Id,
                ActorUserId: command.CreatedBy,
                After: CatalogItemAuditSnapshot.From(catalogItem),
                Payload: new
                {
                    catalogItemId = catalogItem.Id,
                    catalogGroupId = catalogGroup.Id,
                    catalogGroupCode = catalogGroup.Code,
                    catalogGroupSlug = catalogGroup.Slug,
                    catalogItem.Code,
                    catalogItem.Slug,
                    catalogItem.Name,
                }
            ),
            cancellationToken
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cache.RemoveCatalogGroupCacheAsync(catalogGroup.Slug, cancellationToken);

        return Result.Success(catalogItem.Id);
    }
}
