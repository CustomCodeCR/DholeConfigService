using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Dhole.Config.Api.Authorization;
using Dhole.Config.Application.Abstractions.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace Dhole.Config.Api.Endpoints;

/// <summary>Hacienda taxpayer lookup for SRM; never requires CRM scopes.</summary>
internal static class SrmHaciendaEndpoints
{
    private const string Source = "https://api.hacienda.go.cr/fe/ae";
    private static readonly SemaphoreSlim Gate = new(1, 1);

    internal sealed record Activity(string Code, string Description, string Type);
    internal sealed record Result(string Identification, string Name,
        string? IdentificationType, string? Regime, DateTimeOffset VerifiedAt,
        IReadOnlyList<Activity> Activities);

    public static RouteGroupBuilder MapSrmHaciendaEndpoints(this RouteGroupBuilder routes)
    {
        routes.MapGet("/hacienda/lookup", Lookup).RequireScope("srm.supplier.read");
        return routes;
    }

    private static async Task<IResult> Lookup(
        string identificacion, Guid countryId,
        ICatalogGroupRepository groups, ICatalogItemRepository items,
        IHttpClientFactory clients, IMemoryCache cache, CancellationToken ct)
    {
        var taxId = Regex.Replace(identificacion.Trim(), @"[\s\-]", "");
        if (!Regex.IsMatch(taxId, @"^\d{9,12}$"))
            return Results.BadRequest(new { code = "srm_hacienda_invalid_tax_id" });
        var countryGroup = await groups.GetBySlugAsync("countries", ct);
        var country = await items.GetByIdAsync(countryId, ct);
        if (countryGroup is null || country is null || country.IsDeleted || !country.IsActive ||
            country.CatalogGroupId != countryGroup.Id ||
            !new[] { country.Code, country.Slug, country.Name, country.Value }
                .Any(x => x is not null && new[] { "CR", "CRI", "COSTA RICA", "COSTA-RICA" }
                    .Contains(x.Trim().ToUpperInvariant())))
            return Results.BadRequest(new { code = "srm_hacienda_costa_rica_only" });

        var cacheKey = "srm:hacienda:ae:" + taxId;
        if (cache.TryGetValue(cacheKey, out Result? hit) && hit is not null)
            return Results.Ok(hit);

        await Gate.WaitAsync(ct);
        try
        {
            if (cache.TryGetValue(cacheKey, out hit) && hit is not null)
                return Results.Ok(hit);
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(9));
            using var response = await clients.CreateClient().GetAsync(
                Source + "?identificacion=" + Uri.EscapeDataString(taxId), timeout.Token);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return Results.NotFound(new { code = "srm_hacienda_tax_id_not_found" });
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
                return Results.Problem(statusCode: 503, title: "Hacienda limita temporalmente las consultas.");
            if (!response.IsSuccessStatusCode)
                return Results.Problem(statusCode: 502, title: "No fue posible consultar Hacienda.");
            using var doc = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(timeout.Token), cancellationToken: timeout.Token);
            var root = doc.RootElement;
            string? GetText(string name) => root.TryGetProperty(name, out var node) &&
                node.ValueKind == JsonValueKind.String ? node.GetString() : null;
            var name = GetText("nombre")?.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(name))
                return Results.Problem(statusCode: 502, title: "Hacienda devolvió una respuesta sin razón social.");
            var activities = new List<Activity>();
            if (root.TryGetProperty("actividades", out var rows) &&
                rows.ValueKind == JsonValueKind.Array)
            {
                foreach (var row in rows.EnumerateArray())
                {
                    var code = row.TryGetProperty("codigo", out var c) ? c.ToString().ToUpperInvariant() : "";
                    var description = row.TryGetProperty("descripcion", out var d)
                        ? d.ToString().ToUpperInvariant() : "";
                    var kind = row.TryGetProperty("tipo", out var t) ? t.ToString().ToUpperInvariant() : "";
                    if (code.Length > 0) activities.Add(new Activity(code, description, kind));
                }
            }
            var resolved = new Result(taxId, name, GetText("tipoIdentificacion"),
                GetText("regimen"), DateTimeOffset.UtcNow, activities);
            cache.Set(cacheKey, resolved, TimeSpan.FromHours(12));
            return Results.Ok(resolved);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return Results.Problem(statusCode: 504, title: "Hacienda no respondió a tiempo.");
        }
        catch (HttpRequestException)
        {
            return Results.Problem(statusCode: 502, title: "No fue posible conectar con Hacienda.");
        }
        catch (JsonException)
        {
            return Results.Problem(statusCode: 502, title: "Respuesta inválida de Hacienda.");
        }
        finally { Gate.Release(); }
    }
}
