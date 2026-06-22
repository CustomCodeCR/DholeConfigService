namespace Dhole.Config.Domain.Shared;

public static class ConfigConstants
{
    public const string ServiceName = "Config";

    public static class Scopes
    {
        // Catalog groups
        public const string CatalogGroupCreate = "config.catalog-groups.create";
        public const string CatalogGroupView = "config.catalog-groups.view";
        public const string CatalogGroupUpdate = "config.catalog-groups.update";
        public const string CatalogGroupDelete = "config.catalog-groups.delete";
        public const string CatalogGroupSetActive = "config.catalog-groups.set-active";

        // Catalog items
        public const string CatalogItemCreate = "config.catalog-items.create";
        public const string CatalogItemView = "config.catalog-items.view";
        public const string CatalogItemUpdate = "config.catalog-items.update";
        public const string CatalogItemDelete = "config.catalog-items.delete";
        public const string CatalogItemSetActive = "config.catalog-items.set-active";
        public const string CatalogItemChangeSortOrder = "config.catalog-items.change-sort-order";

        // Selects / lookups
        public const string CatalogSelectView = "config.catalog-selects.view";
        public const string CatalogItemValidate = "config.catalog-items.validate";
    }

    public static class Audit
    {
        public static class EntityTypes
        {
            public const string CatalogGroup = "CatalogGroup";
            public const string CatalogItem = "CatalogItem";
        }

        public static class Actions
        {
            public const string Created = "created";
            public const string Updated = "updated";
            public const string Deleted = "deleted";
            public const string Activated = "activated";
            public const string Inactivated = "inactivated";
            public const string SortOrderChanged = "sort_order_changed";
        }

        public static class EventTypes
        {
            // Catalog groups
            public const string CatalogGroupCreated = "config.catalog-group.created";
            public const string CatalogGroupUpdated = "config.catalog-group.updated";
            public const string CatalogGroupDeleted = "config.catalog-group.deleted";
            public const string CatalogGroupActivated = "config.catalog-group.activated";
            public const string CatalogGroupInactivated = "config.catalog-group.inactivated";

            // Catalog items
            public const string CatalogItemCreated = "config.catalog-item.created";
            public const string CatalogItemUpdated = "config.catalog-item.updated";
            public const string CatalogItemDeleted = "config.catalog-item.deleted";
            public const string CatalogItemActivated = "config.catalog-item.activated";
            public const string CatalogItemInactivated = "config.catalog-item.inactivated";
            public const string CatalogItemSortOrderChanged =
                "config.catalog-item.sort-order-changed";
        }
    }
}
