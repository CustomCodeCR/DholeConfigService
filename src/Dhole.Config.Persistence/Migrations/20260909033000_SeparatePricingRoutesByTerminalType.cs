using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260909033000_SeparatePricingRoutesByTerminalType")]
public sealed class SeparatePricingRoutesByTerminalType : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            INSERT INTO config."CatalogGroups"
                (id, code, slug, name, description, metadata_json, is_system, is_active, created_at_utc, created_by, is_deleted)
            VALUES
                ('c2000000-0000-4000-8000-000000000021', 'LAND_POL', 'land-pol', 'POL terrestre (SD)', 'Puntos de origen terrestres para Pricing. Door/terminal terrestre (SD).', '{"pricingWorkflow":true,"transportMode":"Land","terminalType":"SD","routeRole":"POL"}'::jsonb, TRUE, TRUE, NOW(), 'migration-route-terminal-types', FALSE),
                ('c2000000-0000-4000-8000-000000000022', 'LAND_POE', 'land-poe', 'POE terrestre (SD)', 'Puntos de destino terrestres para Pricing. Door/terminal terrestre (SD).', '{"pricingWorkflow":true,"transportMode":"Land","terminalType":"SD","routeRole":"POE"}'::jsonb, TRUE, TRUE, NOW(), 'migration-route-terminal-types', FALSE)
            ON CONFLICT DO NOTHING;

            UPDATE config."CatalogGroups"
            SET
                name = CASE slug
                    WHEN 'pol' THEN 'POL marítimo (CY)'
                    WHEN 'poe' THEN 'POE marítimo (CY)'
                    WHEN 'pod' THEN 'POD marítimo (SD)'
                    WHEN 'land-pol' THEN 'POL terrestre (SD)'
                    WHEN 'land-poe' THEN 'POE terrestre (SD)'
                    ELSE name
                END,
                metadata_json = COALESCE(metadata_json, '{}'::jsonb) ||
                    CASE slug
                        WHEN 'pol' THEN '{"pricingWorkflow":true,"transportMode":"Maritime","terminalType":"CY","routeRole":"POL"}'::jsonb
                        WHEN 'poe' THEN '{"pricingWorkflow":true,"transportMode":"Maritime","terminalType":"CY","routeRole":"POE"}'::jsonb
                        WHEN 'pod' THEN '{"pricingWorkflow":true,"transportMode":"Maritime","terminalType":"SD","routeRole":"POD"}'::jsonb
                        WHEN 'land-pol' THEN '{"pricingWorkflow":true,"transportMode":"Land","terminalType":"SD","routeRole":"POL"}'::jsonb
                        WHEN 'land-poe' THEN '{"pricingWorkflow":true,"transportMode":"Land","terminalType":"SD","routeRole":"POE"}'::jsonb
                        ELSE '{}'::jsonb
                    END,
                is_active = TRUE,
                is_deleted = FALSE,
                updated_at_utc = NOW(),
                updated_by = 'migration-route-terminal-types'
            WHERE slug IN ('pol', 'poe', 'pod', 'land-pol', 'land-poe');

            UPDATE config."CatalogItems" i
            SET
                metadata_json = COALESCE(i.metadata_json, '{}'::jsonb) ||
                    CASE g.slug
                        WHEN 'pol' THEN '{"transportMode":"Maritime","terminalType":"CY","routeRole":"POL"}'::jsonb
                        WHEN 'poe' THEN '{"transportMode":"Maritime","terminalType":"CY","routeRole":"POE"}'::jsonb
                        WHEN 'pod' THEN '{"transportMode":"Maritime","terminalType":"SD","routeRole":"POD"}'::jsonb
                        WHEN 'land-pol' THEN '{"modality":"Land","transportMode":"Land","terminalType":"SD","routeRole":"POL"}'::jsonb
                        WHEN 'land-poe' THEN '{"modality":"Land","transportMode":"Land","terminalType":"SD","routeRole":"POE"}'::jsonb
                        ELSE '{}'::jsonb
                    END,
                updated_at_utc = NOW(),
                updated_by = 'migration-route-terminal-types'
            FROM config."CatalogGroups" g
            WHERE i.catalog_group_id = g.id
              AND g.slug IN ('pol', 'poe', 'pod', 'land-pol', 'land-poe')
              AND i.is_deleted = FALSE;

            WITH desired(id, code, slug, name, sort_order, country_code) AS (
                VALUES
                    ('c2600000-0000-4000-8000-000000000001'::uuid, 'MX-MEXICO-CITY', 'ciudad-de-mexico-mexico', 'Ciudad de Mexico, Mexico', 10, 'MX'),
                    ('c2600000-0000-4000-8000-000000000002'::uuid, 'MX-HIDALGO-CITY', 'ciudad-de-hidalgo-mexico', 'Ciudad de Hidalgo, Mexico', 20, 'MX'),
                    ('c2600000-0000-4000-8000-000000000003'::uuid, 'GT-GUATEMALA-CITY', 'ciudad-de-guatemala-guatemala', 'Ciudad de Guatemala, Guatemala', 30, 'GT'),
                    ('c2600000-0000-4000-8000-000000000004'::uuid, 'SV-SAN-SALVADOR', 'san-salvador-el-salvador', 'San Salvador, El Salvador', 40, 'SV'),
                    ('c2600000-0000-4000-8000-000000000005'::uuid, 'HN-SAN-PEDRO-SULA', 'san-pedro-sula-honduras', 'San Pedro Sula, Honduras', 50, 'HN'),
                    ('c2600000-0000-4000-8000-000000000006'::uuid, 'HN-TEGUCIGALPA', 'tegucigalpa-honduras', 'Tegucidalpa, Honduras', 60, 'HN'),
                    ('c2600000-0000-4000-8000-000000000007'::uuid, 'NI-MANAGUA', 'managua-nicaragua', 'Managua, Nicaragua', 70, 'NI'),
                    ('c2600000-0000-4000-8000-000000000008'::uuid, 'CR-SAN-JOSE', 'san-jose-costa-rica', 'San Jose, Costa Rica', 80, 'CR'),
                    ('c2600000-0000-4000-8000-000000000009'::uuid, 'PA-PANAMA-CITY', 'ciudad-de-panama-panama', 'Ciudad de Panama, Panama', 90, 'PA'),
                    ('c2600000-0000-4000-8000-000000000010'::uuid, 'PA-COLON-FREE-ZONE', 'colon-free-zone-panama', 'Colon Free Zone, Panama', 100, 'PA')
            ), target_groups AS (
                SELECT id, slug
                FROM config."CatalogGroups"
                WHERE slug IN ('land-pol', 'land-poe') AND is_deleted = FALSE
            )
            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT
                CASE
                    WHEN g.slug = 'land-pol' THEN d.id
                    ELSE replace(d.id::text, 'c2600000', 'c2610000')::uuid
                END,
                g.id,
                d.code,
                d.slug,
                d.name,
                'Punto terrestre para Pricing · Store Door / destino terrestre (SD).',
                d.name,
                jsonb_build_object(
                    'modality', 'Land',
                    'transportMode', 'Land',
                    'terminalType', 'SD',
                    'countryCode', d.country_code,
                    'routeRole', CASE WHEN g.slug = 'land-pol' THEN 'POL' ELSE 'POE' END
                ),
                d.sort_order,
                TRUE,
                TRUE,
                NOW(),
                'migration-route-terminal-types',
                FALSE
            FROM target_groups g
            CROSS JOIN desired d
            WHERE NOT EXISTS (
                SELECT 1
                FROM config."CatalogItems" i
                WHERE i.catalog_group_id = g.id
                  AND i.is_deleted = FALSE
                  AND (
                      UPPER(i.code) = UPPER(d.code)
                      OR LOWER(i.slug) = LOWER(d.slug)
                      OR LOWER(TRIM(COALESCE(i.value, ''))) = LOWER(TRIM(d.name))
                  )
            )
            ON CONFLICT DO NOTHING;

            UPDATE config."CatalogItems" i
            SET
                metadata_json = COALESCE(i.metadata_json, '{}'::jsonb) || jsonb_build_object(
                    'modality', 'Land',
                    'transportMode', 'Land',
                    'terminalType', 'SD',
                    'routeRole', CASE WHEN g.slug = 'land-pol' THEN 'POL' ELSE 'POE' END
                ),
                is_active = TRUE,
                is_deleted = FALSE,
                updated_at_utc = NOW(),
                updated_by = 'migration-route-terminal-types'
            FROM config."CatalogGroups" g
            WHERE i.catalog_group_id = g.id
              AND g.slug IN ('land-pol', 'land-poe');
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Intentionally non-destructive: route catalogs may already be referenced by Pricing records.
    }
}
