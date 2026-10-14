using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Config.IntegrationTests;
[TestClass]
public sealed class CrmPhase3CatalogTests
{
    [TestMethod]
    public async Task Commercial_catalogs_exist_and_are_idempotent_on_PostgreSQL()
    {
        var cs = Environment.GetEnvironmentVariable("CRM_CONFIG_TEST_POSTGRES");
        if (string.IsNullOrWhiteSpace(cs)) Assert.Inconclusive("Real PostgreSQL required");
        await using var db = new ServiceDbContext(new DbContextOptionsBuilder<ServiceDbContext>().UseNpgsql(cs).Options);
        await db.Database.MigrateAsync();
        var expected = new Dictionary<string,string[]>
        {
            ["crm-activity-types"] = ["VISIT","CALL","EMAIL","MEETING","OTHER"],
            ["crm-activity-outcomes"] = ["COMPLETED","NO_ANSWER","RESCHEDULED","FOLLOW_UP","OTHER"],
            ["crm-markets"] = ["OTHER"],
            ["crm-opportunity-loss-reasons"] = ["PRICE","TIMING","COMPETITOR","NO_RESPONSE","OTHER"]
        };
        foreach (var pair in expected)
        {
            var result = await (
                from item in db.CatalogItems.AsNoTracking()
                join cat in db.CatalogGroups.AsNoTracking() on item.CatalogGroupId equals cat.Id
                where cat.Slug == pair.Key && cat.IsActive && item.IsActive
                select item.Code
            ).ToListAsync();
            CollectionAssert.AreEquivalent(pair.Value,result.ToArray());
        }
        await db.Database.MigrateAsync();
        Assert.AreEqual(4, await db.CatalogGroups.CountAsync(x => expected.Keys.Contains(x.Slug)));
    }
}
