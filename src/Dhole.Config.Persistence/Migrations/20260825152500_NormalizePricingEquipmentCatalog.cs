using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260825152500_NormalizePricingEquipmentCatalog")]
public sealed class NormalizePricingEquipmentCatalog : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT v.id, g.id, v.code, v.slug, v.name, v.description, v.value, v.metadata_json::jsonb, v.sort_order, TRUE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" g
            CROSS JOIN (VALUES
                ('c2700000-0000-4000-8000-000000000001'::uuid, '20DV', '20-dry-van', '20 Dry Van', 'Equipo válido para marítimo, terrestre y multimodal.', '20DV', '{"size":"20","kind":"dry-van","modalities":["Maritime","Land","Multimodal"],"shipmentModes":["FCL","FTL"]}', 10),
                ('c2700000-0000-4000-8000-000000000002'::uuid, '40DV', '40-dry-van', '40 Dry Van', 'Equipo válido para marítimo, terrestre y multimodal.', '40DV', '{"size":"40","kind":"dry-van","modalities":["Maritime","Land","Multimodal"],"shipmentModes":["FCL","FTL"]}', 20),
                ('c2700000-0000-4000-8000-000000000003'::uuid, '40HC', '40-high-cube', '40 High Cube', 'Equipo válido para marítimo, terrestre y multimodal.', '40HC', '{"size":"40","kind":"high-cube","modalities":["Maritime","Land","Multimodal"],"shipmentModes":["FCL","FTL"]}', 30),
                ('c2700000-0000-4000-8000-000000000004'::uuid, '45HC', '45-high-cube', '45 High Cube', 'Equipo válido para marítimo, terrestre y multimodal.', '45HC', '{"size":"45","kind":"high-cube","modalities":["Maritime","Land","Multimodal"],"shipmentModes":["FCL","FTL"]}', 40),
                ('c2700000-0000-4000-8000-000000000005'::uuid, '48HC', '48-high-cube', '48 High Cube', 'Equipo terrestre y multimodal.', '48HC', '{"size":"48","kind":"high-cube","modalities":["Land","Multimodal"],"shipmentModes":["FCL","FTL"]}', 50),
                ('c2700000-0000-4000-8000-000000000006'::uuid, 'LOOSE', 'air-loose-cargo', 'Carga suelta', 'Equipo lógico para carga aérea LCL.', 'LOOSE', '{"modalities":["Air"],"shipmentModes":["LCL"]}', 200),
                ('c2700000-0000-4000-8000-000000000007'::uuid, 'PALLET', 'air-pallet', 'Pallet aéreo', 'Equipo lógico para carga aérea LCL.', 'PALLET', '{"modalities":["Air"],"shipmentModes":["LCL"]}', 210),
                ('c2700000-0000-4000-8000-000000000008'::uuid, 'ULD', 'air-uld', 'ULD', 'Unit Load Device para carga aérea LCL.', 'ULD', '{"modalities":["Air"],"shipmentModes":["LCL"]}', 220)
            ) AS v(id, code, slug, name, description, value, metadata_json, sort_order)
            WHERE g.slug = 'container-types'
            ON CONFLICT (catalog_group_id, code) DO UPDATE
              SET name = EXCLUDED.name,
                  description = EXCLUDED.description,
                  value = EXCLUDED.value,
                  metadata_json = EXCLUDED.metadata_json,
                  is_active = TRUE,
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
            WHERE id IN (
                'c2700000-0000-4000-8000-000000000006',
                'c2700000-0000-4000-8000-000000000007',
                'c2700000-0000-4000-8000-000000000008'
            );
            """
        );
    }
}
