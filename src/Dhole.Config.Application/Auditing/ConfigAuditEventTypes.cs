namespace Dhole.Config.Application.Auditing;

public static class ConfigAuditEventTypes
{
    public const string CatalogGroupCreated = "config.catalog-group.created";
    public const string CatalogGroupUpdated = "config.catalog-group.updated";
    public const string CatalogGroupDeleted = "config.catalog-group.deleted";
    public const string CatalogGroupActivated = "config.catalog-group.activated";
    public const string CatalogGroupInactivated = "config.catalog-group.inactivated";

    public const string CatalogItemCreated = "config.catalog-item.created";
    public const string CatalogItemUpdated = "config.catalog-item.updated";
    public const string CatalogItemDeleted = "config.catalog-item.deleted";
    public const string CatalogItemActivated = "config.catalog-item.activated";
    public const string CatalogItemInactivated = "config.catalog-item.inactivated";
    public const string CatalogItemSortOrderChanged = "config.catalog-item.sort-order-changed";
}
