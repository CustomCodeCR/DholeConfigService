using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Config.Domain.Catalogs.Events;

public sealed record CatalogGroupCreatedDomainEvent(
    Guid id,
    string code,
    string slug,
    string name,
    string? description,
    string? metadataJson,
    bool isSystem,
    Guid? createdBy
) : DomainEvent;
