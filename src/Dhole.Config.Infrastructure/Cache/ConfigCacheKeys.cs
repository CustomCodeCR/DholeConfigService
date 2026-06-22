namespace Dhole.Config.Infrastructure.Cache;

internal static class ConfigCacheKeys
{
    public static string CatalogGroupBySlug(string catalogGroupSlug)
    {
        return $"config:catalog-groups:slug:{Normalize(catalogGroupSlug)}";
    }

    public static string CatalogGroupsSelect()
    {
        return "config:catalog-groups:select";
    }

    public static string CatalogItemsByGroupSlug(string catalogGroupSlug)
    {
        return $"config:catalog-groups:slug:{Normalize(catalogGroupSlug)}:items";
    }

    public static string CatalogItemsSelectByGroupSlug(string catalogGroupSlug)
    {
        return $"config:catalog-groups:slug:{Normalize(catalogGroupSlug)}:items:select";
    }

    public static string CatalogItemLookup(string catalogGroupSlug, string catalogItemSlug)
    {
        return $"config:catalog-groups:slug:{Normalize(catalogGroupSlug)}:items:slug:{Normalize(catalogItemSlug)}";
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToLowerInvariant();
    }
}
