using CustomCodeFramework.Core.Domain.Entities;
using Dhole.Config.Domain.Catalogs.Events;

namespace Dhole.Config.Domain.Catalogs.Entities;

public sealed class CatalogItem : SoftDeletableAggregateRoot<Guid>
{
    private CatalogItem() { }

    private CatalogItem(
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
    )
        : base(id)
    {
        CatalogGroupId = catalogGroupId;

        Code = code.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Value = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? null : metadataJson;

        SortOrder = sortOrder;
        IsSystem = isSystem;
        IsActive = true;

        MarkAsCreated(DateTime.UtcNow, createdBy?.ToString());
    }

    public Guid CatalogGroupId { get; private set; }

    public string Code { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Value { get; private set; }
    public string? MetadataJson { get; private set; }

    public int SortOrder { get; private set; }

    public bool IsSystem { get; private set; }
    public bool IsActive { get; private set; }

    public CatalogGroup CatalogGroup { get; private set; } = default!;

    public static CatalogItem Create(
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
    )
    {
        return Create(
            Guid.NewGuid(),
            catalogGroupId,
            code,
            slug,
            name,
            description,
            value,
            metadataJson,
            sortOrder,
            isSystem,
            createdBy
        );
    }

    public static CatalogItem Create(
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
    )
    {
        var catalogItem = new CatalogItem(
            id,
            catalogGroupId,
            code,
            slug,
            name,
            description,
            value,
            metadataJson,
            sortOrder,
            isSystem,
            createdBy
        );

        catalogItem.AddDomainEvent(
            new CatalogItemCreatedDomainEvent(
                catalogItem.Id,
                catalogItem.CatalogGroupId,
                catalogItem.Code,
                catalogItem.Slug,
                catalogItem.Name,
                catalogItem.Description,
                catalogItem.Value,
                catalogItem.MetadataJson,
                catalogItem.SortOrder,
                catalogItem.IsSystem,
                createdBy
            )
        );

        return catalogItem;
    }

    public void Update(
        string name,
        string? description,
        string? value,
        string? metadataJson,
        int sortOrder,
        Guid? updatedBy = null
    )
    {
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Value = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? null : metadataJson;
        SortOrder = sortOrder;

        MarkAsUpdated(DateTime.UtcNow, updatedBy?.ToString());

        AddDomainEvent(
            new CatalogItemUpdatedDomainEvent(
                Id,
                CatalogGroupId,
                Code,
                Slug,
                Name,
                Description,
                Value,
                MetadataJson,
                SortOrder,
                updatedBy
            )
        );
    }

    public void ChangeSortOrder(int sortOrder, Guid? updatedBy = null)
    {
        if (SortOrder == sortOrder)
        {
            return;
        }

        var previousSortOrder = SortOrder;

        SortOrder = sortOrder;

        MarkAsUpdated(DateTime.UtcNow, updatedBy?.ToString());

        AddDomainEvent(
            new CatalogItemSortOrderChangedDomainEvent(
                Id,
                CatalogGroupId,
                Code,
                Slug,
                Name,
                previousSortOrder,
                SortOrder,
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
            AddDomainEvent(
                new CatalogItemActivatedDomainEvent(Id, CatalogGroupId, Code, Slug, Name, updatedBy)
            );

            return;
        }

        AddDomainEvent(
            new CatalogItemInactivatedDomainEvent(Id, CatalogGroupId, Code, Slug, Name, updatedBy)
        );
    }

    public void Delete(Guid? deletedBy = null)
    {
        if (IsSystem)
        {
            throw new InvalidOperationException("No se puede eliminar un item del sistema.");
        }

        MarkAsDeleted(DateTime.UtcNow, deletedBy?.ToString());

        AddDomainEvent(
            new CatalogItemDeletedDomainEvent(Id, CatalogGroupId, Code, Slug, Name, deletedBy)
        );
    }
}
