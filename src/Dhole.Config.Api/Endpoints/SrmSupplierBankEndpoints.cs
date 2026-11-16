using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using CustomCodeFramework.Messaging.Outbox;
using Dhole.Config.Api.Authorization;
using Dhole.Config.Application.Abstractions.Repositories;
using Dhole.Config.Persistence.DbContexts;
using Dhole.Config.Persistence.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Config.Api.Endpoints;

/// <summary>
/// Dedicated finance-only API. Bank account numbers NEVER appear in the Config
/// catalog MetadataJson, pricing suppliers, general SRM listings or event payloads.
/// </summary>
internal static class SrmSupplierBankEndpoints
{
    private const int MaximumAccounts = 20;
    private const string KeyVariable = "DHOLE_SRM_BANK_KEY_BASE64";

    internal sealed record BankAccount(
        Guid Id,
        string BankName,
        string AccountHolder,
        string Registration,
        string AccountNumber,
        string? SwiftBic,
        string Currency,
        Guid BankCountryId,
        string? IntermediaryBank,
        bool IsPrimary);

    public static RouteGroupBuilder MapSrmSupplierBankEndpoints(this RouteGroupBuilder routes)
    {
        routes.MapGet("/suppliers/{supplierId:guid}/bank-accounts", Read)
            .RequireScope("srm.supplier.bank.read");
        routes.MapPut("/suppliers/{supplierId:guid}/bank-accounts", Save)
            .RequireScope("srm.supplier.bank.manage");
        return routes;
    }

    private static byte[]? ReadKey(IConfiguration configuration)
    {
        var raw = configuration[KeyVariable];
        if (string.IsNullOrWhiteSpace(raw)) return null;
        try
        {
            var key = Convert.FromBase64String(raw);
            return key.Length == 32 ? key : null;
        }
        catch (FormatException) { return null; }
    }

    private static async Task<bool> Exists(Guid supplierId, ICatalogGroupRepository groups,
        ICatalogItemRepository items, CancellationToken ct)
    {
        var group = await groups.GetBySlugAsync("srm-suppliers", ct);
        var item = await items.GetByIdAsync(supplierId, ct);
        return group is not null && item is not null && !item.IsDeleted &&
            item.CatalogGroupId == group.Id;
    }

    private static async Task<IResult> Read(
        Guid supplierId, ICatalogGroupRepository groups, ICatalogItemRepository items,
        ServiceDbContext db, IConfiguration configuration, HttpContext context, CancellationToken ct)
    {
        context.Response.Headers["Cache-Control"] = "no-store";
        if (!await Exists(supplierId, groups, items, ct))
            return Results.NotFound(new { code = "srm_supplier_not_found" });
        var record = await db.SupplierBankRecords.AsNoTracking()
            .SingleOrDefaultAsync(x => x.SupplierId == supplierId, ct);
        if (record is null) return Results.Ok(Array.Empty<BankAccount>());
        var key = ReadKey(configuration);
        if (key is null) return Results.Problem(statusCode: 503,
            title: "Falta configurar la llave segura de cuentas bancarias SRM.");
        try
        {
            var bytes = new byte[record.Ciphertext.Length];
            using var aes = new AesGcm(key, 16);
            aes.Decrypt(record.Nonce, record.Ciphertext, record.Tag, bytes,
                Encoding.UTF8.GetBytes(supplierId.ToString("N")));
            return Results.Ok(JsonSerializer.Deserialize<BankAccount[]>(bytes) ?? []);
        }
        catch (CryptographicException)
        {
            return Results.Problem(statusCode: 503,
                title: "No fue posible descifrar las cuentas con la llave configurada.");
        }
        catch (JsonException)
        {
            return Results.Problem(statusCode: 503,
                title: "El registro bancario está dañado.");
        }
    }

    private static async Task<IResult> Save(
        Guid supplierId, BankAccount[] accounts,
        ICatalogGroupRepository groups, ICatalogItemRepository items,
        ServiceDbContext db, IConfiguration configuration, HttpContext context, CancellationToken ct)
    {
        context.Response.Headers["Cache-Control"] = "no-store";
        if (!await Exists(supplierId, groups, items, ct))
            return Results.NotFound(new { code = "srm_supplier_not_found" });
        if (accounts.Length > MaximumAccounts)
            return Results.BadRequest(new { code = "srm_too_many_bank_accounts" });
        if (accounts.Length > 0 && accounts.Count(a => a.IsPrimary) != 1)
            return Results.BadRequest(new { code = "srm_one_primary_bank_account_required" });
        if (accounts.Select(x => x.Id).Distinct().Count() != accounts.Length ||
            accounts.Any(a => a.Id == Guid.Empty))
            return Results.BadRequest(new { code = "srm_duplicate_bank_account_id" });

        var countryGroup = await groups.GetBySlugAsync("countries", ct);
        if (countryGroup is null) return Results.Problem(statusCode: 503,
            title: "El catálogo de países no está disponible.");

        var normalized = new List<BankAccount>(accounts.Length);
        var fingerprints = new HashSet<string>(StringComparer.Ordinal);
        foreach (var account in accounts)
        {
            static string Upper(string? value) => (value ?? "").Trim().ToUpperInvariant();
            var bank = Upper(account.BankName);
            var holder = Upper(account.AccountHolder);
            var registration = Upper(account.Registration);
            var number = Upper(account.AccountNumber).Replace(" ", "");
            var swift = Upper(account.SwiftBic);
            var currency = Upper(account.Currency);
            var intermediary = Upper(account.IntermediaryBank);
            var country = await items.GetByIdAsync(account.BankCountryId, ct);
            if (bank.Length is < 2 or > 160 || holder.Length is < 2 or > 180 ||
                registration is not ("LOCAL" or "INTERNACIONAL") ||
                !Regex.IsMatch(number, @"^[A-Z0-9\-]{5,44}$") ||
                (swift.Length != 0 && !Regex.IsMatch(swift, @"^[A-Z0-9]{8}([A-Z0-9]{3})?$")) ||
                (registration == "INTERNACIONAL" && swift.Length == 0) ||
                !Regex.IsMatch(currency, @"^[A-Z]{3}$") ||
                intermediary.Length > 180 ||
                country is null || country.IsDeleted || !country.IsActive ||
                country.CatalogGroupId != countryGroup.Id)
                return Results.BadRequest(new { code = "srm_invalid_bank_account" });
            if (!fingerprints.Add(country.Id + ":" + number))
                return Results.BadRequest(new { code = "srm_duplicate_bank_account" });
            normalized.Add(account with {
                BankName = bank, AccountHolder = holder, Registration = registration,
                AccountNumber = number, SwiftBic = swift.Length == 0 ? null : swift,
                Currency = currency, IntermediaryBank = intermediary.Length == 0 ? null : intermediary,
            });
        }

        var key = ReadKey(configuration);
        if (key is null) return Results.Problem(statusCode: 503,
            title: "Defina DHOLE_SRM_BANK_KEY_BASE64 (32 bytes Base64) antes de guardar cuentas bancarias.");

        var plaintext = JsonSerializer.SerializeToUtf8Bytes(normalized);
        var nonce = RandomNumberGenerator.GetBytes(12);
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[16];
        using (var aes = new AesGcm(key, 16))
            aes.Encrypt(nonce, plaintext, ciphertext, tag,
                Encoding.UTF8.GetBytes(supplierId.ToString("N")));

        var existing = await db.SupplierBankRecords.SingleOrDefaultAsync(x => x.SupplierId == supplierId, ct);
        if (existing is null)
        {
            db.SupplierBankRecords.Add(new SupplierBankRecord {
                SupplierId = supplierId, Ciphertext = ciphertext, Nonce = nonce,
                Tag = tag, UpdatedAtUtc = DateTime.UtcNow,
            });
        }
        else
        {
            existing.Ciphertext = ciphertext;
            existing.Nonce = nonce;
            existing.Tag = tag;
            existing.UpdatedAtUtc = DateTime.UtcNow;
        }
        // Existing durable SRM invalidation contract; no bank details in Redis.
        if (ConfigRealtimeInvalidation.IsEnabled(configuration))
        {
            var eventId = Guid.NewGuid();
            var correlationId = Guid.NewGuid().ToString();
            db.OutboxMessages.Add(new OutboxMessage
            {
                EventId = eventId,
                EventType = ConfigRealtimeInvalidation.EventType("srm-supplier"),
                EventName = "srm.supplier.changed.v1",
                SourceService = "DholeConfigService",
                PayloadJson = ConfigRealtimeInvalidation.Serialize(
                    eventId, supplierId, "srm-supplier",
                    ConfigRealtimeInvalidation.NextVersionHint(), DateTimeOffset.UtcNow, correlationId),
                HeadersJson = null,
                CorrelationId = correlationId,
                Status = OutboxMessageStatus.Pending,
                RetryCount = 0,
                ErrorMessage = null,
                CreatedAtUtc = DateTime.UtcNow,
            });
        }
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }
}
