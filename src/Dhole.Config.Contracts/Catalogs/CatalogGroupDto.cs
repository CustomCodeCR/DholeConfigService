namespace Dhole.Config.Contracts.Catalogs;

public sealed record CatalogGroupDto(
    Guid Id,
    string Code,
    string Slug,
    string Name,
    string? Description,
    string? MetadataJson,
    bool IsSystem,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
