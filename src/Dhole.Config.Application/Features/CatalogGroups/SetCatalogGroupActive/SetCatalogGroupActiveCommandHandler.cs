using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Config.Application.Abstractions.Auditing;
using Dhole.Config.Application.Abstractions.Cache;
using Dhole.Config.Application.Abstractions.Repositories;
using Dhole.Config.Application.Auditing;
using Dhole.Config.Domain.Shared;

namespace Dhole.Config.Application.CatalogGroups.SetCatalogGroupActive;

public sealed class SetCatalogGroupActiveCommandHandler(
    ICatalogGroupRepository catalogGroups,
    IConfigAuditService audit,
    IConfigCacheService cache,
    IUnitOfWork unitOfWork
) : ICommandHandler<SetCatalogGroupActiveCommand, Result>
{
    public async Task<Result> HandleAsync(
        SetCatalogGroupActiveCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var catalogGroup = await catalogGroups.GetByIdAsync(command.Id, cancellationToken);

        if (catalogGroup is null || catalogGroup.IsDeleted)
        {
            return Result.Failure(ConfigErrors.CatalogGroupNotFound);
        }

        var before = CatalogGroupAuditSnapshot.From(catalogGroup);

        catalogGroup.SetActive(command.IsActive, command.UpdatedBy);

        var eventType = command.IsActive
            ? ConfigAuditEventTypes.CatalogGroupActivated
            : ConfigAuditEventTypes.CatalogGroupInactivated;

        var action = command.IsActive
            ? ConfigAuditActions.Activated
            : ConfigAuditActions.Inactivated;

        await audit.PublishAsync(
            new ConfigAuditEvent(
                EventType: eventType,
                Action: action,
                EntityType: ConfigAuditEntityTypes.CatalogGroup,
                EntityId: catalogGroup.Id,
                ActorUserId: command.UpdatedBy,
                Before: before,
                After: CatalogGroupAuditSnapshot.From(catalogGroup),
                Payload: new
                {
                    catalogGroupId = catalogGroup.Id,
                    catalogGroup.Code,
                    catalogGroup.Slug,
                    catalogGroup.Name,
                    catalogGroup.IsActive,
                }
            ),
            cancellationToken
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cache.RemoveCatalogGroupCacheAsync(catalogGroup.Slug, cancellationToken);

        return Result.Success();
    }
}
