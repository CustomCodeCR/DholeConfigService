using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Config.Application.Abstractions.Auditing;
using Dhole.Config.Application.Abstractions.Cache;
using Dhole.Config.Application.Abstractions.Repositories;
using Dhole.Config.Application.Auditing;
using Dhole.Config.Domain.Shared;

namespace Dhole.Config.Application.CatalogItems.DeleteCatalogItem;

public sealed class DeleteCatalogItemCommandHandler(
    ICatalogGroupRepository catalogGroups,
    ICatalogItemRepository catalogItems,
    IConfigAuditService audit,
    IConfigCacheService cache,
    IUnitOfWork unitOfWork
) : ICommandHandler<DeleteCatalogItemCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeleteCatalogItemCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var catalogItem = await catalogItems.GetByIdAsync(command.Id, cancellationToken);

        if (catalogItem is null || catalogItem.IsDeleted)
        {
            return Result.Failure(ConfigErrors.CatalogItemNotFound);
        }

        if (catalogItem.IsSystem)
        {
            return Result.Failure(ConfigErrors.SystemCatalogItemCannotBeDeleted);
        }

        var catalogGroup = await catalogGroups.GetByIdAsync(
            catalogItem.CatalogGroupId,
            cancellationToken
        );

        if (catalogGroup is null || catalogGroup.IsDeleted)
        {
            return Result.Failure(ConfigErrors.CatalogGroupNotFound);
        }

        var before = CatalogItemAuditSnapshot.From(catalogItem);

        catalogItem.Delete(command.DeletedBy);

        await audit.PublishAsync(
            new ConfigAuditEvent(
                EventType: ConfigAuditEventTypes.CatalogItemDeleted,
                Action: ConfigAuditActions.Deleted,
                EntityType: ConfigAuditEntityTypes.CatalogItem,
                EntityId: catalogItem.Id,
                ActorUserId: command.DeletedBy,
                Before: before,
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

        return Result.Success();
    }
}
