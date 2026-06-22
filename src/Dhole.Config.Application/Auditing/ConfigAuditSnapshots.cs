using Dhole.Config.Domain.Catalogs.Entities;

namespace Dhole.Config.Application.Auditing;

public sealed record CatalogGroupAuditSnapshot(
    Guid Id,
    string Code,
    string Slug,
    string Name,
    string? Description,
    string? MetadataJson,
    bool IsSystem,
    bool IsActive,
    bool IsDeleted,
    IReadOnlyCollection<Guid> ItemIds
)
{
    public static CatalogGroupAuditSnapshot From(CatalogGroup catalogGroup)
    {
        return new CatalogGroupAuditSnapshot(
            catalogGroup.Id,
            catalogGroup.Code,
            catalogGroup.Slug,
            catalogGroup.Name,
            catalogGroup.Description,
            catalogGroup.MetadataJson,
            catalogGroup.IsSystem,
            catalogGroup.IsActive,
            catalogGroup.IsDeleted,
            catalogGroup.Items.Select(x => x.Id).OrderBy(x => x).ToArray()
        );
    }
}

public sealed record CatalogItemAuditSnapshot(
    Guid Id,
    Guid CatalogGroupId,
    string Code,
    string Slug,
    string Name,
    string? Description,
    string? Value,
    string? MetadataJson,
    int SortOrder,
    bool IsSystem,
    bool IsActive,
    bool IsDeleted
)
{
    public static CatalogItemAuditSnapshot From(CatalogItem catalogItem)
    {
        return new CatalogItemAuditSnapshot(
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
            catalogItem.IsActive,
            catalogItem.IsDeleted
        );
    }
}
