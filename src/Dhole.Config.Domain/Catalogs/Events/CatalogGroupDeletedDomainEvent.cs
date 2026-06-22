using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Config.Domain.Catalogs.Events;

public sealed record CatalogGroupDeletedDomainEvent(
    Guid id,
    string code,
    string slug,
    string name,
    Guid? deletedBy
) : DomainEvent;
