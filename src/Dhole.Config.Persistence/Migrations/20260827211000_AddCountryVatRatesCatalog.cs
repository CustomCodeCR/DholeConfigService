using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260827211000_AddCountryVatRatesCatalog")]
public sealed class AddCountryVatRatesCatalog : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            INSERT INTO config."CatalogGroups"
                (id, code, slug, name, description, metadata_json, is_system, is_active, created_at_utc, created_by, is_deleted)
            VALUES
                (
                    'c2000000-0000-4000-8000-000000000007'::uuid,
                    'COUNTRY_VAT_RATES',
                    'country-vat-rates',
                    'IVA por país',
                    'Mapeo editable de la tasa de IVA aplicable por país para Pricing. La tasa se guarda como porcentaje en metadata.vatRate.',
                    '{"pricingWorkflow":true,"countryTaxCatalog":true,"metadataFields":["countryCode","vatRate"],"rateUnit":"percent"}'::jsonb,
                    TRUE,
                    TRUE,
                    NOW(),
                    'migration',
                    FALSE
                )
            ON CONFLICT DO NOTHING;

            UPDATE config."CatalogGroups"
            SET code = 'COUNTRY_VAT_RATES',
                name = 'IVA por país',
                description = 'Mapeo editable de la tasa de IVA aplicable por país para Pricing. La tasa se guarda como porcentaje en metadata.vatRate.',
                metadata_json = COALESCE(metadata_json, '{}'::jsonb)
                    || '{"pricingWorkflow":true,"countryTaxCatalog":true,"metadataFields":["countryCode","vatRate"],"rateUnit":"percent"}'::jsonb,
                is_active = TRUE,
                is_deleted = FALSE,
                deleted_at_utc = NULL,
                deleted_by = NULL,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            WHERE slug = 'country-vat-rates';

            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT
                'c2100000-0000-4000-8000-000000000001'::uuid,
                cg.id,
                'CR',
                'costa-rica',
                'Costa Rica',
                'Tasa general de IVA configurable para operaciones de Pricing.',
                '13',
                '{"countryCode":"CR","vatRate":13}'::jsonb,
                10,
                TRUE,
                TRUE,
                NOW(),
                'migration',
                FALSE
            FROM config."CatalogGroups" cg
            WHERE cg.slug = 'country-vat-rates'
            ON CONFLICT DO NOTHING;

            UPDATE config."CatalogItems" ci
            SET value = '13',
                metadata_json = COALESCE(ci.metadata_json, '{}'::jsonb)
                    || '{"countryCode":"CR","vatRate":13}'::jsonb,
                is_active = TRUE,
                is_deleted = FALSE,
                deleted_at_utc = NULL,
                deleted_by = NULL,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            FROM config."CatalogGroups" cg
            WHERE ci.catalog_group_id = cg.id
              AND cg.slug = 'country-vat-rates'
              AND ci.code = 'CR';
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DELETE FROM config."CatalogItems"
            WHERE catalog_group_id IN (
                SELECT id FROM config."CatalogGroups" WHERE slug = 'country-vat-rates'
            );

            DELETE FROM config."CatalogGroups"
            WHERE slug = 'country-vat-rates';
            """
        );
    }
}
