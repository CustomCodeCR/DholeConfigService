using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260827180000_AddCountryTaxRates")]
public sealed class AddCountryTaxRates : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            INSERT INTO config."CatalogGroups"
                (id, code, slug, name, description, metadata_json, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT 'c2200000-0000-4000-8000-000000000001'::uuid, 'COUNTRIES', 'countries',
                   'Países e impuestos', 'Países con porcentaje de IVA aplicable a cargos de destino.',
                   '{"pricingWorkflow":true,"taxMapping":true}'::jsonb,
                   TRUE, TRUE, NOW(), 'migration', FALSE
            WHERE NOT EXISTS (SELECT 1 FROM config."CatalogGroups" WHERE slug = 'countries');

            UPDATE config."CatalogGroups"
            SET is_active = TRUE, is_deleted = FALSE,
                metadata_json = COALESCE(metadata_json, '{}'::jsonb) || '{"pricingWorkflow":true,"taxMapping":true}'::jsonb,
                updated_at_utc = NOW(), updated_by = 'migration'
            WHERE slug = 'countries';

            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT v.id, g.id, v.code, v.slug, v.name, v.description, v.code, v.metadata_json::jsonb,
                   v.sort_order, TRUE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" g
            CROSS JOIN (VALUES
                ('c2210000-0000-4000-8000-000000000001'::uuid, 'CR', 'costa-rica', 'Costa Rica', 'IVA general 13%.', '{"countryCode":"CR","taxRate":13}', 10),
                ('c2210000-0000-4000-8000-000000000002'::uuid, 'PA', 'panama', 'Panamá', 'ITBMS aplicable 7%.', '{"countryCode":"PA","taxRate":7}', 20),
                ('c2210000-0000-4000-8000-000000000003'::uuid, 'GT', 'guatemala', 'Guatemala', 'IVA general 15%.', '{"countryCode":"GT","taxRate":15}', 30)
            ) AS v(id, code, slug, name, description, metadata_json, sort_order)
            WHERE g.slug = 'countries'
              AND NOT EXISTS (
                  SELECT 1 FROM config."CatalogItems" i
                  WHERE i.catalog_group_id = g.id AND i.code = v.code
              );

            UPDATE config."CatalogItems" i
            SET metadata_json = COALESCE(i.metadata_json, '{}'::jsonb) ||
                CASE i.code
                    WHEN 'CR' THEN '{"countryCode":"CR","taxRate":13}'::jsonb
                    WHEN 'PA' THEN '{"countryCode":"PA","taxRate":7}'::jsonb
                    WHEN 'GT' THEN '{"countryCode":"GT","taxRate":15}'::jsonb
                END,
                is_active = TRUE, is_deleted = FALSE,
                updated_at_utc = NOW(), updated_by = 'migration'
            WHERE i.catalog_group_id IN (SELECT id FROM config."CatalogGroups" WHERE slug = 'countries')
              AND i.code IN ('CR', 'PA', 'GT');

            UPDATE config."CatalogItems"
            SET metadata_json = COALESCE(metadata_json, '{}'::jsonb) ||
                CASE
                    WHEN upper(name) LIKE '%COSTA RICA%' THEN '{"countryCode":"CR"}'::jsonb
                    WHEN upper(name) LIKE '%PANAMA%' OR upper(name) LIKE '%PANAMÁ%' THEN '{"countryCode":"PA"}'::jsonb
                    WHEN upper(name) LIKE '%GUATEMALA%' THEN '{"countryCode":"GT"}'::jsonb
                    ELSE '{}'::jsonb
                END,
                updated_at_utc = NOW(), updated_by = 'migration'
            WHERE catalog_group_id IN (
                SELECT id FROM config."CatalogGroups" WHERE slug IN ('poe', 'pod')
            );
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE config."CatalogItems"
            SET is_active = FALSE, is_deleted = TRUE, deleted_at_utc = NOW(), deleted_by = 'migration'
            WHERE catalog_group_id IN (SELECT id FROM config."CatalogGroups" WHERE slug = 'countries')
              AND code IN ('CR', 'PA', 'GT');
            """
        );
    }
}
