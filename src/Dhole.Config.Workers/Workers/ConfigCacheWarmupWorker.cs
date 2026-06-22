using CustomCodeFramework.Workers.Abstractions;
using Dhole.Config.Application.Abstractions.Cache;
using Dhole.Config.Contracts.Catalogs;
using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Config.Worker.Workers;

internal sealed class ConfigCacheWarmupWorker(
    ServiceDbContext dbContext,
    IConfigCacheService cache,
    ILogger<ConfigCacheWarmupWorker> logger
) : IBackgroundWorker
{
    public string Name => "config.cache-warmup";

    public async Task ExecuteAsync(
        IWorkerExecutionContext context,
        CancellationToken cancellationToken
    )
    {
        var catalogGroups = await dbContext
            .CatalogGroups.AsNoTracking()
            .Where(x => !x.IsDeleted && x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.Slug,
                x.Name,
                x.Description,
                x.MetadataJson,
                x.IsSystem,
                x.IsActive,
                x.CreatedAtUtc,
                x.UpdatedAtUtc,
            })
            .ToListAsync(cancellationToken);

        var catalogGroupsSelect = catalogGroups
            .Select(x => new CatalogGroupSelectDto(
                x.Id,
                x.Code,
                x.Slug,
                x.Name,
                x.IsSystem,
                x.IsActive
            ))
            .ToList();

        await cache.SetCatalogGroupsSelectAsync(
            catalogGroupsSelect,
            cancellationToken: cancellationToken
        );

        var warmedItemGroups = 0;
        var warmedItems = 0;

        foreach (var catalogGroup in catalogGroups)
        {
            var items = await dbContext
                .CatalogItems.AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsActive && x.CatalogGroupId == catalogGroup.Id)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Select(x => new CatalogItemDto(
                    x.Id,
                    x.CatalogGroupId,
                    catalogGroup.Code,
                    catalogGroup.Slug,
                    x.Code,
                    x.Slug,
                    x.Name,
                    x.Description,
                    x.Value,
                    x.MetadataJson,
                    x.SortOrder,
                    x.IsSystem,
                    x.IsActive,
                    x.CreatedAtUtc,
                    x.UpdatedAtUtc
                ))
                .ToListAsync(cancellationToken);

            var itemsSelect = items
                .Select(x => new CatalogItemSelectDto(
                    x.Slug,
                    x.Name,
                    x.Id,
                    x.Code,
                    x.Slug,
                    x.MetadataJson,
                    x.IsActive
                ))
                .ToList();

            await cache.SetCatalogItemsByGroupSlugAsync(
                catalogGroup.Slug,
                items,
                cancellationToken: cancellationToken
            );

            await cache.SetCatalogItemsSelectByGroupSlugAsync(
                catalogGroup.Slug,
                itemsSelect,
                cancellationToken: cancellationToken
            );

            warmedItemGroups++;
            warmedItems += items.Count;
        }

        logger.LogInformation(
            "Config cache warmup completed. CatalogGroups: {CatalogGroupsCount}, CatalogItemGroups: {CatalogItemGroupsCount}, CatalogItems: {CatalogItemsCount}.",
            catalogGroups.Count,
            warmedItemGroups,
            warmedItems
        );
    }
}
