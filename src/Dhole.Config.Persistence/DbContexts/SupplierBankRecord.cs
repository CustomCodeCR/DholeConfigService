namespace Dhole.Config.Persistence.DbContexts;

/// <summary>
/// Sensitive supplier bank details are stored separately from public catalog metadata.
/// Ciphertext is authenticated with supplier id as AAD; keys are supplied from the environment.
/// </summary>
public sealed class SupplierBankRecord
{
    public Guid SupplierId { get; set; }
    public byte[] Ciphertext { get; set; } = [];
    public byte[] Nonce { get; set; } = [];
    public byte[] Tag { get; set; } = [];
    public DateTime UpdatedAtUtc { get; set; }
}
