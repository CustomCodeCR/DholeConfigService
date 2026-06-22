namespace Dhole.Config.Api.Authorization;

internal static class ConfigScopeNames
{
    public const string CatalogGroupsCreate = "config.catalog-groups.create";
    public const string CatalogGroupsView = "config.catalog-groups.view";
    public const string CatalogGroupsUpdate = "config.catalog-groups.update";
    public const string CatalogGroupsDelete = "config.catalog-groups.delete";
    public const string CatalogGroupsSetActive = "config.catalog-groups.set-active";

    public const string CatalogItemsCreate = "config.catalog-items.create";
    public const string CatalogItemsView = "config.catalog-items.view";
    public const string CatalogItemsUpdate = "config.catalog-items.update";
    public const string CatalogItemsDelete = "config.catalog-items.delete";
    public const string CatalogItemsSetActive = "config.catalog-items.set-active";
    public const string CatalogItemsChangeSortOrder = "config.catalog-items.change-sort-order";

    public const string CatalogSelectsView = "config.catalog-selects.view";
    public const string CatalogItemsValidate = "config.catalog-items.validate";
}
