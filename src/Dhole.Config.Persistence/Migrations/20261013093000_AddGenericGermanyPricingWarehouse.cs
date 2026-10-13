using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20261013093000_AddGenericGermanyPricingWarehouse")]
public sealed class AddGenericGermanyPricingWarehouse : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            -- Country-level WHS placeholder. No street address, city, coordinates
            -- or operational contacts are known for this generic location.
            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json,
                 sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT
                'c2130000-0000-4000-8000-000000000001'::uuid,
                grp.id,
                'WHS_GERMANY_GENERIC',
                'whs-germany-generic-de',
                'Alemania (WHS genérico)',
                'WHS genérico de Alemania, sin dirección física registrada.',
                'Alemania',
                '{"country":"Alemania","countryCode":"DE"}'::jsonb,
                190,
                FALSE,
                TRUE,
                NOW(),
                'migration',
                FALSE
            FROM config."CatalogGroups" AS grp
            WHERE grp.slug = 'pricing-warehouses'
              AND grp.is_deleted = FALSE
              AND NOT EXISTS (
                  SELECT 1
                  FROM config."CatalogItems" AS existing
                  WHERE existing.catalog_group_id = grp.id
                    AND existing.is_deleted = FALSE
                    AND (
                        UPPER(existing.code) = 'WHS_GERMANY_GENERIC'
                        OR LOWER(existing.slug) = 'whs-germany-generic-de'
                        OR LOWER(existing.name) = LOWER('Alemania (WHS genérico)')
                    )
              )
            ON CONFLICT DO NOTHING;
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DELETE FROM config."CatalogItems"
            WHERE id = 'c2130000-0000-4000-8000-000000000001'::uuid
              AND code = 'WHS_GERMANY_GENERIC'
              AND created_by = 'migration';
            """
        );
    }
}
