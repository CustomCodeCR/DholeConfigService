using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Config.Domain.Catalogs.Events;

public sealed record CatalogGroupActivatedDomainEvent(
    Guid id,
    string code,
    string slug,
    string name,
    Guid? updatedBy
) : DomainEvent;
