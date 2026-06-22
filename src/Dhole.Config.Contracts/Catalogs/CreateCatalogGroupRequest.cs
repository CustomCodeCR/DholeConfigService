namespace Dhole.Config.Contracts.Catalogs;

public sealed record CreateCatalogGroupRequest(
    string Name,
    string? Slug,
    string? Description,
    string? MetadataJson,
    bool IsSystem = false
);
