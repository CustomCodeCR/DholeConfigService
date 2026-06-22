using System.Globalization;
using System.Text;
using Dhole.Config.Application.Abstractions.Slugs;

namespace Dhole.Config.Application.Slugs;

public sealed class SlugGenerator : ISlugGenerator
{
    public string Generate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "El valor para generar el slug es obligatorio.",
                nameof(value)
            );
        }

        var normalized = RemoveDiacritics(value.Trim()).ToLowerInvariant();

        var builder = new StringBuilder();
        var previousWasSeparator = false;

        foreach (var character in normalized)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                previousWasSeparator = false;
                continue;
            }

            if (!previousWasSeparator)
            {
                builder.Append('-');
                previousWasSeparator = true;
            }
        }

        var slug = builder.ToString().Trim('-');

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new InvalidOperationException("No fue posible generar un slug válido.");
        }

        return slug;
    }

    public Task<string> GenerateUniqueCatalogGroupSlugAsync(
        string name,
        Func<string, CancellationToken, Task<bool>> existsAsync,
        CancellationToken cancellationToken = default
    )
    {
        return GenerateUniqueAsync(name, existsAsync, cancellationToken);
    }

    public Task<string> GenerateUniqueCatalogItemSlugAsync(
        string name,
        Func<string, CancellationToken, Task<bool>> existsAsync,
        CancellationToken cancellationToken = default
    )
    {
        return GenerateUniqueAsync(name, existsAsync, cancellationToken);
    }

    private async Task<string> GenerateUniqueAsync(
        string value,
        Func<string, CancellationToken, Task<bool>> existsAsync,
        CancellationToken cancellationToken
    )
    {
        var baseSlug = Generate(value);

        if (!await existsAsync(baseSlug, cancellationToken))
        {
            return baseSlug;
        }

        for (var number = 2; number <= 9999; number++)
        {
            var slug = $"{baseSlug}-{number}";

            if (!await existsAsync(slug, cancellationToken))
            {
                return slug;
            }
        }

        throw new InvalidOperationException($"No hay slugs disponibles para '{baseSlug}'.");
    }

    private static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var character in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);

            if (category != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
