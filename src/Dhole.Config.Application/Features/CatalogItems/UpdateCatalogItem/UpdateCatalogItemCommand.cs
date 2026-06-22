using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Config.Application.CatalogItems.UpdateCatalogItem;

public sealed record UpdateCatalogItemCommand(
    Guid Id,
    string Name,
    string? Description,
    string? Value,
    string? MetadataJson,
    int SortOrder,
    Guid? UpdatedBy
) : ICommand<Result>;
