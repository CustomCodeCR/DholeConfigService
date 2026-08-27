using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260827201500_AddPricingWarehouses")]
public sealed class AddPricingWarehouses : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            INSERT INTO config."CatalogGroups"
                (id, code, slug, name, description, metadata_json, is_system, is_active, created_at_utc, created_by, is_deleted)
            VALUES
                (
                    'c2000000-0000-4000-8000-000000000006'::uuid,
                    'PRICING_WAREHOUSES',
                    'pricing-warehouses',
                    'WHS globales',
                    'Bodegas y warehouses globales disponibles para operaciones FCA en Pricing.',
                    '{"pricingWorkflow":true,"modality":"Global","locationCatalog":true,"metadataFields":["address","countryCode","latitude","longitude"]}'::jsonb,
                    TRUE,
                    TRUE,
                    NOW(),
                    'migration',
                    FALSE
                )
            ON CONFLICT DO NOTHING;

            UPDATE config."CatalogGroups"
            SET code = 'PRICING_WAREHOUSES',
                name = 'WHS globales',
                description = 'Bodegas y warehouses globales disponibles para operaciones FCA en Pricing.',
                metadata_json = COALESCE(metadata_json, '{}'::jsonb)
                    || '{"pricingWorkflow":true,"modality":"Global","locationCatalog":true,"metadataFields":["address","countryCode","latitude","longitude"]}'::jsonb,
                is_active = TRUE,
                is_deleted = FALSE,
                deleted_at_utc = NULL,
                deleted_by = NULL,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            WHERE slug = 'pricing-warehouses';
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DELETE FROM config."CatalogItems"
            WHERE catalog_group_id IN (
                SELECT id FROM config."CatalogGroups" WHERE slug = 'pricing-warehouses'
            );

            DELETE FROM config."CatalogGroups"
            WHERE slug = 'pricing-warehouses';
            """
        );
    }
}
