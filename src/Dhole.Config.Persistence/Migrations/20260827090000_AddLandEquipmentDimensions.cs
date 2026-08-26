using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260827090000_AddLandEquipmentDimensions")]
public sealed class AddLandEquipmentDimensions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE config."CatalogGroups"
            SET code = 'LAND_EQUIPMENT_TYPES',
                name = 'Equipos terrestres',
                description = 'Combinaciones canónicas de tamaño y tipo de equipo terrestre utilizadas por Pricing.',
                metadata_json = '{"modality":"Land","dimension":"equipment"}'::jsonb,
                is_system = TRUE,
                is_active = TRUE,
                is_deleted = FALSE,
                deleted_at_utc = NULL,
                deleted_by = NULL,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            WHERE slug = 'land-equipment-types';

            INSERT INTO config."CatalogGroups"
                (id, code, slug, name, description, metadata_json, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT
                'c2000000-0000-4000-8000-000000000004'::uuid,
                'LAND_EQUIPMENT_TYPES',
                'land-equipment-types',
                'Equipos terrestres',
                'Combinaciones canónicas de tamaño y tipo de equipo terrestre utilizadas por Pricing.',
                '{"modality":"Land","dimension":"equipment"}'::jsonb,
                TRUE,
                TRUE,
                NOW(),
                'migration',
                FALSE
            WHERE NOT EXISTS (
                SELECT 1 FROM config."CatalogGroups" WHERE slug = 'land-equipment-types'
            )
            ON CONFLICT DO NOTHING;

            INSERT INTO config."CatalogGroups"
                (id, code, slug, name, description, metadata_json, is_system, is_active, created_at_utc, created_by, is_deleted)
            VALUES
                ('c2100000-0000-4000-8000-000000000001', 'LAND_EQUIPMENT_SIZES', 'land-equipment-sizes', 'Tamaños de equipo terrestre', 'Largos nominales de furgones y equipos terrestres.', '{"modality":"Land","dimension":"size"}'::jsonb, TRUE, TRUE, NOW(), 'migration', FALSE),
                ('c2100000-0000-4000-8000-000000000002', 'LAND_EQUIPMENT_KINDS', 'land-equipment-kinds', 'Tipos de furgón y equipo', 'Tipo físico del equipo terrestre independiente de su tamaño.', '{"modality":"Land","dimension":"kind"}'::jsonb, TRUE, TRUE, NOW(), 'migration', FALSE)
            ON CONFLICT DO NOTHING;

            UPDATE config."CatalogGroups"
            SET is_active = TRUE,
                is_deleted = FALSE,
                deleted_at_utc = NULL,
                deleted_by = NULL,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            WHERE slug IN ('land-equipment-sizes', 'land-equipment-kinds');

            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT v.id, g.id, v.code, v.slug, v.name, v.description, v.value, v.metadata_json::jsonb, v.sort_order, TRUE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" g
            CROSS JOIN (VALUES
                ('c2110000-0000-4000-8000-000000000048'::uuid, '48', '48', '48 pies', 'Equipo terrestre de 48 pies.', '48', '{"feet":48,"modality":"Land"}', 48),
                ('c2110000-0000-4000-8000-000000000053'::uuid, '53', '53', '53 pies', 'Equipo terrestre de 53 pies.', '53', '{"feet":53,"modality":"Land"}', 53)
            ) AS v(id, code, slug, name, description, value, metadata_json, sort_order)
            WHERE g.slug = 'land-equipment-sizes'
            ON CONFLICT DO NOTHING;

            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT v.id, g.id, v.code, v.slug, v.name, v.description, v.value, v.metadata_json::jsonb, v.sort_order, TRUE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" g
            CROSS JOIN (VALUES
                ('c2120000-0000-4000-8000-000000000001'::uuid, 'DRY_VAN', 'dry-van', 'Furgón seco', 'Furgón seco cerrado para carga general.', 'DryVan', '{"modality":"Land","aliases":["DRY VAN","FURGON SECO","FURGÓN SECO","CAJA SECA"]}', 10),
                ('c2120000-0000-4000-8000-000000000002'::uuid, 'PLATFORM', 'platform', 'Plataforma', 'Plataforma abierta para carga sobredimensionada o carga general.', 'Platform', '{"modality":"Land","aliases":["PLATFORM","PLATAFORMA","FLATBED"]}', 20),
                ('c2120000-0000-4000-8000-000000000003'::uuid, 'REEFER', 'reefer', 'Refrigerado', 'Furgón refrigerado para carga con control de temperatura.', 'Reefer', '{"modality":"Land","aliases":["REEFER","REFRIGERADO","FURGON REFRIGERADO","FURGÓN REFRIGERADO"]}', 30),
                ('c2120000-0000-4000-8000-000000000004'::uuid, 'CURTAIN_SIDE', 'curtain-side', 'Furgón con cortina', 'Equipo con laterales de cortina para facilitar carga lateral.', 'CurtainSide', '{"modality":"Land","aliases":["CURTAIN SIDE","CURTAINSIDE","FURGON CON CORTINA","FURGÓN CON CORTINA"]}', 40)
            ) AS v(id, code, slug, name, description, value, metadata_json, sort_order)
            WHERE g.slug = 'land-equipment-kinds'
            ON CONFLICT DO NOTHING;

            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT v.id, g.id, v.code, v.slug, v.name, v.description, v.value, v.metadata_json::jsonb, v.sort_order, TRUE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" g
            CROSS JOIN (VALUES
                ('c2130000-0000-4000-8000-000000000001'::uuid, '48_DRY_VAN', '48-dry-van', '48 pies · Furgón seco', 'Furgón seco de 48 pies.', '48_DRY_VAN', '{"modality":"Land","size":"48","kind":"dry-van","kindCode":"DRY_VAN"}', 10),
                ('c2130000-0000-4000-8000-000000000002'::uuid, '48_PLATFORM', '48-platform', '48 pies · Plataforma', 'Plataforma de 48 pies.', '48_PLATFORM', '{"modality":"Land","size":"48","kind":"platform","kindCode":"PLATFORM"}', 20),
                ('c2130000-0000-4000-8000-000000000003'::uuid, '48_REEFER', '48-reefer', '48 pies · Refrigerado', 'Furgón refrigerado de 48 pies.', '48_REEFER', '{"modality":"Land","size":"48","kind":"reefer","kindCode":"REEFER"}', 30),
                ('c2130000-0000-4000-8000-000000000004'::uuid, '48_CURTAIN_SIDE', '48-curtain-side', '48 pies · Furgón con cortina', 'Furgón con cortina de 48 pies.', '48_CURTAIN_SIDE', '{"modality":"Land","size":"48","kind":"curtain-side","kindCode":"CURTAIN_SIDE"}', 40),
                ('c2130000-0000-4000-8000-000000000005'::uuid, '53_DRY_VAN', '53-dry-van', '53 pies · Furgón seco', 'Furgón seco de 53 pies.', '53_DRY_VAN', '{"modality":"Land","size":"53","kind":"dry-van","kindCode":"DRY_VAN"}', 50),
                ('c2130000-0000-4000-8000-000000000006'::uuid, '53_PLATFORM', '53-platform', '53 pies · Plataforma', 'Plataforma de 53 pies.', '53_PLATFORM', '{"modality":"Land","size":"53","kind":"platform","kindCode":"PLATFORM"}', 60),
                ('c2130000-0000-4000-8000-000000000007'::uuid, '53_REEFER', '53-reefer', '53 pies · Refrigerado', 'Furgón refrigerado de 53 pies.', '53_REEFER', '{"modality":"Land","size":"53","kind":"reefer","kindCode":"REEFER"}', 70),
                ('c2130000-0000-4000-8000-000000000008'::uuid, '53_CURTAIN_SIDE', '53-curtain-side', '53 pies · Furgón con cortina', 'Furgón con cortina de 53 pies.', '53_CURTAIN_SIDE', '{"modality":"Land","size":"53","kind":"curtain-side","kindCode":"CURTAIN_SIDE"}', 80)
            ) AS v(id, code, slug, name, description, value, metadata_json, sort_order)
            WHERE g.slug = 'land-equipment-types'
            ON CONFLICT DO NOTHING;

            UPDATE config."CatalogItems"
            SET is_active = FALSE,
                is_deleted = TRUE,
                deleted_at_utc = COALESCE(deleted_at_utc, NOW()),
                deleted_by = COALESCE(deleted_by, 'migration'),
                updated_at_utc = NOW(),
                updated_by = 'migration'
            WHERE catalog_group_id IN (
                SELECT id FROM config."CatalogGroups" WHERE slug = 'land-equipment-types'
            )
              AND code IN ('DRY_VAN', 'PLATFORM', 'REEFER', 'CURTAIN_SIDE');
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
            WHERE id::text LIKE 'c213%'
               OR id::text LIKE 'c212%'
               OR id::text LIKE 'c211%';

            UPDATE config."CatalogGroups"
            SET is_active = FALSE,
                is_deleted = TRUE,
                deleted_at_utc = NOW(),
                deleted_by = 'migration',
                updated_at_utc = NOW(),
                updated_by = 'migration'
            WHERE slug IN ('land-equipment-types', 'land-equipment-sizes', 'land-equipment-kinds');
            """
        );
    }
}
