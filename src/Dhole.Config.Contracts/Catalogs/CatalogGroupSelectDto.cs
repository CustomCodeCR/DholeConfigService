namespace Dhole.Config.Contracts.Catalogs;

public sealed record CatalogGroupSelectDto(
    Guid Id,
    string Code,
    string Slug,
    string Name,
    bool IsSystem,
    bool IsActive
);
