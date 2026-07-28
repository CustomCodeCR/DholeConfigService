using System.Globalization;
using System.Text;
using System.Text.Json;
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

    public async Task<CatalogItemLookupDto?> ResolveLookupAsync(
        string catalogGroupSlug,
        string value,
        CancellationToken cancellationToken = default
    )
    {
        var groupSlug = catalogGroupSlug.Trim().ToLowerInvariant();
        var normalizedValue = value.Trim().ToLowerInvariant();

        if (Guid.TryParse(value, out var catalogItemId))
        {
            return await dbContext
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

        var exact = await dbContext
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

        if (exact is not null)
        {
            return exact;
        }

        // Segunda etapa: comparación canónica contra los elementos activos de Config.
        // Tolera acentos, separadores, aliases, nombres compuestos y errores menores,
        // manteniendo un margen mínimo para no resolver coincidencias ambiguas.
        var activeItems = await GetActiveLookupsByGroupSlugAsync(groupSlug, cancellationToken);
        var lookupKey = NormalizeLookupKey(value);
        if (string.IsNullOrWhiteSpace(lookupKey))
        {
            return null;
        }

        // Nunca se resuelve un agente por similitud, tokens o contenido parcial.
        // Solo se admite código, slug, nombre, value o alias completo y único.
        if (groupSlug.Equals("agents", StringComparison.OrdinalIgnoreCase))
        {
            var strictMatches = activeItems
                .Where(item =>
                    GetLookupValues(item).Any(candidate =>
                        NormalizeLookupKey(candidate).Equals(
                            lookupKey,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                )
                .DistinctBy(item => item.Id)
                .Take(2)
                .ToArray();

            return strictMatches.Length == 1 ? strictMatches[0] : null;
        }

        var lookupKeys = BuildLookupKeys(value, groupSlug);
        var matches = activeItems
            .Where(item => GetLookupValues(item)
                .SelectMany(candidate => BuildLookupKeys(candidate, groupSlug))
                .Any(lookupKeys.Contains))
            .DistinctBy(item => item.Id)
            .Take(2)
            .ToArray();

        if (matches.Length == 1)
        {
            return matches[0];
        }

        if (matches.Length > 1 || !IsSafeContainmentKey(lookupKey))
        {
            return null;
        }

        // Coincidencia direccional: el valor almacenado en Config contiene el valor
        // recibido. Permite resolver, por ejemplo, "MOIN" contra "Puerto de Moín"
        // y "COLON" o "MANZANILLO" contra "Colón/Manzanillo".
        var containsMatches = activeItems
            .Select(item => new
            {
                Item = item,
                Distance = GetLookupValues(item)
                    .SelectMany(candidate => BuildLookupKeys(candidate, groupSlug))
                    .Where(IsSafeContainmentKey)
                    .SelectMany(candidate => lookupKeys
                        .Where(IsSafeContainmentKey)
                        .Where(input =>
                            candidate.Contains(input, StringComparison.OrdinalIgnoreCase)
                            || input.Contains(candidate, StringComparison.OrdinalIgnoreCase)
                        )
                        .Select(input => (int?)Math.Abs(candidate.Length - input.Length)))
                    .OrderBy(distance => distance)
                    .FirstOrDefault(),
            })
            .Where(result => result.Distance.HasValue)
            .OrderBy(result => result.Distance!.Value)
            .ThenBy(result => result.Item.Name)
            .ToArray();

        if (containsMatches.Length == 1)
        {
            return containsMatches[0].Item;
        }

        if (
            containsMatches.Length > 1
            && containsMatches[0].Distance!.Value < containsMatches[1].Distance!.Value
        )
        {
            return containsMatches[0].Item;
        }

        var scored = activeItems
            .Select(item => new
            {
                Item = item,
                Score = CalculateLookupScore(value, groupSlug, item),
            })
            .Where(result => result.Score >= MinimumLookupScore(groupSlug))
            .OrderByDescending(result => result.Score)
            .ThenBy(result => result.Item.Name)
            .ToArray();

        if (scored.Length == 0)
        {
            return null;
        }

        return scored.Length == 1 || scored[0].Score - scored[1].Score >= 0.06m
            ? scored[0].Item
            : null;
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
    private static IEnumerable<string> GetLookupValues(CatalogItemLookupDto item)
    {
        yield return item.Code;
        yield return item.Slug;
        yield return item.Name;

        if (!string.IsNullOrWhiteSpace(item.Value))
        {
            yield return item.Value;
        }

        foreach (var alias in ReadAliases(item.MetadataJson))
        {
            yield return alias;
        }
    }

    private static IReadOnlyCollection<string> ReadAliases(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
        {
            return [];
        }

        try
        {
            using var document = JsonDocument.Parse(metadataJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return [];
            }

            var aliases = new List<string>();
            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (!IsAliasProperty(property.Name))
                {
                    continue;
                }

                if (property.Value.ValueKind == JsonValueKind.Array)
                {
                    aliases.AddRange(
                        property.Value
                            .EnumerateArray()
                            .Where(element => element.ValueKind == JsonValueKind.String)
                            .Select(element => element.GetString())
                            .Where(alias => !string.IsNullOrWhiteSpace(alias))
                            .Select(alias => alias!)
                    );
                }
                else if (property.Value.ValueKind == JsonValueKind.String)
                {
                    aliases.AddRange(
                        (property.Value.GetString() ?? string.Empty).Split(
                            [',', ';', '|'],
                            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                        )
                    );
                }
            }

            return aliases.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static bool IsAliasProperty(string name)
    {
        return name.Equals("aliases", StringComparison.OrdinalIgnoreCase)
            || name.Equals("alias", StringComparison.OrdinalIgnoreCase)
            || name.Equals("synonyms", StringComparison.OrdinalIgnoreCase)
            || name.Equals("alternativeNames", StringComparison.OrdinalIgnoreCase)
            || name.Equals("abbreviations", StringComparison.OrdinalIgnoreCase)
            || name.Equals("keywords", StringComparison.OrdinalIgnoreCase)
            || name.Equals("searchTerms", StringComparison.OrdinalIgnoreCase)
            || name.Equals("codes", StringComparison.OrdinalIgnoreCase)
            || name.Equals("unlocodes", StringComparison.OrdinalIgnoreCase);
    }

    private static HashSet<string> BuildLookupKeys(string value, string groupSlug)
    {
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var normalized = NormalizeLookupKey(value);
        if (!string.IsNullOrWhiteSpace(normalized))
        {
            keys.Add(normalized);
        }

        foreach (var token in TokenizeLookupValue(value, groupSlug))
        {
            keys.Add(token);
        }

        return keys;
    }

    private static HashSet<string> TokenizeLookupValue(string value, string groupSlug)
    {
        var decomposed = value.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);

        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            builder.Append(char.IsLetterOrDigit(character)
                ? char.ToUpperInvariant(character)
                : ' ');
        }

        var tokens = builder
            .ToString()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(token => token.Length >= 3)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (IsPortCatalog(groupSlug))
        {
            tokens.ExceptWith(
                [
                    "PORT",
                    "PUERTO",
                    "PTO",
                    "TERMINAL",
                    "PORTOF",
                    "DEL",
                    "THE",
                    "MARITIME",
                    "MARITIMO",
                    "MARITIMA",
                ]
            );
        }
        else if (groupSlug.Equals("carriers", StringComparison.OrdinalIgnoreCase))
        {
            tokens.ExceptWith(
                [
                    "LINE",
                    "LINES",
                    "SHIPPING",
                    "COMPANY",
                    "LIMITED",
                    "CORPORATION",
                    "GROUP",
                ]
            );
        }

        return tokens;
    }

    private static decimal CalculateLookupScore(
        string value,
        string groupSlug,
        CatalogItemLookupDto item
    )
    {
        var inputKeys = BuildLookupKeys(value, groupSlug);
        var candidateKeys = GetLookupValues(item)
            .SelectMany(candidate => BuildLookupKeys(candidate, groupSlug))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var best = 0m;

        foreach (var input in inputKeys.Where(key => key.Length >= 3))
        {
            foreach (var candidate in candidateKeys.Where(key => key.Length >= 3))
            {
                if (input.Equals(candidate, StringComparison.OrdinalIgnoreCase))
                {
                    return 1m;
                }

                var shortest = Math.Min(input.Length, candidate.Length);
                var longest = Math.Max(input.Length, candidate.Length);
                if (shortest >= 4
                    && (
                        input.Contains(candidate, StringComparison.OrdinalIgnoreCase)
                        || candidate.Contains(input, StringComparison.OrdinalIgnoreCase)
                    ))
                {
                    best = Math.Max(best, 0.86m + (0.12m * decimal.Divide(shortest, longest)));
                }

                if (shortest >= 4)
                {
                    best = Math.Max(best, CalculateSimilarity(input, candidate) * 0.98m);
                }
            }
        }

        return Math.Min(best, 1m);
    }

    private static decimal MinimumLookupScore(string groupSlug)
    {
        return groupSlug.ToLowerInvariant() switch
        {
            "currencies" => 0.94m,
            "container-types" => 0.88m,
            "agents" => 0.84m,
            "carriers" => 0.82m,
            "pol" or "poe" or "pod" => 0.78m,
            _ => 0.86m,
        };
    }

    private static bool IsPortCatalog(string groupSlug)
    {
        return groupSlug.Equals("pol", StringComparison.OrdinalIgnoreCase)
            || groupSlug.Equals("poe", StringComparison.OrdinalIgnoreCase)
            || groupSlug.Equals("pod", StringComparison.OrdinalIgnoreCase);
    }

    private static decimal CalculateSimilarity(string left, string right)
    {
        var distance = LevenshteinDistance(left, right);
        var maxLength = Math.Max(left.Length, right.Length);
        return maxLength == 0 ? 1m : 1m - decimal.Divide(distance, maxLength);
    }

    private static int LevenshteinDistance(string left, string right)
    {
        var previous = new int[right.Length + 1];
        var current = new int[right.Length + 1];

        for (var index = 0; index <= right.Length; index++)
        {
            previous[index] = index;
        }

        for (var leftIndex = 1; leftIndex <= left.Length; leftIndex++)
        {
            current[0] = leftIndex;

            for (var rightIndex = 1; rightIndex <= right.Length; rightIndex++)
            {
                var substitutionCost = left[leftIndex - 1] == right[rightIndex - 1] ? 0 : 1;
                current[rightIndex] = Math.Min(
                    Math.Min(current[rightIndex - 1] + 1, previous[rightIndex] + 1),
                    previous[rightIndex - 1] + substitutionCost
                );
            }

            (previous, current) = (current, previous);
        }

        return previous[right.Length];
    }

    private static bool IsSafeContainmentKey(string key)
    {
        return key.Length >= 4
            && key.Any(char.IsLetter)
            && !Guid.TryParseExact(key, "N", out _);
    }

    private static string NormalizeLookupKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var decomposed = value.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);

        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(character))
            {
                builder.Append(char.ToUpperInvariant(character));
            }
        }

        return builder.ToString();
    }

}
