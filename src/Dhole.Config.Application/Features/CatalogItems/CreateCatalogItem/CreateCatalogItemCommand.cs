using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Config.Application.CatalogItems.CreateCatalogItem;

public sealed record CreateCatalogItemCommand(
    Guid CatalogGroupId,
    string Name,
    string? Slug,
    string? Description,
    string? Value,
    string? MetadataJson,
    int SortOrder,
    bool IsSystem,
    Guid? CreatedBy
) : ICommand<Result<Guid>>;
