using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Config.Domain.Catalogs.Events;

public sealed record CatalogGroupUpdatedDomainEvent(
    Guid id,
    string code,
    string slug,
    string name,
    string? description,
    string? metadataJson,
    Guid? updatedBy
) : DomainEvent;
