using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Postgres.EntityFramework.Repositories;
using Dhole.Config.Application.Abstractions.Repositories;
using Dhole.Config.Contracts.Catalogs;
using Dhole.Config.Domain.Catalogs.Entities;
using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Config.Persistence.Repositories;

public sealed class CatalogGroupRepository(ServiceDbContext dbContext)
    : EfRepository<CatalogGroup, Guid>(dbContext),
        ICatalogGroupRepository
{
    public Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var value = code.Trim();

        return dbContext.CatalogGroups.AnyAsync(
            x => x.Code == value && !x.IsDeleted,
            cancellationToken
        );
    }

    public Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var value = slug.Trim().ToLowerInvariant();

        return dbContext.CatalogGroups.AnyAsync(
            x => x.Slug == value && !x.IsDeleted,
            cancellationToken
        );
    }

    public Task<CatalogGroup?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default
    )
    {
        var value = code.Trim();

        return dbContext.CatalogGroups.FirstOrDefaultAsync(
            x => x.Code == value && !x.IsDeleted,
            cancellationToken
        );
    }

    public Task<CatalogGroup?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default
    )
    {
        var value = slug.Trim().ToLowerInvariant();

        return dbContext.CatalogGroups.FirstOrDefaultAsync(
            x => x.Slug == value && !x.IsDeleted,
            cancellationToken
        );
    }

    public async Task<PagedResult<CatalogGroupDto>> GetPagedAsync(
        PageRequest page,
        string? search = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = dbContext.CatalogGroups.AsNoTracking().Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim().ToLower();

            query = query.Where(x =>
                x.Code.ToLower().Contains(value)
                || x.Slug.ToLower().Contains(value)
                || x.Name.ToLower().Contains(value)
                || (x.Description != null && x.Description.ToLower().Contains(value))
            );
        }

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Name)
            .Skip(page.Skip)
            .Take(page.PageSize)
            .Select(x => new CatalogGroupDto(
                x.Id,
                x.Code,
                x.Slug,
                x.Name,
                x.Description,
                x.MetadataJson,
                x.IsSystem,
                x.IsActive,
                x.CreatedAtUtc,
                x.UpdatedAtUtc
            ))
            .ToListAsync(cancellationToken);

        return PagedResult<CatalogGroupDto>.Create(items, page.PageNumber, page.PageSize, total);
    }

    public async Task<IReadOnlyCollection<CatalogGroupSelectDto>> GetForSelectAsync(
        string? search = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = dbContext.CatalogGroups.AsNoTracking().Where(x => !x.IsDeleted && x.IsActive);

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
            .OrderBy(x => x.Name)
            .Take(50)
            .Select(x => new CatalogGroupSelectDto(
                x.Id,
                x.Code,
                x.Slug,
                x.Name,
                x.IsSystem,
                x.IsActive
            ))
            .ToListAsync(cancellationToken);
    }
}
