using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260825151500_AddAirEquipmentTypes")]
public sealed class AddAirEquipmentTypes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            INSERT INTO config."CatalogGroups"
                (id, code, slug, name, description, metadata_json, is_system, is_active, created_at_utc, created_by, is_deleted)
            VALUES
                ('c2000000-0000-4000-8000-000000000006', 'AIR_EQUIPMENT_TYPES', 'air-equipment-types', 'Equipos aéreos', 'Tipos de equipo utilizados en alternativas aéreas.', '{"pricingWorkflow":true,"modality":"Air"}'::jsonb, TRUE, TRUE, NOW(), 'migration', FALSE)
            ON CONFLICT DO NOTHING;

            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT v.id, g.id, v.code, v.slug, v.name, v.description, v.value, v.metadata_json::jsonb, v.sort_order, TRUE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" g
            CROSS JOIN (VALUES
                ('c2600000-0000-4000-8000-000000000001'::uuid, 'LOOSE', 'loose-cargo', 'Carga suelta', 'Carga aérea suelta.', 'LOOSE', '{"modalities":["Air"],"shipmentModes":["LCL"]}', 10),
                ('c2600000-0000-4000-8000-000000000002'::uuid, 'PALLET', 'air-pallet', 'Pallet', 'Carga aérea paletizada.', 'PALLET', '{"modalities":["Air"],"shipmentModes":["LCL"]}', 20),
                ('c2600000-0000-4000-8000-000000000003'::uuid, 'ULD', 'uld', 'ULD', 'Unit Load Device para carga aérea.', 'ULD', '{"modalities":["Air"],"shipmentModes":["LCL"]}', 30)
            ) AS v(id, code, slug, name, description, value, metadata_json, sort_order)
            WHERE g.slug = 'air-equipment-types'
            ON CONFLICT (catalog_group_id, code) DO UPDATE
              SET name = EXCLUDED.name, description = EXCLUDED.description, value = EXCLUDED.value,
                  metadata_json = EXCLUDED.metadata_json, is_active = TRUE, updated_at_utc = NOW(), updated_by = 'migration';
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DELETE FROM config."CatalogItems"
            WHERE id::text LIKE 'c26%';

            DELETE FROM config."CatalogGroups"
            WHERE slug = 'air-equipment-types' AND created_by = 'migration';
            """
        );
    }
}
