namespace Dhole.Config.Application.Abstractions.Codes;

public interface ICodeGenerator
{
    Task<string> GenerateCatalogGroupCodeAsync(
        Func<string, CancellationToken, Task<bool>> existsAsync,
        CancellationToken cancellationToken = default
    );

    Task<string> GenerateCatalogItemCodeAsync(
        string catalogGroupName,
        Func<string, CancellationToken, Task<bool>> existsAsync,
        CancellationToken cancellationToken = default
    );
}
