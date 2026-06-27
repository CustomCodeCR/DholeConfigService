using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Config.Application.Abstractions.Auditing;
using Dhole.Config.Application.Abstractions.Cache;
using Dhole.Config.Application.Abstractions.Repositories;
using Dhole.Config.Application.Auditing;
using Dhole.Config.Domain.Shared;

namespace Dhole.Config.Application.CatalogGroups.UpdateCatalogGroup;

public sealed class UpdateCatalogGroupCommandHandler(
    ICatalogGroupRepository catalogGroups,
    IConfigAuditService audit,
    IConfigCacheService cache,
    IUnitOfWork unitOfWork
) : ICommandHandler<UpdateCatalogGroupCommand, Result>
{
    public async Task<Result> HandleAsync(
        UpdateCatalogGroupCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var catalogGroup = await catalogGroups.GetByIdAsync(command.Id, cancellationToken);

        if (catalogGroup is null || catalogGroup.IsDeleted)
        {
            return Result.Failure(ConfigErrors.CatalogGroupNotFound);
        }

        if (await catalogGroups.ExistsByNameAsync(command.Name, catalogGroup.Id, cancellationToken))
        {
            return Result.Failure(ConfigErrors.CatalogGroupNameAlreadyExists);
        }

        var before = CatalogGroupAuditSnapshot.From(catalogGroup);

        catalogGroup.Update(
            command.Name,
            command.Description,
            command.MetadataJson,
            command.UpdatedBy
        );

        await audit.PublishAsync(
            new ConfigAuditEvent(
                EventType: ConfigAuditEventTypes.CatalogGroupUpdated,
                Action: ConfigAuditActions.Updated,
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
                }
            ),
            cancellationToken
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cache.RemoveCatalogGroupCacheAsync(catalogGroup.Slug, cancellationToken);

        return Result.Success();
    }
}
