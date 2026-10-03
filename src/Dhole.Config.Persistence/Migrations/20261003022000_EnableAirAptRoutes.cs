using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20261003022000_EnableAirAptRoutes")]
public sealed class EnableAirAptRoutes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            -- POL/POE remain the shared Pricing route catalogs.
            -- CY = maritime, SD = land, APT = air.
            UPDATE config."CatalogGroups"
            SET
                description = CASE slug
                    WHEN 'pol' THEN 'Puntos de origen de Pricing. CY = marítimo; SD = terrestre; APT = aéreo.'
                    WHEN 'poe' THEN 'Puntos de salida/destino de Pricing. CY = marítimo; SD = terrestre; APT = aéreo.'
                    ELSE description
                END,
                metadata_json = (COALESCE(metadata_json, '{}'::jsonb) - 'transportMode' - 'terminalType' - 'terminalTypes' - 'transportModes') ||
                    CASE slug
                        WHEN 'pol' THEN '{"pricingWorkflow":true,"routeRole":"POL","terminalTypes":["CY","SD","APT"],"transportModes":["Maritime","Land","Air"]}'::jsonb
                        WHEN 'poe' THEN '{"pricingWorkflow":true,"routeRole":"POE","terminalTypes":["CY","SD","APT"],"transportModes":["Maritime","Land","Air"]}'::jsonb
                        ELSE '{}'::jsonb
                    END,
                is_active = TRUE,
                is_deleted = FALSE,
                updated_at_utc = NOW(),
                updated_by = 'migration-air-apt-routes'
            WHERE slug IN ('pol', 'poe');

            -- Normalize any airport records that may already have been created manually
            -- in the shared POL/POE catalogs.
            UPDATE config."CatalogItems" i
            SET
                metadata_json = COALESCE(i.metadata_json, '{}'::jsonb) || jsonb_build_object(
                    'pricingWorkflow', true,
                    'transportMode', 'Air',
                    'terminalType', 'APT',
                    'routeRole', UPPER(g.slug)
                ),
                updated_at_utc = NOW(),
                updated_by = 'migration-air-apt-routes'
            FROM config."CatalogGroups" g
            WHERE i.catalog_group_id = g.id
              AND g.slug IN ('pol', 'poe')
              AND i.is_deleted = FALSE
              AND (
                    UPPER(COALESCE(i.metadata_json->>'terminalType', '')) = 'APT'
                 OR UPPER(i.code) LIKE 'APT\_%' ESCAPE '\'
              );

            -- Initial airports used by the current air-pricing rollout.
            -- Additional airports are managed as ordinary CatalogItems in POL/POE
            -- with transportMode=Air and terminalType=APT.
            WITH desired(ordinal, pol_id, poe_id, code, slug, value, country_code, iata_code, airport_name, city) AS (
                VALUES
                    (
                        10,
                        'c2700000-0000-4000-8000-000000000001'::uuid,
                        'c2710000-0000-4000-8000-000000000001'::uuid,
                        'APT_MIA',
                        'miami-international-airport-mia',
                        'MIA - Miami International Airport, Miami, United States',
                        'US',
                        'MIA',
                        'Miami International Airport',
                        'Miami'
                    ),
                    (
                        20,
                        'c2700000-0000-4000-8000-000000000002'::uuid,
                        'c2710000-0000-4000-8000-000000000002'::uuid,
                        'APT_SJO',
                        'juan-santamaria-international-airport-sjo',
                        'SJO - Juan Santamaria International Airport, Costa Rica',
                        'CR',
                        'SJO',
                        'Juan Santamaria International Airport',
                        'Alajuela'
                    ),
                    (
                        30,
                        'c2700000-0000-4000-8000-000000000003'::uuid,
                        'c2710000-0000-4000-8000-000000000003'::uuid,
                        'APT_MAD',
                        'adolfo-suarez-madrid-barajas-airport-mad',
                        'MAD - Adolfo Suarez Madrid-Barajas Airport, Madrid, Spain',
                        'ES',
                        'MAD',
                        'Adolfo Suarez Madrid-Barajas Airport',
                        'Madrid'
                    )
            )
            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order,
                 is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT
                CASE WHEN g.slug = 'pol' THEN d.pol_id ELSE d.poe_id END,
                g.id,
                d.code,
                d.slug || CASE WHEN g.slug = 'poe' THEN '-poe' ELSE '' END,
                d.value,
                'Aeropuerto APT editable para Pricing aéreo.',
                d.value,
                jsonb_build_object(
                    'pricingWorkflow', true,
                    'transportMode', 'Air',
                    'terminalType', 'APT',
                    'routeRole', UPPER(g.slug),
                    'countryCode', d.country_code,
                    'iataCode', d.iata_code,
                    'airportName', d.airport_name,
                    'city', d.city
                ),
                20000 + d.ordinal,
                FALSE,
                TRUE,
                NOW(),
                'migration-air-apt-routes',
                FALSE
            FROM config."CatalogGroups" g
            CROSS JOIN desired d
            WHERE g.slug IN ('pol', 'poe')
              AND g.is_deleted = FALSE
            ON CONFLICT (id) DO UPDATE SET
                catalog_group_id = EXCLUDED.catalog_group_id,
                code = EXCLUDED.code,
                slug = EXCLUDED.slug,
                name = EXCLUDED.name,
                description = EXCLUDED.description,
                value = EXCLUDED.value,
                metadata_json = EXCLUDED.metadata_json,
                sort_order = EXCLUDED.sort_order,
                is_system = FALSE,
                is_active = TRUE,
                is_deleted = FALSE,
                deleted_at_utc = NULL,
                deleted_by = NULL,
                updated_at_utc = NOW(),
                updated_by = 'migration-air-apt-routes';
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Non-destructive by design. Airport POL/POE ids may already be referenced by Pricing.
    }
}
