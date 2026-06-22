namespace Dhole.Config.Contracts.Catalogs;

public sealed record UpdateCatalogItemRequest(
    string Name,
    string? Description,
    string? Value,
    string? MetadataJson,
    int SortOrder
);
