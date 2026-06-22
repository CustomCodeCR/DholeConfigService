using CustomCodeFramework.Core.Domain.Events;
using Dhole.Config.Domain.Catalogs.Events;

namespace Dhole.Config.Persistence.Messaging;

internal static class DomainEventOutboxMapper
{
    public static string GetEventName(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            // Catalog groups
            CatalogGroupCreatedDomainEvent => "config.catalog-group.created",
            CatalogGroupUpdatedDomainEvent => "config.catalog-group.updated",
            CatalogGroupDeletedDomainEvent => "config.catalog-group.deleted",
            CatalogGroupActivatedDomainEvent => "config.catalog-group.activated",
            CatalogGroupInactivatedDomainEvent => "config.catalog-group.inactivated",

            // Catalog items
            CatalogItemCreatedDomainEvent => "config.catalog-item.created",
            CatalogItemUpdatedDomainEvent => "config.catalog-item.updated",
            CatalogItemDeletedDomainEvent => "config.catalog-item.deleted",
            CatalogItemActivatedDomainEvent => "config.catalog-item.activated",
            CatalogItemInactivatedDomainEvent => "config.catalog-item.inactivated",
            CatalogItemSortOrderChangedDomainEvent => "config.catalog-item.sort-order-changed",

            _ => $"config.{domainEvent.GetType().Name}",
        };
    }

    public static string GetEventType(IDomainEvent domainEvent)
    {
        return domainEvent.GetType().FullName ?? domainEvent.GetType().Name;
    }
}
