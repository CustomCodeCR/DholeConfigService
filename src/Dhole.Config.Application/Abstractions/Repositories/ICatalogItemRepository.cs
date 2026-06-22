using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Config.Contracts.Catalogs;
using Dhole.Config.Domain.Catalogs.Entities;

namespace Dhole.Config.Application.Abstractions.Repositories;

public interface ICatalogItemRepository : IRepository<CatalogItem, Guid>
{
    Task<bool> ExistsByCodeAsync(
        Guid catalogGroupId,
        string code,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsBySlugAsync(
        Guid catalogGroupId,
        string slug,
        CancellationToken cancellationToken = default
    );

    Task<CatalogItem?> GetByCodeAsync(
        Guid catalogGroupId,
        string code,
        CancellationToken cancellationToken = default
    );

    Task<CatalogItem?> GetBySlugAsync(
        Guid catalogGroupId,
        string slug,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<CatalogItemDto>> GetActiveByGroupSlugAsync(
        string catalogGroupSlug,
        CancellationToken cancellationToken = default
    );

    Task<PagedResult<CatalogItemDto>> GetPagedAsync(
        PageRequest page,
        Guid? catalogGroupId = null,
        string? search = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<CatalogItemSelectDto>> GetForSelectAsync(
        Guid? catalogGroupId = null,
        string? catalogGroupSlug = null,
        string? search = null,
        CancellationToken cancellationToken = default
    );

    Task<CatalogItemLookupDto?> GetLookupAsync(
        string catalogGroupSlug,
        string catalogItemSlug,
        CancellationToken cancellationToken = default
    );

    Task<bool> IsValidActiveItemAsync(
        string catalogGroupSlug,
        string catalogItemSlug,
        CancellationToken cancellationToken = default
    );
}
