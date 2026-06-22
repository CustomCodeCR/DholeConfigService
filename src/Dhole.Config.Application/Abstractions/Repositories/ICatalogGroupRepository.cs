using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Config.Contracts.Catalogs;
using Dhole.Config.Domain.Catalogs.Entities;

namespace Dhole.Config.Application.Abstractions.Repositories;

public interface ICatalogGroupRepository : IRepository<CatalogGroup, Guid>
{
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<CatalogGroup?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<CatalogGroup?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<PagedResult<CatalogGroupDto>> GetPagedAsync(
        PageRequest page,
        string? search = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<CatalogGroupSelectDto>> GetForSelectAsync(
        string? search = null,
        CancellationToken cancellationToken = default
    );
}
