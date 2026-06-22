using Dhole.Config.Contracts.Catalogs;

namespace Dhole.Config.Application.Abstractions.Cache;

public interface IConfigCacheService
{
    Task<CatalogGroupDto?> GetCatalogGroupBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default
    );

    Task SetCatalogGroupBySlugAsync(
        string slug,
        CatalogGroupDto catalogGroup,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default
    );

    Task RemoveCatalogGroupBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CatalogGroupSelectDto>?> GetCatalogGroupsSelectAsync(
        CancellationToken cancellationToken = default
    );

    Task SetCatalogGroupsSelectAsync(
        IReadOnlyCollection<CatalogGroupSelectDto> catalogGroups,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default
    );

    Task RemoveCatalogGroupsSelectAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CatalogItemDto>?> GetCatalogItemsByGroupSlugAsync(
        string catalogGroupSlug,
        CancellationToken cancellationToken = default
    );

    Task SetCatalogItemsByGroupSlugAsync(
        string catalogGroupSlug,
        IReadOnlyCollection<CatalogItemDto> items,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default
    );

    Task RemoveCatalogItemsByGroupSlugAsync(
        string catalogGroupSlug,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<CatalogItemSelectDto>?> GetCatalogItemsSelectByGroupSlugAsync(
        string catalogGroupSlug,
        CancellationToken cancellationToken = default
    );

    Task SetCatalogItemsSelectByGroupSlugAsync(
        string catalogGroupSlug,
        IReadOnlyCollection<CatalogItemSelectDto> items,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default
    );

    Task RemoveCatalogItemsSelectByGroupSlugAsync(
        string catalogGroupSlug,
        CancellationToken cancellationToken = default
    );

    Task RemoveCatalogGroupCacheAsync(
        string catalogGroupSlug,
        CancellationToken cancellationToken = default
    );
}
