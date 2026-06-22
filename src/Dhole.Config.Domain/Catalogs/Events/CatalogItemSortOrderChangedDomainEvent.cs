using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Config.Domain.Catalogs.Events;

public sealed record CatalogItemSortOrderChangedDomainEvent(
    Guid id,
    Guid catalogGroupId,
    string code,
    string slug,
    string name,
    int previousSortOrder,
    int currentSortOrder,
    Guid? updatedBy
) : DomainEvent;
