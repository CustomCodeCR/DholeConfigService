using System.Text.Json;
using Dhole.Config.Persistence.Messaging;
using Microsoft.Extensions.Configuration;

namespace Dhole.Config.IntegrationTests;

[TestClass]
public sealed class F6ConfigRealtimeContractTests
{
    [TestMethod]
    public void Config_SRM_flags_and_batch_release_fail_closed()
    {
        Assert.IsFalse(ConfigRealtimeInvalidation.IsEnabled(null));
        var config = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["Features:UniversalRealtimeEnabled"] = "true",
                ["Realtime:Outbox:Enabled"] = "true",
                ["Realtime:BatchApproved"] = "false",
                ["Realtime:ReleaseId"] = ConfigRealtimeInvalidation.ReleaseId
            }).Build();
        Assert.IsFalse(ConfigRealtimeInvalidation.IsEnabled(config));
        config["Realtime:BatchApproved"] = "true";
        Assert.IsTrue(ConfigRealtimeInvalidation.IsEnabled(config));
        config["Realtime:ReleaseId"] = "another-release";
        Assert.IsFalse(ConfigRealtimeInvalidation.IsEnabled(config));
    }

    [TestMethod]
    public void Catalog_items_and_groups_have_safe_distinct_aggregate_types()
    {
        var itemId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        foreach (var kind in new[] { "catalog-group", "catalog-item" })
        {
            var json = ConfigRealtimeInvalidation.Serialize(eventId, itemId, kind,
                ConfigRealtimeInvalidation.NextVersionHint(),
                DateTimeOffset.UtcNow, "config-ci");
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            Assert.AreEqual($"config.{kind}.changed.v1", root.GetProperty("eventType").GetString());
            Assert.AreEqual($"config:{kind}:{itemId:D}", root.GetProperty("audienceRef").GetString());
            Assert.AreEqual(kind, root.GetProperty("aggregateType").GetString());
            Assert.IsGreaterThan(0L, root.GetProperty("aggregateVersion").GetInt64());
            CollectionAssert.AreEquivalent(new[] { "changedFields" },
                root.GetProperty("payload").EnumerateObject().Select(x => x.Name).ToArray());
            foreach (var forbidden in new[] { "supplier", "password", "phone", "contact", "metadataJson" })
                Assert.IsFalse(json.Contains(forbidden, StringComparison.OrdinalIgnoreCase));
        }
    }

    [TestMethod]
    public void Srm_supplier_has_separate_audience_and_the_same_safe_Config_stream()
    {
        var id = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var json = ConfigRealtimeInvalidation.Serialize(
            eventId, id, "srm-supplier",
            ConfigRealtimeInvalidation.NextVersionHint(),
            DateTimeOffset.UtcNow, "srm-ci");
        using var parsed = JsonDocument.Parse(json);
        var root = parsed.RootElement;
        Assert.AreEqual("srm.supplier.changed.v1", root.GetProperty("eventType").GetString());
        Assert.AreEqual("supplier", root.GetProperty("aggregateType").GetString());
        Assert.AreEqual($"srm:supplier:{id:D}", root.GetProperty("audienceRef").GetString());
        foreach (var field in new[] { "metadataJson", "contact", "bankAccount", "phone", "supplierName" })
            Assert.IsFalse(json.Contains(field, StringComparison.OrdinalIgnoreCase), field);
    }

    [TestMethod]
    public void Resource_ids_versions_and_unknown_kinds_are_validated()
    {
        var e = Guid.NewGuid();
        var first = ConfigRealtimeInvalidation.NextVersionHint();
        var second = ConfigRealtimeInvalidation.NextVersionHint();
        Assert.IsGreaterThan(first, second);
        Assert.ThrowsExactly<ArgumentException>(() =>
            ConfigRealtimeInvalidation.EventType("admin"));
        Assert.ThrowsExactly<ArgumentException>(() =>
            ConfigRealtimeInvalidation.Serialize(e, Guid.Empty, "catalog-item", 2,
                DateTimeOffset.UtcNow, "test"));
        Assert.ThrowsExactly<ArgumentException>(() =>
            ConfigRealtimeInvalidation.Serialize(e, e, "catalog-item", 0,
                DateTimeOffset.UtcNow, "test"));
    }
}
