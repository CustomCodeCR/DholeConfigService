using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Config.Application.CatalogGroups.UpdateCatalogGroup;

public sealed record UpdateCatalogGroupCommand(
    Guid Id,
    string Name,
    string? Description,
    string? MetadataJson,
    Guid? UpdatedBy
) : ICommand<Result>;
