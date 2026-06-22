using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Config.Domain.Catalogs.Events;

public sealed record CatalogItemActivatedDomainEvent(
    Guid id,
    Guid catalogGroupId,
    string code,
    string slug,
    string name,
    Guid? updatedBy
) : DomainEvent;
