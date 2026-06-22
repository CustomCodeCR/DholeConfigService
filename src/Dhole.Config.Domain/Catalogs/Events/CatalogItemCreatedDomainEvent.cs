using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Config.Domain.Catalogs.Events;

public sealed record CatalogItemCreatedDomainEvent(
    Guid id,
    Guid catalogGroupId,
    string code,
    string slug,
    string name,
    string? description,
    string? value,
    string? metadataJson,
    int sortOrder,
    bool isSystem,
    Guid? createdBy
) : DomainEvent;
