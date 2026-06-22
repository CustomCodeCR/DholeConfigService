using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Config.Application.CatalogGroups.CreateCatalogGroup;

public sealed record CreateCatalogGroupCommand(
    string Name,
    string? Slug,
    string? Description,
    string? MetadataJson,
    bool IsSystem,
    Guid? CreatedBy
) : ICommand<Result<Guid>>;
