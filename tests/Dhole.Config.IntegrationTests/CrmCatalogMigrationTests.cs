using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Config.IntegrationTests;

[TestClass]
public sealed class CrmCatalogMigrationTests
{
    [TestMethod]
    public async Task Real_postgres_migration_seeds_exact_phase2_catalog_options_once()
    {
        var connection = Environment.GetEnvironmentVariable("CRM_CONFIG_TEST_POSTGRES");
        if (string.IsNullOrWhiteSpace(connection))
            Assert.Inconclusive("Real test PostgreSQL required.");
        var options = new DbContextOptionsBuilder<ServiceDbContext>().UseNpgsql(connection).Options;
        await using var db = new ServiceDbContext(options);
        await db.Database.MigrateAsync();
        var specs = new Dictionary<string, string[]>
        {
            ["crm-customer-types"] = ["CASH", "CREDIT"],
            ["crm-account-sources"] = ["WEB", "REFERRAL", "PHONE", "EMAIL", "CAMPAIGN", "OTHER"],
            ["crm-relationship-types"] = ["PROSPECT", "CUSTOMER", "PARTNER", "NVOCC", "OTHER"],
            ["crm-customer-categories"] = ["A", "B", "C", "D"],
            ["crm-nvocc-categories"] = ["A", "B"],
            ["crm-contact-channels"] = ["EMAIL", "PHONE", "WHATSAPP", "VISIT", "OTHER"]
        };
        foreach (var (slug, codes) in specs)
        {
            var actual = await (from item in db.CatalogItems.AsNoTracking()
                join group in db.CatalogGroups.AsNoTracking() on item.CatalogGroupId equals group.Id
                where group.Slug == slug && group.IsActive && item.IsActive
                select item.Code).ToListAsync();
            CollectionAssert.AreEquivalent(codes, actual.ToArray(), "Incorrect CRM catalog " + slug);
        }
        await db.Database.MigrateAsync();
        var groups = await db.CatalogGroups.CountAsync(x => specs.Keys.Contains(x.Slug));
        Assert.AreEqual(6, groups, "Catalog seeding must be idempotent.");
    }
}
