namespace Dhole.Config.Application.Abstractions.Slugs;

public interface ISlugGenerator
{
    string Generate(string value);

    Task<string> GenerateUniqueCatalogGroupSlugAsync(
        string name,
        Func<string, CancellationToken, Task<bool>> existsAsync,
        CancellationToken cancellationToken = default
    );

    Task<string> GenerateUniqueCatalogItemSlugAsync(
        string name,
        Func<string, CancellationToken, Task<bool>> existsAsync,
        CancellationToken cancellationToken = default
    );
}
