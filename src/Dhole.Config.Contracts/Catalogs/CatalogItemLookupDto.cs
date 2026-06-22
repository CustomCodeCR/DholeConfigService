namespace Dhole.Config.Contracts.Catalogs;

public sealed record CatalogItemLookupDto(
    Guid Id,
    Guid CatalogGroupId,
    string CatalogGroupCode,
    string CatalogGroupSlug,
    string Code,
    string Slug,
    string Name,
    string? Value,
    string? MetadataJson,
    bool IsActive
);
