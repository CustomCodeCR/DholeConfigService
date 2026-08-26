using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260826165000_RemoveLandEquipmentCatalog")]
public sealed class RemoveLandEquipmentCatalog : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            -- FTL y LTL son tipos de embarque terrestre; no requieren un catálogo de equipo adicional.
            UPDATE config."CatalogItems" i
            SET is_active = FALSE,
                is_deleted = TRUE,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            FROM config."CatalogGroups" g
            WHERE i.catalog_group_id = g.id
              AND g.slug = 'land-equipment-types'
              AND i.is_deleted = FALSE;

            UPDATE config."CatalogGroups"
            SET is_active = FALSE,
                is_deleted = TRUE,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            WHERE slug = 'land-equipment-types'
              AND is_deleted = FALSE;

            -- Se conserva la relación correcta: Terrestre -> FTL/LTL.
            UPDATE config."CatalogItems" i
            SET metadata_json = COALESCE(i.metadata_json, '{}'::jsonb)
                    || '{"shipmentModes":["FTL","LTL"]}'::jsonb,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            FROM config."CatalogGroups" g
            WHERE i.catalog_group_id = g.id
              AND g.slug = 'transport-modalities'
              AND i.is_deleted = FALSE
              AND UPPER(i.code) = 'LAND';

            WITH desired(code, metadata_json) AS (
                VALUES
                    ('FTL', '{"modalities":["Land"]}'::jsonb),
                    ('LTL', '{"modalities":["Land"]}'::jsonb)
            )
            UPDATE config."CatalogItems" i
            SET metadata_json = COALESCE(i.metadata_json, '{}'::jsonb) || d.metadata_json,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            FROM config."CatalogGroups" g, desired d
            WHERE i.catalog_group_id = g.id
              AND g.slug = 'shipment-modes'
              AND i.is_deleted = FALSE
              AND UPPER(i.code) = d.code;
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
                updated_at_utc = NOW(),
                updated_by = 'migration-down'
            WHERE slug = 'land-equipment-types';

            UPDATE config."CatalogItems" i
            SET is_active = TRUE,
                is_deleted = FALSE,
                updated_at_utc = NOW(),
                updated_by = 'migration-down'
            FROM config."CatalogGroups" g
            WHERE i.catalog_group_id = g.id
              AND g.slug = 'land-equipment-types';
            """
        );
    }
}
