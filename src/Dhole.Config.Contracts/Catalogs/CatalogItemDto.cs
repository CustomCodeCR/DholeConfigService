namespace Dhole.Config.Contracts.Catalogs;

public sealed record CatalogItemDto(
    Guid Id,
    Guid CatalogGroupId,
    string CatalogGroupCode,
    string CatalogGroupSlug,
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
