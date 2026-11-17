using Dhole.Config.Api.Endpoints;

namespace Dhole.Config.IntegrationTests;

[TestClass]
public sealed class SrmIbanRulesTests
{
    [TestMethod]
    public void CountryLengthAndAliasesMatchIso13616()
    {
        Assert.IsTrue(SrmIbanRules.TryGetFormat("CRI", "Costa Rica", null, null, out var cr, out var crLength));
        Assert.AreEqual("CR", cr);
        Assert.AreEqual(22, crLength);
        Assert.IsTrue(SrmIbanRules.TryGetFormat("DE", null, null, null, out var de, out var deLength));
        Assert.AreEqual("DE", de);
        Assert.AreEqual(22, deLength);
        Assert.IsTrue(SrmIbanRules.TryGetFormat("ESP", null, null, null, out var es, out var esLength));
        Assert.AreEqual("ES", es);
        Assert.AreEqual(24, esLength);
        Assert.IsTrue(SrmIbanRules.TryGetFormat("NIC", null, null, null, out var ni, out var niLength));
        Assert.AreEqual("NI", ni);
        Assert.AreEqual(28, niLength);
        Assert.IsFalse(SrmIbanRules.TryGetFormat("US", null, null, "United States", out _, out _));
    }

    [TestMethod]
    public void CostaRicanIbanMustHavePrefixLengthAndCorrectCheckDigits()
    {
        const string valid = "CR05015202001026284066";
        Assert.IsTrue(SrmIbanRules.IsValid(valid, "CR", "Costa Rica", null, null));
        Assert.IsTrue(SrmIbanRules.IsValid("CR05 0152-0200 1026 2840 66", "CRI", null, null, null));
        foreach (var value in new[] {
            "CR0501520200102628406",
            "CR050152020010262840669",
            "CR05015202001026284067",
            "DE89370400440532013000",
        })
            Assert.IsFalse(SrmIbanRules.IsValid(value, "CR", null, null, null));
    }

    [TestMethod]
    public void DifferentIbanCountriesAndNationalAccountsStayIndependent()
    {
        Assert.IsTrue(SrmIbanRules.IsValid("DE89370400440532013000", "DE", null, null, null));
        Assert.IsTrue(SrmIbanRules.IsValid("GB82 WEST 1234 5698 7654 32", "GB", null, null, null));
        Assert.IsFalse(SrmIbanRules.IsValid("DE89370400440532013000", "ES", null, null, null));
        Assert.IsTrue(SrmIbanRules.IsValid("123456789", "US", null, null, "Estados Unidos"));
        Assert.IsFalse(SrmIbanRules.IsValid("1234", "US", null, null, null));
        Assert.IsFalse(SrmIbanRules.IsValid(new string('9', 45), "US", null, null, null));
    }
}
