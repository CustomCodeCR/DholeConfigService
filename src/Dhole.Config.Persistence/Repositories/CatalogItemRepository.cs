using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Postgres.EntityFramework.Repositories;
using Dhole.Config.Application.Abstractions.Repositories;
using Dhole.Config.Contracts.Catalogs;
using Dhole.Config.Domain.Catalogs.Entities;
using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Config.Persistence.Repositories;

public sealed class CatalogItemRepository(ServiceDbContext dbContext)
    : EfRepository<CatalogItem, Guid>(dbContext),
        ICatalogItemRepository
{
    public Task<bool> ExistsByCodeAsync(
        Guid catalogGroupId,
        string code,
        CancellationToken cancellationToken = default
    )
    {
        var value = code.Trim();

        // El índice único de la base de datos no filtra soft-deleted.
        // Por eso esta validación también debe considerar registros eliminados.
        return dbContext.CatalogItems.AnyAsync(
            x => x.CatalogGroupId == catalogGroupId && x.Code == value,
            cancellationToken
        );
    }

    public Task<bool> ExistsBySlugAsync(
        Guid catalogGroupId,
        string slug,
        CancellationToken cancellationToken = default
    )
    {
        var value = slug.Trim().ToLowerInvariant();

        // El índice único de la base de datos no filtra soft-deleted.
        // Por eso esta validación también debe considerar registros eliminados.
        return dbContext.CatalogItems.AnyAsync(
            x => x.CatalogGroupId == catalogGroupId && x.Slug == value,
            cancellationToken
        );
    }


    public Task<bool> ExistsByNameAsync(
        Guid catalogGroupId,
        string name,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default
    )
    {
        var value = name.Trim();

        // El índice único de la base de datos no filtra soft-deleted.
        // Por eso esta validación también debe considerar registros eliminados.
        return dbContext.CatalogItems.AnyAsync(
            x =>
                x.CatalogGroupId == catalogGroupId
                && x.Name == value
                && (!excludeId.HasValue || x.Id != excludeId.Value),
            cancellationToken
        );
    }

    public Task<CatalogItem?> GetByCodeAsync(
        Guid catalogGroupId,
        string code,
        CancellationToken cancellationToken = default
    )
    {
        var value = code.Trim();

        return dbContext.CatalogItems.FirstOrDefaultAsync(
            x => x.CatalogGroupId == catalogGroupId && x.Code == value && !x.IsDeleted,
            cancellationToken
        );
    }

    public Task<CatalogItem?> GetBySlugAsync(
        Guid catalogGroupId,
        string slug,
        CancellationToken cancellationToken = default
    )
    {
        var value = slug.Trim().ToLowerInvariant();

        return dbContext.CatalogItems.FirstOrDefaultAsync(
            x => x.CatalogGroupId == catalogGroupId && x.Slug == value && !x.IsDeleted,
            cancellationToken
        );
    }

    public async Task<IReadOnlyCollection<CatalogItemDto>> GetActiveByGroupSlugAsync(
        string catalogGroupSlug,
        CancellationToken cancellationToken = default
    )
    {
        var value = catalogGroupSlug.Trim().ToLowerInvariant();

        return await dbContext
            .CatalogItems.AsNoTracking()
            .Where(x =>
                !x.IsDeleted
                && x.IsActive
                && x.CatalogGroup.Slug == value
                && !x.CatalogGroup.IsDeleted
                && x.CatalogGroup.IsActive
            )
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new CatalogItemDto(
                x.Id,
                x.CatalogGroupId,
                x.CatalogGroup.Code,
                x.CatalogGroup.Slug,
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
    }

    public async Task<PagedResult<CatalogItemDto>> GetPagedAsync(
        PageRequest page,
        Guid? catalogGroupId = null,
        string? search = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = dbContext
            .CatalogItems.AsNoTracking()
            .Where(x => !x.IsDeleted && !x.CatalogGroup.IsDeleted);

        if (catalogGroupId.HasValue)
        {
            query = query.Where(x => x.CatalogGroupId == catalogGroupId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim().ToLower();

            query = query.Where(x =>
                x.Code.ToLower().Contains(value)
                || x.Slug.ToLower().Contains(value)
                || x.Name.ToLower().Contains(value)
                || (x.Description != null && x.Description.ToLower().Contains(value))
                || (x.Value != null && x.Value.ToLower().Contains(value))
            );
        }

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Skip(page.Skip)
            .Take(page.PageSize)
            .Select(x => new CatalogItemDto(
                x.Id,
                x.CatalogGroupId,
                x.CatalogGroup.Code,
                x.CatalogGroup.Slug,
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

        return PagedResult<CatalogItemDto>.Create(items, page.PageNumber, page.PageSize, total);
    }

    public async Task<IReadOnlyCollection<CatalogItemSelectDto>> GetForSelectAsync(
        Guid? catalogGroupId = null,
        string? catalogGroupSlug = null,
        string? search = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = dbContext
            .CatalogItems.AsNoTracking()
            .Where(x =>
                !x.IsDeleted && x.IsActive && !x.CatalogGroup.IsDeleted && x.CatalogGroup.IsActive
            );

        if (catalogGroupId.HasValue)
        {
            query = query.Where(x => x.CatalogGroupId == catalogGroupId.Value);
        }

        if (!string.IsNullOrWhiteSpace(catalogGroupSlug))
        {
            var groupSlug = catalogGroupSlug.Trim().ToLowerInvariant();
            query = query.Where(x => x.CatalogGroup.Slug == groupSlug);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim().ToLower();

            query = query.Where(x =>
                x.Code.ToLower().Contains(value)
                || x.Slug.ToLower().Contains(value)
                || x.Name.ToLower().Contains(value)
            );
        }

        return await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Take(50)
            .Select(x => new CatalogItemSelectDto(
                x.Slug,
                x.Name,
                x.Id,
                x.Code,
                x.Slug,
                x.MetadataJson,
                x.IsActive
            ))
            .ToListAsync(cancellationToken);
    }

    public Task<CatalogItemLookupDto?> GetLookupAsync(
        string catalogGroupSlug,
        string catalogItemSlug,
        CancellationToken cancellationToken = default
    )
    {
        var groupSlug = catalogGroupSlug.Trim().ToLowerInvariant();
        var itemSlug = catalogItemSlug.Trim().ToLowerInvariant();

        return dbContext
            .CatalogItems.AsNoTracking()
            .Where(x =>
                x.CatalogGroup.Slug == groupSlug
                && x.Slug == itemSlug
                && !x.IsDeleted
                && !x.CatalogGroup.IsDeleted
            )
            .Select(x => new CatalogItemLookupDto(
                x.Id,
                x.CatalogGroupId,
                x.CatalogGroup.Code,
                x.CatalogGroup.Slug,
                x.Code,
                x.Slug,
                x.Name,
                x.Value,
                x.MetadataJson,
                x.IsActive && x.CatalogGroup.IsActive
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<CatalogItemLookupDto?> GetLookupByIdAsync(
        Guid catalogItemId,
        CancellationToken cancellationToken = default
    )
    {
        return dbContext
            .CatalogItems.AsNoTracking()
            .Where(x => x.Id == catalogItemId && !x.IsDeleted && !x.CatalogGroup.IsDeleted)
            .Select(x => new CatalogItemLookupDto(
                x.Id,
                x.CatalogGroupId,
                x.CatalogGroup.Code,
                x.CatalogGroup.Slug,
                x.Code,
                x.Slug,
                x.Name,
                x.Value,
                x.MetadataJson,
                x.IsActive && x.CatalogGroup.IsActive
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<CatalogItemLookupDto?> GetLookupByCodeAsync(
        string catalogGroupSlug,
        string catalogItemCode,
        CancellationToken cancellationToken = default
    )
    {
        var groupSlug = catalogGroupSlug.Trim().ToLowerInvariant();
        var itemCode = catalogItemCode.Trim().ToLowerInvariant();

        return dbContext
            .CatalogItems.AsNoTracking()
            .Where(x =>
                x.CatalogGroup.Slug == groupSlug
                && x.Code.ToLower() == itemCode
                && !x.IsDeleted
                && !x.CatalogGroup.IsDeleted
            )
            .Select(x => new CatalogItemLookupDto(
                x.Id,
                x.CatalogGroupId,
                x.CatalogGroup.Code,
                x.CatalogGroup.Slug,
                x.Code,
                x.Slug,
                x.Name,
                x.Value,
                x.MetadataJson,
                x.IsActive && x.CatalogGroup.IsActive
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<CatalogItemLookupDto?> ResolveLookupAsync(
        string catalogGroupSlug,
        string value,
        CancellationToken cancellationToken = default
    )
    {
        var groupSlug = catalogGroupSlug.Trim().ToLowerInvariant();
        var normalizedValue = value.Trim().ToLowerInvariant();

        if (Guid.TryParse(value, out var catalogItemId))
        {
            return dbContext
                .CatalogItems.AsNoTracking()
                .Where(x =>
                    x.Id == catalogItemId
                    && x.CatalogGroup.Slug == groupSlug
                    && !x.IsDeleted
                    && !x.CatalogGroup.IsDeleted
                )
                .Select(x => new CatalogItemLookupDto(
                    x.Id,
                    x.CatalogGroupId,
                    x.CatalogGroup.Code,
                    x.CatalogGroup.Slug,
                    x.Code,
                    x.Slug,
                    x.Name,
                    x.Value,
                    x.MetadataJson,
                    x.IsActive && x.CatalogGroup.IsActive
                ))
                .FirstOrDefaultAsync(cancellationToken);
        }

        return dbContext
            .CatalogItems.AsNoTracking()
            .Where(x =>
                x.CatalogGroup.Slug == groupSlug
                && !x.IsDeleted
                && !x.CatalogGroup.IsDeleted
                && (
                    x.Slug == normalizedValue
                    || x.Code.ToLower() == normalizedValue
                    || x.Name.ToLower() == normalizedValue
                    || (x.Value != null && x.Value.ToLower() == normalizedValue)
                )
            )
            .OrderByDescending(x => x.IsActive && x.CatalogGroup.IsActive)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new CatalogItemLookupDto(
                x.Id,
                x.CatalogGroupId,
                x.CatalogGroup.Code,
                x.CatalogGroup.Slug,
                x.Code,
                x.Slug,
                x.Name,
                x.Value,
                x.MetadataJson,
                x.IsActive && x.CatalogGroup.IsActive
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<CatalogItemLookupDto>> GetActiveLookupsByGroupSlugAsync(
        string catalogGroupSlug,
        CancellationToken cancellationToken = default
    )
    {
        var groupSlug = catalogGroupSlug.Trim().ToLowerInvariant();

        return await dbContext
            .CatalogItems.AsNoTracking()
            .Where(x =>
                x.CatalogGroup.Slug == groupSlug
                && x.IsActive
                && x.CatalogGroup.IsActive
                && !x.IsDeleted
                && !x.CatalogGroup.IsDeleted
            )
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new CatalogItemLookupDto(
                x.Id,
                x.CatalogGroupId,
                x.CatalogGroup.Code,
                x.CatalogGroup.Slug,
                x.Code,
                x.Slug,
                x.Name,
                x.Value,
                x.MetadataJson,
                true
            ))
            .ToListAsync(cancellationToken);
    }

    public Task<bool> IsValidActiveItemAsync(
        string catalogGroupSlug,
        string catalogItemSlug,
        CancellationToken cancellationToken = default
    )
    {
        var groupSlug = catalogGroupSlug.Trim().ToLowerInvariant();
        var itemSlug = catalogItemSlug.Trim().ToLowerInvariant();

        return dbContext.CatalogItems.AnyAsync(
            x =>
                x.CatalogGroup.Slug == groupSlug
                && x.Slug == itemSlug
                && x.IsActive
                && x.CatalogGroup.IsActive
                && !x.IsDeleted
                && !x.CatalogGroup.IsDeleted,
            cancellationToken
        );
    }
}
