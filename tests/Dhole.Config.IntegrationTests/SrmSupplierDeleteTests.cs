using Dhole.Config.Api.Endpoints;

namespace Dhole.Config.IntegrationTests;

[TestClass]
public sealed class SrmSupplierDeleteTests
{
    [TestMethod]
    public void SupplierDependenciesAreMatchedByExactId()
    {
        var supplierId = Guid.Parse("07785485-cc18-42f8-9fed-8458d75cf404");
        var otherSupplier = Guid.Parse("174452b9-f026-4920-9627-37bc324882ab");

        Assert.IsTrue(SrmCatalogEndpoints.HasSupplierReference(
            $$"""{"supplierId":"{{supplierId}}","score":4}""", supplierId));
        Assert.IsFalse(SrmCatalogEndpoints.HasSupplierReference(
            $$"""{"supplierId":"{{otherSupplier}}"}""", supplierId));
        Assert.IsFalse(SrmCatalogEndpoints.HasSupplierReference(
            """{"supplierId":"not-a-guid"}""", supplierId));
        Assert.IsFalse(SrmCatalogEndpoints.HasSupplierReference(
            """{"legacyConfigId":"07785485-cc18-42f8-9fed-8458d75cf404"}""", supplierId));
        Assert.IsFalse(SrmCatalogEndpoints.HasSupplierReference("invalid-json", supplierId));
        Assert.IsFalse(SrmCatalogEndpoints.HasSupplierReference(null, supplierId));
    }
}
