using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260829185800_HideLegacyEmployeeDirectoryCatalog")]
public sealed class HideLegacyEmployeeDirectoryCatalog : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE config."CatalogItems" i
            SET is_active = FALSE,
                is_deleted = TRUE,
                deleted_at_utc = NOW(),
                deleted_by = 'migration',
                updated_at_utc = NOW(),
                updated_by = 'migration'
            FROM config."CatalogGroups" g
            WHERE i.catalog_group_id = g.id
              AND g.slug = 'employee-directory'
              AND i.is_deleted = FALSE;

            UPDATE config."CatalogGroups"
            SET is_active = FALSE,
                is_deleted = TRUE,
                deleted_at_utc = NOW(),
                deleted_by = 'migration',
                updated_at_utc = NOW(),
                updated_by = 'migration'
            WHERE slug = 'employee-directory'
              AND is_deleted = FALSE;
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE config."CatalogGroups"
            SET is_active = TRUE,
                is_deleted = FALSE,
                deleted_at_utc = NULL,
                deleted_by = NULL
            WHERE slug = 'employee-directory';

            UPDATE config."CatalogItems" i
            SET is_active = TRUE,
                is_deleted = FALSE,
                deleted_at_utc = NULL,
                deleted_by = NULL
            FROM config."CatalogGroups" g
            WHERE i.catalog_group_id = g.id
              AND g.slug = 'employee-directory';
            """
        );
    }
}
