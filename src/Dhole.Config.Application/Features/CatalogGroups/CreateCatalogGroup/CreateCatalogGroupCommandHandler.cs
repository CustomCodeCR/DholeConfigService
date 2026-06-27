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

namespace Dhole.Config.Application.CatalogGroups.CreateCatalogGroup;

public sealed class CreateCatalogGroupCommandHandler(
    ICatalogGroupRepository catalogGroups,
    ICodeGenerator codeGenerator,
    ISlugGenerator slugGenerator,
    IConfigAuditService audit,
    IConfigCacheService cache,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateCatalogGroupCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(
        CreateCatalogGroupCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var code = await codeGenerator.GenerateCatalogGroupCodeAsync(
            catalogGroups.ExistsByCodeAsync,
            cancellationToken
        );

        var slug = string.IsNullOrWhiteSpace(command.Slug)
            ? await slugGenerator.GenerateUniqueCatalogGroupSlugAsync(
                command.Name,
                catalogGroups.ExistsBySlugAsync,
                cancellationToken
            )
            : slugGenerator.Generate(command.Slug);

        if (await catalogGroups.ExistsBySlugAsync(slug, cancellationToken))
        {
            return Result.Failure<Guid>(ConfigErrors.CatalogGroupSlugAlreadyExists);
        }

        if (await catalogGroups.ExistsByNameAsync(command.Name, cancellationToken: cancellationToken))
        {
            return Result.Failure<Guid>(ConfigErrors.CatalogGroupNameAlreadyExists);
        }

        var catalogGroup = CatalogGroup.Create(
            code,
            slug,
            command.Name,
            command.Description,
            command.MetadataJson,
            command.IsSystem,
            command.CreatedBy
        );

        await catalogGroups.AddAsync(catalogGroup, cancellationToken);

        await audit.PublishAsync(
            new ConfigAuditEvent(
                EventType: ConfigAuditEventTypes.CatalogGroupCreated,
                Action: ConfigAuditActions.Created,
                EntityType: ConfigAuditEntityTypes.CatalogGroup,
                EntityId: catalogGroup.Id,
                ActorUserId: command.CreatedBy,
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

        await cache.RemoveCatalogGroupsSelectAsync(cancellationToken);

        return Result.Success(catalogGroup.Id);
    }
}
