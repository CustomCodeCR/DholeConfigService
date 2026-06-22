using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Config.Domain.Catalogs.Events;

public sealed record CatalogItemDeletedDomainEvent(
    Guid id,
    Guid catalogGroupId,
    string code,
    string slug,
    string name,
    Guid? deletedBy
) : DomainEvent;
