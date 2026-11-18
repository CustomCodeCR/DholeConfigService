using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Dhole.Config.Persistence.Migrations;

/// <summary>
/// An archived supplier keeps its identity, links and audit history, while the
/// displayed name can be used by a fresh SRM supplier. Do not alter archived data.
/// </summary>
[DbContext(typeof(ServiceDbContext))]
[Migration("20261117193000_AllowArchivedSrmSupplierNameReuse")]
public sealed class AllowArchivedSrmSupplierNameReuse : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_CatalogItems_catalog_group_id_name",
            schema: "config",
            table: "CatalogItems");
        migrationBuilder.CreateIndex(
            name: "IX_CatalogItems_catalog_group_id_name",
            schema: "config",
            table: "CatalogItems",
            columns: new[] { "catalog_group_id", "name" },
            unique: true,
            filter: "is_deleted = false");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // This rollback requires cleaning up any active/archived duplicate names.
        // Fail explicitly instead of silently deleting the archived history.
        migrationBuilder.Sql("""
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM config."CatalogItems"
        GROUP BY catalog_group_id, name
        HAVING COUNT(*) > 1
    ) THEN
        RAISE EXCEPTION 'Cannot restore full supplier-name uniqueness while duplicate names exist';
    END IF;
END $$;
""");
        migrationBuilder.DropIndex(
            name: "IX_CatalogItems_catalog_group_id_name",
            schema: "config",
            table: "CatalogItems");
        migrationBuilder.CreateIndex(
            name: "IX_CatalogItems_catalog_group_id_name",
            schema: "config",
            table: "CatalogItems",
            columns: new[] { "catalog_group_id", "name" },
            unique: true);
    }
}
