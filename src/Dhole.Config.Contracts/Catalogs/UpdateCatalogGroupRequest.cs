namespace Dhole.Config.Contracts.Catalogs;

public sealed record UpdateCatalogGroupRequest(
    string Name,
    string? Description,
    string? MetadataJson
);
