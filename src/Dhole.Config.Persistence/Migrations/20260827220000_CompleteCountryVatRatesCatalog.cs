using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260827220000_CompleteCountryVatRatesCatalog")]
public sealed class CompleteCountryVatRatesCatalog : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT v.id, cg.id, v.code, v.slug, v.name,
                   'Tasa general configurable para operaciones de Pricing.',
                   v.value, v.metadata_json::jsonb, v.sort_order,
                   TRUE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" cg
            CROSS JOIN (VALUES
                ('8e2710bb-89e8-4b9c-bc0c-55bb4da2e88e'::uuid, 'PA', 'panama', 'Panamá', '7', '{"countryCode":"PA","vatRate":7}', 20),
                ('58dc67bd-7948-4a12-b418-a7fd072f25e0'::uuid, 'GT', 'guatemala', 'Guatemala', '15', '{"countryCode":"GT","vatRate":15}', 30)
            ) AS v(id, code, slug, name, value, metadata_json, sort_order)
            WHERE cg.slug = 'country-vat-rates'
              AND NOT EXISTS (
                  SELECT 1 FROM config."CatalogItems" ci
                  WHERE ci.catalog_group_id = cg.id AND ci.code = v.code
              );

            UPDATE config."CatalogItems" ci
            SET value = CASE ci.code WHEN 'CR' THEN '13' WHEN 'PA' THEN '7' WHEN 'GT' THEN '15' END,
                metadata_json = COALESCE(ci.metadata_json, '{}'::jsonb) ||
                    CASE ci.code
                        WHEN 'CR' THEN '{"countryCode":"CR","vatRate":13}'::jsonb
                        WHEN 'PA' THEN '{"countryCode":"PA","vatRate":7}'::jsonb
                        WHEN 'GT' THEN '{"countryCode":"GT","vatRate":15}'::jsonb
                    END,
                is_active = TRUE, is_deleted = FALSE,
                deleted_at_utc = NULL, deleted_by = NULL,
                updated_at_utc = NOW(), updated_by = 'migration'
            FROM config."CatalogGroups" cg
            WHERE ci.catalog_group_id = cg.id
              AND cg.slug = 'country-vat-rates'
              AND ci.code IN ('CR', 'PA', 'GT');
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
            ) AND code IN ('PA', 'GT');
            """
        );
    }
}
