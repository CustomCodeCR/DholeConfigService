using CustomCodeFramework.Core.Domain.Entities;
using Dhole.Config.Domain.Catalogs.Events;

namespace Dhole.Config.Domain.Catalogs.Entities;

public sealed class CatalogGroup : SoftDeletableAggregateRoot<Guid>
{
    private readonly List<CatalogItem> _items = [];

    private CatalogGroup() { }

    private CatalogGroup(
        Guid id,
        string code,
        string slug,
        string name,
        string? description,
        string? metadataJson,
        bool isSystem,
        Guid? createdBy
    )
        : base(id)
    {
        Code = code.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? null : metadataJson;

        IsSystem = isSystem;
        IsActive = true;

        MarkAsCreated(DateTime.UtcNow, createdBy?.ToString());
    }

    public string Code { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? MetadataJson { get; private set; }

    public bool IsSystem { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<CatalogItem> Items => _items;

    public static CatalogGroup Create(
        string code,
        string slug,
        string name,
        string? description,
        string? metadataJson,
        bool isSystem,
        Guid? createdBy
    )
    {
        return Create(
            Guid.NewGuid(),
            code,
            slug,
            name,
            description,
            metadataJson,
            isSystem,
            createdBy
        );
    }

    public static CatalogGroup Create(
        Guid id,
        string code,
        string slug,
        string name,
        string? description,
        string? metadataJson,
        bool isSystem,
        Guid? createdBy
    )
    {
        var catalogGroup = new CatalogGroup(
            id,
            code,
            slug,
            name,
            description,
            metadataJson,
            isSystem,
            createdBy
        );

        catalogGroup.AddDomainEvent(
            new CatalogGroupCreatedDomainEvent(
                catalogGroup.Id,
                catalogGroup.Code,
                catalogGroup.Slug,
                catalogGroup.Name,
                catalogGroup.Description,
                catalogGroup.MetadataJson,
                catalogGroup.IsSystem,
                createdBy
            )
        );

        return catalogGroup;
    }

    public void Update(
        string name,
        string? description,
        string? metadataJson,
        Guid? updatedBy = null
    )
    {
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? null : metadataJson;

        MarkAsUpdated(DateTime.UtcNow, updatedBy?.ToString());

        AddDomainEvent(
            new CatalogGroupUpdatedDomainEvent(
                Id,
                Code,
                Slug,
                Name,
                Description,
                MetadataJson,
                updatedBy
            )
        );
    }

    public void SetActive(bool isActive, Guid? updatedBy = null)
    {
        if (IsActive == isActive)
        {
            return;
        }

        IsActive = isActive;

        MarkAsUpdated(DateTime.UtcNow, updatedBy?.ToString());

        if (IsActive)
        {
            AddDomainEvent(new CatalogGroupActivatedDomainEvent(Id, Code, Slug, Name, updatedBy));

            return;
        }

        AddDomainEvent(new CatalogGroupInactivatedDomainEvent(Id, Code, Slug, Name, updatedBy));
    }

    public void Delete(Guid? deletedBy = null)
    {
        if (IsSystem)
        {
            throw new InvalidOperationException("No se puede eliminar un catálogo del sistema.");
        }

        MarkAsDeleted(DateTime.UtcNow, deletedBy?.ToString());

        AddDomainEvent(new CatalogGroupDeletedDomainEvent(Id, Code, Slug, Name, deletedBy));
    }
}
