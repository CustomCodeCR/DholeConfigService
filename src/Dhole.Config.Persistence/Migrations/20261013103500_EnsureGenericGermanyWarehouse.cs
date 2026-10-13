using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20261013103500_EnsureGenericGermanyWarehouse")]
public sealed class EnsureGenericGermanyWarehouse : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            -- Normalize the existing migration-created placeholder, if present.
            -- Do not overwrite manually created or edited warehouse records.
            UPDATE config."CatalogItems" AS item
            SET name = 'Alemania',
                value = 'Alemania',
                description = NULL,
                metadata_json = '{"isGeneric":true,"countryCode":"DE"}'::jsonb,
                is_active = TRUE,
                is_deleted = FALSE,
                deleted_at_utc = NULL,
                deleted_by = NULL,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            FROM config."CatalogGroups" AS grp
            WHERE item.catalog_group_id = grp.id
              AND grp.slug = 'pricing-warehouses'
              AND item.code = 'WHS_GERMANY_GENERIC'
              AND item.created_by = 'migration'
              AND NOT EXISTS (
                  SELECT 1 FROM config."CatalogItems" AS duplicate
                  WHERE duplicate.catalog_group_id = item.catalog_group_id
                    AND duplicate.name = 'Alemania'
                    AND duplicate.id <> item.id
              );

            -- Repair missing seed data even on installations where the original
            -- generic Germany migration was previously marked as applied.
            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value,
                 metadata_json, sort_order, is_system, is_active,
                 created_at_utc, created_by, is_deleted)
            SELECT
                'c2140000-0000-4000-8000-000000000001'::uuid,
                grp.id,
                'WHS_GERMANY_GENERIC',
                'whs-germany-generic-de',
                'Alemania',
                NULL,
                'Alemania',
                '{"isGeneric":true,"countryCode":"DE"}'::jsonb,
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
                  SELECT 1 FROM config."CatalogItems" AS item
                  WHERE item.catalog_group_id = grp.id
                    AND (
                        item.code = 'WHS_GERMANY_GENERIC'
                        OR item.slug = 'whs-germany-generic-de'
                        OR LOWER(item.name) = 'alemania'
                    )
              )
            ON CONFLICT DO NOTHING;
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Leave an operational WHS referenced by existing FCA rates intact.
    }
}
