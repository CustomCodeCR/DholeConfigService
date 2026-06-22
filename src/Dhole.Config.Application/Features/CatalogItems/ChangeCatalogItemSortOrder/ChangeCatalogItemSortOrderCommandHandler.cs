using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Config.Application.Abstractions.Auditing;
using Dhole.Config.Application.Abstractions.Cache;
using Dhole.Config.Application.Abstractions.Repositories;
using Dhole.Config.Application.Auditing;
using Dhole.Config.Domain.Shared;

namespace Dhole.Config.Application.CatalogItems.ChangeCatalogItemSortOrder;

public sealed class ChangeCatalogItemSortOrderCommandHandler(
    ICatalogGroupRepository catalogGroups,
    ICatalogItemRepository catalogItems,
    IConfigAuditService audit,
    IConfigCacheService cache,
    IUnitOfWork unitOfWork
) : ICommandHandler<ChangeCatalogItemSortOrderCommand, Result>
{
    public async Task<Result> HandleAsync(
        ChangeCatalogItemSortOrderCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var catalogItem = await catalogItems.GetByIdAsync(command.Id, cancellationToken);

        if (catalogItem is null || catalogItem.IsDeleted)
        {
            return Result.Failure(ConfigErrors.CatalogItemNotFound);
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
        var previousSortOrder = catalogItem.SortOrder;

        catalogItem.ChangeSortOrder(command.SortOrder, command.UpdatedBy);

        await audit.PublishAsync(
            new ConfigAuditEvent(
                EventType: ConfigAuditEventTypes.CatalogItemSortOrderChanged,
                Action: ConfigAuditActions.SortOrderChanged,
                EntityType: ConfigAuditEntityTypes.CatalogItem,
                EntityId: catalogItem.Id,
                ActorUserId: command.UpdatedBy,
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
                    previousSortOrder,
                    currentSortOrder = catalogItem.SortOrder,
                }
            ),
            cancellationToken
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cache.RemoveCatalogGroupCacheAsync(catalogGroup.Slug, cancellationToken);

        return Result.Success();
    }
}
