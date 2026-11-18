using Dhole.Config.Persistence.DbContexts;
using Dhole.Config.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Config.IntegrationTests;

[TestClass]
public sealed class SrmArchivedSupplierNameTests
{
    [TestMethod]
    public async Task Archived_supplier_does_not_reserve_name_for_a_new_supplier()
    {
        var connection = Environment.GetEnvironmentVariable("CRM_CONFIG_TEST_POSTGRES");
        if (string.IsNullOrWhiteSpace(connection))
            Assert.Inconclusive("Real PostgreSQL required to verify the unique index.");

        var options = new DbContextOptionsBuilder<ServiceDbContext>()
            .UseNpgsql(connection).Options;
        await using var db = new ServiceDbContext(options);
        await db.Database.MigrateAsync();

        var groupId = await db.CatalogGroups.AsNoTracking()
            .Where(x => x.Slug == "srm-suppliers" && !x.IsDeleted)
            .Select(x => x.Id).SingleAsync();

        await using var transaction = await db.Database.BeginTransactionAsync();
        var suffix = Guid.NewGuid().ToString("N");
        var name = "PROVEEDOR REUTILIZABLE " + suffix.ToUpperInvariant();
        var archivedId = Guid.NewGuid();
        var freshId = Guid.NewGuid();

        const string emptyMetadata = "{}";
        async Task Insert(Guid id, string code, string slug, bool deleted)
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO config."CatalogItems" (
                    id, catalog_group_id, code, slug, name, metadata_json,
                    sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
                VALUES (
                    {id}, {groupId}, {code}, {slug}, {name}, {emptyMetadata}::jsonb,
                    0, false, true, now(), 'srm-test', {deleted})
                """);
        }

        await Insert(archivedId, "ARCH" + suffix[..12], "arch-" + suffix, true);
        var repository = new CatalogItemRepository(db);
        Assert.IsFalse(await repository.ExistsByNameAsync(groupId, name),
            "A logically deleted supplier must not reserve its name.");

        await Insert(freshId, "LIVE" + suffix[..12], "live-" + suffix, false);
        Assert.IsTrue(await repository.ExistsByNameAsync(groupId, name),
            "An active supplier must reserve its name.");

        var rows = await db.CatalogItems.AsNoTracking()
            .Where(x => x.CatalogGroupId == groupId && x.Name == name)
            .Select(x => new { x.Id, x.IsDeleted }).ToListAsync();
        Assert.HasCount(2, rows, "The old record must remain archived.");
        Assert.IsTrue(rows.Single(x => x.Id == archivedId).IsDeleted);
        Assert.IsFalse(rows.Single(x => x.Id == freshId).IsDeleted);
        await transaction.RollbackAsync();
    }
}
