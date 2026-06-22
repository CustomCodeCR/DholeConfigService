namespace Dhole.Config.Contracts.Catalogs;

public sealed record CreateCatalogItemRequest(
    Guid CatalogGroupId,
    string Name,
    string? Slug,
    string? Description,
    string? Value,
    string? MetadataJson,
    int SortOrder,
    bool IsSystem
);
