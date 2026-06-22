namespace Dhole.Config.Contracts.Catalogs;

public sealed record ValidateCatalogItemResponse(
    bool IsValid,
    string CatalogGroupSlug,
    string CatalogItemSlug,
    CatalogItemLookupDto? Item
);
