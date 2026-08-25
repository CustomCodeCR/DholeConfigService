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
        // v2: select values now expose CatalogItem.Value instead of duplicating Slug.
        // Version the key so Redis cannot serve stale select snapshots created with
        // the old semantics after a deployment.
        return $"config:catalog-groups:slug:{Normalize(catalogGroupSlug)}:items:select:v2";
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
