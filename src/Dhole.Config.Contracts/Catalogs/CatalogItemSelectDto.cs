namespace Dhole.Config.Contracts.Catalogs;

public sealed record CatalogItemSelectDto(
    string Value,
    string Label,
    Guid Id,
    string Code,
    string Slug,
    string? MetadataJson,
    bool IsActive
);
