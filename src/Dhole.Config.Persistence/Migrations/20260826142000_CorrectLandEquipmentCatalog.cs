using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260826142000_CorrectLandEquipmentCatalog")]
public sealed class CorrectLandEquipmentCatalog : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            -- Terrestre usa FTL/LTL. FCL/LCL quedan reservados a contenedor/carga consolidada.
            UPDATE config."CatalogItems" i
            SET metadata_json = COALESCE(i.metadata_json, '{}'::jsonb) || '{"shipmentModes":["FTL","LTL"]}'::jsonb,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            FROM config."CatalogGroups" g
            WHERE i.catalog_group_id = g.id
              AND g.slug = 'transport-modalities'
              AND i.is_deleted = FALSE
              AND UPPER(i.code) = 'LAND';

            WITH desired(code, metadata_json) AS (
                VALUES
                    ('FCL', '{"modalities":["Maritime","Multimodal"]}'::jsonb),
                    ('LCL', '{"modalities":["Maritime","Air","Multimodal"]}'::jsonb),
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

            -- Los tipos marítimos dejan de declararse como equipo terrestre.
            WITH desired(code, metadata_json) AS (
                VALUES
                    ('20DV', '{"size":"20","kind":"dry-van","modalities":["Maritime","Multimodal"],"shipmentModes":["FCL"]}'::jsonb),
                    ('40DV', '{"size":"40","kind":"dry-van","modalities":["Maritime","Multimodal"],"shipmentModes":["FCL"]}'::jsonb),
                    ('40HC', '{"size":"40","kind":"high-cube","modalities":["Maritime","Multimodal"],"shipmentModes":["FCL"]}'::jsonb),
                    ('45HC', '{"size":"45","kind":"high-cube","modalities":["Maritime","Multimodal"],"shipmentModes":["FCL"]}'::jsonb),
                    ('48HC', '{"size":"48","kind":"high-cube","modalities":["Multimodal"],"shipmentModes":["FCL"]}'::jsonb)
            )
            UPDATE config."CatalogItems" i
            SET metadata_json = d.metadata_json,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            FROM config."CatalogGroups" g, desired d
            WHERE i.catalog_group_id = g.id
              AND g.slug = 'container-types'
              AND i.is_deleted = FALSE
              AND UPPER(i.code) = d.code;

            -- Tipos de unidad terrestre. El Value es el texto comercial que debe mostrar Web.
            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT v.id, g.id, v.code, v.slug, v.name, v.description, v.value, v.metadata_json::jsonb, v.sort_order,
                   TRUE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" g
            CROSS JOIN (VALUES
                ('c2800000-0000-4000-8000-000000000001'::uuid, 'LAND_DRY_VAN', 'land-dry-van', 'Furgón seco', 'Furgón cerrado para carga seca.', 'Furgón seco', '{"modalities":["Land"],"shipmentModes":["FTL","LTL"],"kind":"dry-van"}', 10),
                ('c2800000-0000-4000-8000-000000000002'::uuid, 'LAND_REEFER', 'land-reefer', 'Furgón refrigerado', 'Furgón con control de temperatura.', 'Furgón refrigerado', '{"modalities":["Land"],"shipmentModes":["FTL","LTL"],"kind":"reefer"}', 20),
                ('c2800000-0000-4000-8000-000000000003'::uuid, 'LAND_CURTAIN', 'land-curtain', 'Furgón cortina', 'Furgón con lona o cortina lateral.', 'Furgón cortina', '{"modalities":["Land"],"shipmentModes":["FTL","LTL"],"kind":"curtain"}', 30),
                ('c2800000-0000-4000-8000-000000000004'::uuid, 'LAND_FLATBED', 'land-flatbed', 'Plataforma', 'Plataforma abierta para carga general o sobredimensionada.', 'Plataforma', '{"modalities":["Land"],"shipmentModes":["FTL","LTL"],"kind":"flatbed"}', 40),
                ('c2800000-0000-4000-8000-000000000005'::uuid, 'LAND_LOWBOY', 'land-lowboy', 'Cama baja', 'Unidad de cama baja para maquinaria y carga pesada.', 'Cama baja', '{"modalities":["Land"],"shipmentModes":["FTL"],"kind":"lowboy"}', 50),
                ('c2800000-0000-4000-8000-000000000006'::uuid, 'LAND_TANKER', 'land-tanker', 'Cisterna', 'Unidad cisterna para líquidos o granel.', 'Cisterna', '{"modalities":["Land"],"shipmentModes":["FTL"],"kind":"tanker"}', 60),
                ('c2800000-0000-4000-8000-000000000007'::uuid, 'LAND_CONTAINER_CARRIER', 'land-container-carrier', 'Portacontenedor', 'Chasis o unidad dedicada al traslado terrestre de contenedores.', 'Portacontenedor', '{"modalities":["Land"],"shipmentModes":["FTL"],"kind":"container-carrier"}', 70),
                ('c2800000-0000-4000-8000-000000000008'::uuid, 'LAND_CHASSIS', 'land-chassis', 'Chasis', 'Chasis terrestre para operaciones de carga especializada.', 'Chasis', '{"modalities":["Land"],"shipmentModes":["FTL"],"kind":"chassis"}', 80),
                ('c2800000-0000-4000-8000-000000000009'::uuid, 'LAND_DUMP', 'land-dump', 'Volteo', 'Unidad de volteo para carga a granel.', 'Volteo', '{"modalities":["Land"],"shipmentModes":["FTL"],"kind":"dump"}', 90)
            ) AS v(id, code, slug, name, description, value, metadata_json, sort_order)
            WHERE g.slug = 'land-equipment-types'
              AND g.is_deleted = FALSE
            ON CONFLICT (catalog_group_id, code) DO UPDATE
              SET slug = EXCLUDED.slug,
                  name = EXCLUDED.name,
                  description = EXCLUDED.description,
                  value = EXCLUDED.value,
                  metadata_json = EXCLUDED.metadata_json,
                  sort_order = EXCLUDED.sort_order,
                  is_active = TRUE,
                  is_deleted = FALSE,
                  updated_at_utc = NOW(),
                  updated_by = 'migration';
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DELETE FROM config."CatalogItems"
            WHERE id::text LIKE 'c2800000-%';

            UPDATE config."CatalogItems" i
            SET metadata_json = COALESCE(i.metadata_json, '{}'::jsonb) || '{"shipmentModes":["FTL","FCL"]}'::jsonb,
                updated_at_utc = NOW(),
                updated_by = 'migration-down'
            FROM config."CatalogGroups" g
            WHERE i.catalog_group_id = g.id
              AND g.slug = 'transport-modalities'
              AND UPPER(i.code) = 'LAND';
            """
        );
    }
}
