namespace Dhole.Config.Contracts.Catalogs;

public sealed record CatalogItemDetailDto(
    Guid Id,
    Guid CatalogGroupId,
    string CatalogGroupCode,
    string CatalogGroupSlug,
    string CatalogGroupName,
    string Code,
    string Slug,
    string Name,
    string? Description,
    string? Value,
    string? MetadataJson,
    int SortOrder,
    bool IsSystem,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
