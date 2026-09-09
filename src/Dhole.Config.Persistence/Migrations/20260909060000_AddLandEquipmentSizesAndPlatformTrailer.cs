using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260909060000_AddLandEquipmentSizesAndPlatformTrailer")]
public sealed class AddLandEquipmentSizesAndPlatformTrailer : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            -- Tamaños terrestres solicitados: 24 pies / 5 TON y 26 pies / 7 TON.
            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT v.id, g.id, v.code, v.slug, v.name, v.description, v.value, v.metadata_json::jsonb, v.sort_order, TRUE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" g
            CROSS JOIN (VALUES
                ('c2150000-0000-4000-8000-000000000001'::uuid, '24', '24', '24 pies / 5 TON', 'Equipo terrestre de 24 pies con capacidad nominal de 5 toneladas.', '24', '{"feet":24,"capacityTons":5,"modality":"Land"}', 24),
                ('c2150000-0000-4000-8000-000000000002'::uuid, '26', '26', '26 pies / 7 TON', 'Equipo terrestre de 26 pies con capacidad nominal de 7 toneladas.', '26', '{"feet":26,"capacityTons":7,"modality":"Land"}', 26)
            ) AS v(id, code, slug, name, description, value, metadata_json, sort_order)
            WHERE g.slug = 'land-equipment-sizes'
              AND NOT EXISTS (
                  SELECT 1
                  FROM config."CatalogItems" i
                  WHERE i.catalog_group_id = g.id
                    AND (i.code = v.code OR i.slug = v.slug)
              )
            ON CONFLICT DO NOTHING;

            UPDATE config."CatalogItems" i
            SET name = CASE i.code
                    WHEN '24' THEN '24 pies / 5 TON'
                    WHEN '26' THEN '26 pies / 7 TON'
                END,
                description = CASE i.code
                    WHEN '24' THEN 'Equipo terrestre de 24 pies con capacidad nominal de 5 toneladas.'
                    WHEN '26' THEN 'Equipo terrestre de 26 pies con capacidad nominal de 7 toneladas.'
                END,
                value = i.code,
                metadata_json = CASE i.code
                    WHEN '24' THEN '{"feet":24,"capacityTons":5,"modality":"Land"}'::jsonb
                    WHEN '26' THEN '{"feet":26,"capacityTons":7,"modality":"Land"}'::jsonb
                END,
                sort_order = i.code::integer,
                is_system = TRUE,
                is_active = TRUE,
                is_deleted = FALSE,
                deleted_at_utc = NULL,
                deleted_by = NULL,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            FROM config."CatalogGroups" g
            WHERE i.catalog_group_id = g.id
              AND g.slug = 'land-equipment-sizes'
              AND i.code IN ('24', '26');

            -- Plataforma se presenta comercialmente como Plataforma / Carreta.
            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT
                'c2160000-0000-4000-8000-000000000001'::uuid,
                g.id,
                'PLATFORM',
                'platform',
                'Plataforma / Carreta',
                'Plataforma o carreta abierta para carga general o sobredimensionada.',
                'Platform',
                '{"modality":"Land","aliases":["PLATFORM","PLATAFORMA","CARRETA","PLATAFORMA / CARRETA","FLATBED"]}'::jsonb,
                20,
                TRUE,
                TRUE,
                NOW(),
                'migration',
                FALSE
            FROM config."CatalogGroups" g
            WHERE g.slug = 'land-equipment-kinds'
              AND NOT EXISTS (
                  SELECT 1
                  FROM config."CatalogItems" i
                  WHERE i.catalog_group_id = g.id
                    AND (i.code = 'PLATFORM' OR i.slug = 'platform')
              )
            ON CONFLICT DO NOTHING;

            UPDATE config."CatalogItems" i
            SET code = 'PLATFORM',
                slug = 'platform',
                name = 'Plataforma / Carreta',
                description = 'Plataforma o carreta abierta para carga general o sobredimensionada.',
                value = 'Platform',
                metadata_json = '{"modality":"Land","aliases":["PLATFORM","PLATAFORMA","CARRETA","PLATAFORMA / CARRETA","FLATBED"]}'::jsonb,
                sort_order = 20,
                is_system = TRUE,
                is_active = TRUE,
                is_deleted = FALSE,
                deleted_at_utc = NULL,
                deleted_by = NULL,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            FROM config."CatalogGroups" g
            WHERE i.catalog_group_id = g.id
              AND g.slug = 'land-equipment-kinds'
              AND (i.code = 'PLATFORM' OR i.slug = 'platform');

            -- Combinaciones canónicas requeridas por el selector terrestre.
            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT v.id, g.id, v.code, v.slug, v.name, v.description, v.value, v.metadata_json::jsonb, v.sort_order, TRUE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" g
            CROSS JOIN (VALUES
                ('c2140000-0000-4000-8000-000000000001'::uuid, '24_DRY_VAN', '24-dry-van', '24 pies / 5 TON · Furgón seco', 'Furgón seco de 24 pies con capacidad nominal de 5 toneladas.', '24_DRY_VAN', '{"modality":"Land","size":"24","capacityTons":5,"kind":"dry-van","kindCode":"DRY_VAN"}', 1),
                ('c2140000-0000-4000-8000-000000000002'::uuid, '24_PLATFORM', '24-platform', '24 pies / 5 TON · Plataforma / Carreta', 'Plataforma / Carreta de 24 pies con capacidad nominal de 5 toneladas.', '24_PLATFORM', '{"modality":"Land","size":"24","capacityTons":5,"kind":"platform","kindCode":"PLATFORM"}', 2),
                ('c2140000-0000-4000-8000-000000000003'::uuid, '24_REEFER', '24-reefer', '24 pies / 5 TON · Refrigerado', 'Furgón refrigerado de 24 pies con capacidad nominal de 5 toneladas.', '24_REEFER', '{"modality":"Land","size":"24","capacityTons":5,"kind":"reefer","kindCode":"REEFER"}', 3),
                ('c2140000-0000-4000-8000-000000000004'::uuid, '24_CURTAIN_SIDE', '24-curtain-side', '24 pies / 5 TON · Furgón con cortina', 'Furgón con cortina de 24 pies con capacidad nominal de 5 toneladas.', '24_CURTAIN_SIDE', '{"modality":"Land","size":"24","capacityTons":5,"kind":"curtain-side","kindCode":"CURTAIN_SIDE"}', 4),
                ('c2140000-0000-4000-8000-000000000005'::uuid, '26_DRY_VAN', '26-dry-van', '26 pies / 7 TON · Furgón seco', 'Furgón seco de 26 pies con capacidad nominal de 7 toneladas.', '26_DRY_VAN', '{"modality":"Land","size":"26","capacityTons":7,"kind":"dry-van","kindCode":"DRY_VAN"}', 5),
                ('c2140000-0000-4000-8000-000000000006'::uuid, '26_PLATFORM', '26-platform', '26 pies / 7 TON · Plataforma / Carreta', 'Plataforma / Carreta de 26 pies con capacidad nominal de 7 toneladas.', '26_PLATFORM', '{"modality":"Land","size":"26","capacityTons":7,"kind":"platform","kindCode":"PLATFORM"}', 6),
                ('c2140000-0000-4000-8000-000000000007'::uuid, '26_REEFER', '26-reefer', '26 pies / 7 TON · Refrigerado', 'Furgón refrigerado de 26 pies con capacidad nominal de 7 toneladas.', '26_REEFER', '{"modality":"Land","size":"26","capacityTons":7,"kind":"reefer","kindCode":"REEFER"}', 7),
                ('c2140000-0000-4000-8000-000000000008'::uuid, '26_CURTAIN_SIDE', '26-curtain-side', '26 pies / 7 TON · Furgón con cortina', 'Furgón con cortina de 26 pies con capacidad nominal de 7 toneladas.', '26_CURTAIN_SIDE', '{"modality":"Land","size":"26","capacityTons":7,"kind":"curtain-side","kindCode":"CURTAIN_SIDE"}', 8)
            ) AS v(id, code, slug, name, description, value, metadata_json, sort_order)
            WHERE g.slug = 'land-equipment-types'
              AND NOT EXISTS (
                  SELECT 1
                  FROM config."CatalogItems" i
                  WHERE i.catalog_group_id = g.id
                    AND (i.code = v.code OR i.slug = v.slug)
              )
            ON CONFLICT DO NOTHING;

            UPDATE config."CatalogItems" i
            SET is_system = TRUE,
                is_active = TRUE,
                is_deleted = FALSE,
                deleted_at_utc = NULL,
                deleted_by = NULL,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            FROM config."CatalogGroups" g
            WHERE i.catalog_group_id = g.id
              AND g.slug = 'land-equipment-types'
              AND i.code IN (
                  '24_DRY_VAN', '24_PLATFORM', '24_REEFER', '24_CURTAIN_SIDE',
                  '26_DRY_VAN', '26_PLATFORM', '26_REEFER', '26_CURTAIN_SIDE'
              );

            UPDATE config."CatalogItems" i
            SET name = CASE i.code
                    WHEN '48_PLATFORM' THEN '48 pies · Plataforma / Carreta'
                    WHEN '53_PLATFORM' THEN '53 pies · Plataforma / Carreta'
                END,
                description = CASE i.code
                    WHEN '48_PLATFORM' THEN 'Plataforma / Carreta de 48 pies.'
                    WHEN '53_PLATFORM' THEN 'Plataforma / Carreta de 53 pies.'
                END,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            FROM config."CatalogGroups" g
            WHERE i.catalog_group_id = g.id
              AND g.slug = 'land-equipment-types'
              AND i.code IN ('48_PLATFORM', '53_PLATFORM');
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE config."CatalogItems"
            SET is_active = FALSE,
                is_deleted = TRUE,
                deleted_at_utc = NOW(),
                deleted_by = 'migration',
                updated_at_utc = NOW(),
                updated_by = 'migration'
            WHERE id IN (
                'c2140000-0000-4000-8000-000000000001'::uuid,
                'c2140000-0000-4000-8000-000000000002'::uuid,
                'c2140000-0000-4000-8000-000000000003'::uuid,
                'c2140000-0000-4000-8000-000000000004'::uuid,
                'c2140000-0000-4000-8000-000000000005'::uuid,
                'c2140000-0000-4000-8000-000000000006'::uuid,
                'c2140000-0000-4000-8000-000000000007'::uuid,
                'c2140000-0000-4000-8000-000000000008'::uuid,
                'c2150000-0000-4000-8000-000000000001'::uuid,
                'c2150000-0000-4000-8000-000000000002'::uuid,
                'c2160000-0000-4000-8000-000000000001'::uuid
            );

            UPDATE config."CatalogItems" i
            SET name = 'Plataforma',
                description = 'Plataforma abierta para carga sobredimensionada o carga general.',
                metadata_json = '{"modality":"Land","aliases":["PLATFORM","PLATAFORMA","FLATBED"]}'::jsonb,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            FROM config."CatalogGroups" g
            WHERE i.catalog_group_id = g.id
              AND g.slug = 'land-equipment-kinds'
              AND i.code = 'PLATFORM'
              AND i.id <> 'c2160000-0000-4000-8000-000000000001'::uuid;

            UPDATE config."CatalogItems" i
            SET name = CASE i.code
                    WHEN '48_PLATFORM' THEN '48 pies · Plataforma'
                    WHEN '53_PLATFORM' THEN '53 pies · Plataforma'
                END,
                description = CASE i.code
                    WHEN '48_PLATFORM' THEN 'Plataforma de 48 pies.'
                    WHEN '53_PLATFORM' THEN 'Plataforma de 53 pies.'
                END,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            FROM config."CatalogGroups" g
            WHERE i.catalog_group_id = g.id
              AND g.slug = 'land-equipment-types'
              AND i.code IN ('48_PLATFORM', '53_PLATFORM');
            """
        );
    }
}
