using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Config.Application.Abstractions.Auditing;
using Dhole.Config.Application.Abstractions.Cache;
using Dhole.Config.Application.Abstractions.Repositories;
using Dhole.Config.Application.Auditing;
using Dhole.Config.Domain.Shared;

namespace Dhole.Config.Application.CatalogGroups.DeleteCatalogGroup;

public sealed class DeleteCatalogGroupCommandHandler(
    ICatalogGroupRepository catalogGroups,
    IConfigAuditService audit,
    IConfigCacheService cache,
    IUnitOfWork unitOfWork
) : ICommandHandler<DeleteCatalogGroupCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeleteCatalogGroupCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var catalogGroup = await catalogGroups.GetByIdAsync(command.Id, cancellationToken);

        if (catalogGroup is null || catalogGroup.IsDeleted)
        {
            return Result.Failure(ConfigErrors.CatalogGroupNotFound);
        }

        if (catalogGroup.IsSystem)
        {
            return Result.Failure(ConfigErrors.SystemCatalogGroupCannotBeDeleted);
        }

        var before = CatalogGroupAuditSnapshot.From(catalogGroup);

        catalogGroup.Delete(command.DeletedBy);

        await audit.PublishAsync(
            new ConfigAuditEvent(
                EventType: ConfigAuditEventTypes.CatalogGroupDeleted,
                Action: ConfigAuditActions.Deleted,
                EntityType: ConfigAuditEntityTypes.CatalogGroup,
                EntityId: catalogGroup.Id,
                ActorUserId: command.DeletedBy,
                Before: before,
                After: CatalogGroupAuditSnapshot.From(catalogGroup),
                Payload: new
                {
                    catalogGroupId = catalogGroup.Id,
                    catalogGroup.Code,
                    catalogGroup.Slug,
                    catalogGroup.Name,
                }
            ),
            cancellationToken
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cache.RemoveCatalogGroupCacheAsync(catalogGroup.Slug, cancellationToken);

        return Result.Success();
    }
}
