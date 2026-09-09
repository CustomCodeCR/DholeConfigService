using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260909024500_EnsureMultimodalViaPanamaPoe")]
public sealed class EnsureMultimodalViaPanamaPoe : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT
                'c2700000-0000-4000-8000-000000000001'::uuid,
                g.id,
                'MULTIMODAL_VIA_PANAMA',
                'multimodal-via-panama',
                'Multimodal Via Panamá',
                'Opción virtual FCL. En la búsqueda se expande a todos los POE reales de Panamá.',
                'Multimodal Via Panamá',
                '{"pricingWorkflow":true,"multimodalViaPanama":true,"countryCode":"PA","virtualPoe":true}'::jsonb,
                5,
                TRUE,
                TRUE,
                NOW(),
                'migration',
                FALSE
            FROM config."CatalogGroups" g
            WHERE g.slug = 'poe'
              AND g.is_deleted = FALSE
              AND NOT EXISTS (
                  SELECT 1
                  FROM config."CatalogItems" i
                  WHERE i.catalog_group_id = g.id
                    AND i.is_deleted = FALSE
                    AND (i.code = 'MULTIMODAL_VIA_PANAMA' OR i.slug = 'multimodal-via-panama')
              )
            ON CONFLICT DO NOTHING;

            UPDATE config."CatalogItems" i
            SET
                name = 'Multimodal Via Panamá',
                value = 'Multimodal Via Panamá',
                description = 'Opción virtual FCL. En la búsqueda se expande a todos los POE reales de Panamá.',
                metadata_json = COALESCE(i.metadata_json, '{}'::jsonb)
                    || '{"pricingWorkflow":true,"multimodalViaPanama":true,"countryCode":"PA","virtualPoe":true}'::jsonb,
                sort_order = 5,
                is_active = TRUE,
                is_deleted = FALSE,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            FROM config."CatalogGroups" g
            WHERE i.catalog_group_id = g.id
              AND g.slug = 'poe'
              AND (i.code = 'MULTIMODAL_VIA_PANAMA' OR i.slug = 'multimodal-via-panama');
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DELETE FROM config."CatalogItems"
            WHERE code = 'MULTIMODAL_VIA_PANAMA'
               OR slug = 'multimodal-via-panama';
            """
        );
    }
}
