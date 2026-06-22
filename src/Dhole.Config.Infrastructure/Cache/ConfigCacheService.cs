using CustomCodeFramework.Redis.Abstractions;
using CustomCodeFramework.Redis.Caching;
using Dhole.Config.Application.Abstractions.Cache;
using Dhole.Config.Contracts.Catalogs;

namespace Dhole.Config.Infrastructure.Cache;

public sealed class ConfigCacheService(ICacheService cache) : IConfigCacheService
{
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(30);

    public Task<CatalogGroupDto?> GetCatalogGroupBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default
    )
    {
        return cache.GetAsync<CatalogGroupDto>(
            ConfigCacheKeys.CatalogGroupBySlug(slug),
            cancellationToken
        );
    }

    public Task SetCatalogGroupBySlugAsync(
        string slug,
        CatalogGroupDto catalogGroup,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default
    )
    {
        return cache.SetAsync(
            ConfigCacheKeys.CatalogGroupBySlug(slug),
            catalogGroup,
            CacheEntryOptions.Default(expiration ?? DefaultExpiration),
            cancellationToken
        );
    }

    public Task RemoveCatalogGroupBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default
    )
    {
        return cache.RemoveAsync(ConfigCacheKeys.CatalogGroupBySlug(slug), cancellationToken);
    }

    public Task<IReadOnlyCollection<CatalogGroupSelectDto>?> GetCatalogGroupsSelectAsync(
        CancellationToken cancellationToken = default
    )
    {
        return cache.GetAsync<IReadOnlyCollection<CatalogGroupSelectDto>>(
            ConfigCacheKeys.CatalogGroupsSelect(),
            cancellationToken
        );
    }

    public Task SetCatalogGroupsSelectAsync(
        IReadOnlyCollection<CatalogGroupSelectDto> catalogGroups,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default
    )
    {
        return cache.SetAsync(
            ConfigCacheKeys.CatalogGroupsSelect(),
            catalogGroups,
            CacheEntryOptions.Default(expiration ?? DefaultExpiration),
            cancellationToken
        );
    }

    public Task RemoveCatalogGroupsSelectAsync(CancellationToken cancellationToken = default)
    {
        return cache.RemoveAsync(ConfigCacheKeys.CatalogGroupsSelect(), cancellationToken);
    }

    public Task<IReadOnlyCollection<CatalogItemDto>?> GetCatalogItemsByGroupSlugAsync(
        string catalogGroupSlug,
        CancellationToken cancellationToken = default
    )
    {
        return cache.GetAsync<IReadOnlyCollection<CatalogItemDto>>(
            ConfigCacheKeys.CatalogItemsByGroupSlug(catalogGroupSlug),
            cancellationToken
        );
    }

    public Task SetCatalogItemsByGroupSlugAsync(
        string catalogGroupSlug,
        IReadOnlyCollection<CatalogItemDto> items,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default
    )
    {
        return cache.SetAsync(
            ConfigCacheKeys.CatalogItemsByGroupSlug(catalogGroupSlug),
            items,
            CacheEntryOptions.Default(expiration ?? DefaultExpiration),
            cancellationToken
        );
    }

    public Task RemoveCatalogItemsByGroupSlugAsync(
        string catalogGroupSlug,
        CancellationToken cancellationToken = default
    )
    {
        return cache.RemoveAsync(
            ConfigCacheKeys.CatalogItemsByGroupSlug(catalogGroupSlug),
            cancellationToken
        );
    }

    public Task<IReadOnlyCollection<CatalogItemSelectDto>?> GetCatalogItemsSelectByGroupSlugAsync(
        string catalogGroupSlug,
        CancellationToken cancellationToken = default
    )
    {
        return cache.GetAsync<IReadOnlyCollection<CatalogItemSelectDto>>(
            ConfigCacheKeys.CatalogItemsSelectByGroupSlug(catalogGroupSlug),
            cancellationToken
        );
    }

    public Task SetCatalogItemsSelectByGroupSlugAsync(
        string catalogGroupSlug,
        IReadOnlyCollection<CatalogItemSelectDto> items,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default
    )
    {
        return cache.SetAsync(
            ConfigCacheKeys.CatalogItemsSelectByGroupSlug(catalogGroupSlug),
            items,
            CacheEntryOptions.Default(expiration ?? DefaultExpiration),
            cancellationToken
        );
    }

    public Task RemoveCatalogItemsSelectByGroupSlugAsync(
        string catalogGroupSlug,
        CancellationToken cancellationToken = default
    )
    {
        return cache.RemoveAsync(
            ConfigCacheKeys.CatalogItemsSelectByGroupSlug(catalogGroupSlug),
            cancellationToken
        );
    }

    public async Task RemoveCatalogGroupCacheAsync(
        string catalogGroupSlug,
        CancellationToken cancellationToken = default
    )
    {
        await RemoveCatalogGroupBySlugAsync(catalogGroupSlug, cancellationToken);
        await RemoveCatalogItemsByGroupSlugAsync(catalogGroupSlug, cancellationToken);
        await RemoveCatalogItemsSelectByGroupSlugAsync(catalogGroupSlug, cancellationToken);
        await RemoveCatalogGroupsSelectAsync(cancellationToken);
    }
}
