using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260908223000_ExtendLandEquipmentCatalog")]
public sealed class ExtendLandEquipmentCatalog : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE config."CatalogItems" i
            SET name = 'Plataforma / Carreta',
                description = 'Plataforma o carreta abierta para carga general y sobredimensionada.',
                value = 'Platform',
                metadata_json = COALESCE(i.metadata_json, '{}'::jsonb)
                    || '{"modality":"Land","aliases":["PLATFORM","PLATAFORMA","CARRETA","PLATAFORMA / CARRETA","FLATBED"]}'::jsonb,
                is_active = TRUE,
                is_deleted = FALSE,
                deleted_at_utc = NULL,
                deleted_by = NULL,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            FROM config."CatalogGroups" g
            WHERE i.catalog_group_id = g.id
              AND g.slug = 'land-equipment-kinds'
              AND i.code = 'PLATFORM';

            WITH desired(id, code, slug, name, description, value, metadata_json, sort_order) AS (
                VALUES
                    ('c2110000-0000-4000-8000-000000000024'::uuid, '24', '24', '24 pies / 5 TON', 'Equipo terrestre de 24 pies con capacidad nominal de 5 toneladas.', '24', '{"feet":24,"tonCapacity":5,"modality":"Land"}', 24),
                    ('c2110000-0000-4000-8000-000000000026'::uuid, '26', '26', '26 pies / 7 TON', 'Equipo terrestre de 26 pies con capacidad nominal de 7 toneladas.', '26', '{"feet":26,"tonCapacity":7,"modality":"Land"}', 26)
            )
            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT d.id, g.id, d.code, d.slug, d.name, d.description, d.value, d.metadata_json::jsonb, d.sort_order,
                   TRUE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" g CROSS JOIN desired d
            WHERE g.slug = 'land-equipment-sizes'
              AND g.is_deleted = FALSE
              AND NOT EXISTS (
                  SELECT 1
                  FROM config."CatalogItems" i
                  WHERE i.catalog_group_id = g.id
                    AND i.is_deleted = FALSE
                    AND (UPPER(i.code) = UPPER(d.code) OR LOWER(i.slug) = LOWER(d.slug))
              )
            ON CONFLICT DO NOTHING;

            WITH desired(id, code, slug, name, description, value, metadata_json, sort_order) AS (
                VALUES
                    ('c2140000-0000-4000-8000-000000000001'::uuid, '24_DRY_VAN', '24-dry-van', '24 pies / 5 TON · Furgón seco', 'Furgón seco de 24 pies con capacidad nominal de 5 toneladas.', '24_DRY_VAN', '{"modality":"Land","size":"24","kind":"dry-van","kindCode":"DRY_VAN","tonCapacity":5}', 210),
                    ('c2140000-0000-4000-8000-000000000002'::uuid, '24_PLATFORM', '24-platform', '24 pies / 5 TON · Plataforma / Carreta', 'Plataforma / Carreta de 24 pies con capacidad nominal de 5 toneladas.', '24_PLATFORM', '{"modality":"Land","size":"24","kind":"platform","kindCode":"PLATFORM","tonCapacity":5}', 220),
                    ('c2140000-0000-4000-8000-000000000003'::uuid, '24_REEFER', '24-reefer', '24 pies / 5 TON · Refrigerado', 'Furgón refrigerado de 24 pies con capacidad nominal de 5 toneladas.', '24_REEFER', '{"modality":"Land","size":"24","kind":"reefer","kindCode":"REEFER","tonCapacity":5}', 230),
                    ('c2140000-0000-4000-8000-000000000004'::uuid, '24_CURTAIN_SIDE', '24-curtain-side', '24 pies / 5 TON · Furgón con cortina', 'Furgón con cortina de 24 pies con capacidad nominal de 5 toneladas.', '24_CURTAIN_SIDE', '{"modality":"Land","size":"24","kind":"curtain-side","kindCode":"CURTAIN_SIDE","tonCapacity":5}', 240),
                    ('c2140000-0000-4000-8000-000000000005'::uuid, '26_DRY_VAN', '26-dry-van', '26 pies / 7 TON · Furgón seco', 'Furgón seco de 26 pies con capacidad nominal de 7 toneladas.', '26_DRY_VAN', '{"modality":"Land","size":"26","kind":"dry-van","kindCode":"DRY_VAN","tonCapacity":7}', 250),
                    ('c2140000-0000-4000-8000-000000000006'::uuid, '26_PLATFORM', '26-platform', '26 pies / 7 TON · Plataforma / Carreta', 'Plataforma / Carreta de 26 pies con capacidad nominal de 7 toneladas.', '26_PLATFORM', '{"modality":"Land","size":"26","kind":"platform","kindCode":"PLATFORM","tonCapacity":7}', 260),
                    ('c2140000-0000-4000-8000-000000000007'::uuid, '26_REEFER', '26-reefer', '26 pies / 7 TON · Refrigerado', 'Furgón refrigerado de 26 pies con capacidad nominal de 7 toneladas.', '26_REEFER', '{"modality":"Land","size":"26","kind":"reefer","kindCode":"REEFER","tonCapacity":7}', 270),
                    ('c2140000-0000-4000-8000-000000000008'::uuid, '26_CURTAIN_SIDE', '26-curtain-side', '26 pies / 7 TON · Furgón con cortina', 'Furgón con cortina de 26 pies con capacidad nominal de 7 toneladas.', '26_CURTAIN_SIDE', '{"modality":"Land","size":"26","kind":"curtain-side","kindCode":"CURTAIN_SIDE","tonCapacity":7}', 280)
            )
            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT d.id, g.id, d.code, d.slug, d.name, d.description, d.value, d.metadata_json::jsonb, d.sort_order,
                   TRUE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" g CROSS JOIN desired d
            WHERE g.slug = 'land-equipment-types'
              AND g.is_deleted = FALSE
              AND NOT EXISTS (
                  SELECT 1
                  FROM config."CatalogItems" i
                  WHERE i.catalog_group_id = g.id
                    AND i.is_deleted = FALSE
                    AND (UPPER(i.code) = UPPER(d.code) OR LOWER(i.slug) = LOWER(d.slug))
              )
            ON CONFLICT DO NOTHING;

            UPDATE config."CatalogItems" i
            SET name = replace(i.name, '· Plataforma', '· Plataforma / Carreta'),
                description = replace(i.description, 'Plataforma de', 'Plataforma / Carreta de'),
                updated_at_utc = NOW(),
                updated_by = 'migration'
            FROM config."CatalogGroups" g
            WHERE i.catalog_group_id = g.id
              AND g.slug = 'land-equipment-types'
              AND i.code IN ('48_PLATFORM', '53_PLATFORM')
              AND i.is_deleted = FALSE;
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
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
              AND g.slug IN ('land-equipment-sizes', 'land-equipment-types')
              AND (i.code IN ('24', '26') OR i.code LIKE '24\_%' ESCAPE '\\' OR i.code LIKE '26\_%' ESCAPE '\\');

            UPDATE config."CatalogItems" i
            SET name = CASE
                    WHEN g.slug = 'land-equipment-kinds' AND i.code = 'PLATFORM' THEN 'Plataforma'
                    ELSE replace(i.name, '· Plataforma / Carreta', '· Plataforma')
                END,
                description = CASE
                    WHEN g.slug = 'land-equipment-kinds' AND i.code = 'PLATFORM' THEN 'Plataforma abierta para carga sobredimensionada o carga general.'
                    ELSE replace(i.description, 'Plataforma / Carreta de', 'Plataforma de')
                END,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            FROM config."CatalogGroups" g
            WHERE i.catalog_group_id = g.id
              AND ((g.slug = 'land-equipment-kinds' AND i.code = 'PLATFORM')
                OR (g.slug = 'land-equipment-types' AND i.code IN ('48_PLATFORM', '53_PLATFORM')));
            """
        );
    }
}
