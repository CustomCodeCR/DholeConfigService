using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260909050000_CleanPricingRouteCatalogs")]
public sealed class CleanPricingRouteCatalogs : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            -- POL and POE are the only route catalogs used by both maritime and land.
            UPDATE config."CatalogGroups"
            SET
                name = CASE slug WHEN 'pol' THEN 'POL' WHEN 'poe' THEN 'POE' ELSE name END,
                description = CASE slug
                    WHEN 'pol' THEN 'Puntos de origen de Pricing. CY = marítimo; SD = terrestre.'
                    WHEN 'poe' THEN 'Puntos de salida/destino de Pricing. CY = marítimo; SD = terrestre.'
                    ELSE description
                END,
                metadata_json = CASE slug
                    WHEN 'pol' THEN '{"pricingWorkflow":true,"routeRole":"POL","terminalTypes":["CY","SD"],"transportModes":["Maritime","Land"]}'::jsonb
                    WHEN 'poe' THEN '{"pricingWorkflow":true,"routeRole":"POE","terminalTypes":["CY","SD"],"transportModes":["Maritime","Land"]}'::jsonb
                    ELSE metadata_json
                END,
                is_active = TRUE,
                is_deleted = FALSE,
                updated_at_utc = NOW(),
                updated_by = 'migration-clean-route-catalogs'
            WHERE slug IN ('pol', 'poe');

            CREATE TEMP TABLE "_dhole_sd_routes" (
                ordinal integer NOT NULL,
                pol_id uuid NOT NULL,
                poe_id uuid NOT NULL,
                code text NOT NULL,
                slug text NOT NULL,
                name text NOT NULL,
                country_code text NOT NULL
            ) ON COMMIT DROP;

            INSERT INTO "_dhole_sd_routes" (ordinal, pol_id, poe_id, code, slug, name, country_code)
            VALUES
                (10,  'c2600000-0000-4000-8000-000000000001', 'c2610000-0000-4000-8000-000000000001', 'SD_CIUDAD_DE_MEXICO_MEXICO',        'ciudad-de-mexico-mexico',        'Ciudad de Mexico, Mexico',        'MX'),
                (20,  'c2600000-0000-4000-8000-000000000002', 'c2610000-0000-4000-8000-000000000002', 'SD_CIUDAD_DE_HIDALGO_MEXICO',       'ciudad-de-hidalgo-mexico',       'Ciudad de Hidalgo, Mexico',       'MX'),
                (30,  'c2600000-0000-4000-8000-000000000003', 'c2610000-0000-4000-8000-000000000003', 'SD_CIUDAD_DE_GUATEMALA_GUATEMALA',  'ciudad-de-guatemala-guatemala',  'Ciudad de Guatemala, Guatemala',  'GT'),
                (40,  'c2600000-0000-4000-8000-000000000004', 'c2610000-0000-4000-8000-000000000004', 'SD_SAN_SALVADOR_EL_SALVADOR',       'san-salvador-el-salvador',       'San Salvador, El Salvador',       'SV'),
                (50,  'c2600000-0000-4000-8000-000000000005', 'c2610000-0000-4000-8000-000000000005', 'SD_SAN_PEDRO_SULA_HONDURAS',         'san-pedro-sula-honduras',        'San Pedro Sula, Honduras',        'HN'),
                (60,  'c2600000-0000-4000-8000-000000000006', 'c2610000-0000-4000-8000-000000000006', 'SD_TEGUCIDALPA_HONDURAS',            'tegucidalpa-honduras',            'Tegucidalpa, Honduras',           'HN'),
                (70,  'c2600000-0000-4000-8000-000000000007', 'c2610000-0000-4000-8000-000000000007', 'SD_MANAGUA_NICARAGUA',               'managua-nicaragua',               'Managua, Nicaragua',              'NI'),
                (80,  'c2600000-0000-4000-8000-000000000008', 'c2610000-0000-4000-8000-000000000008', 'SD_SAN_JOSE_COSTA_RICA',              'san-jose-costa-rica',             'San Jose, Costa Rica',            'CR'),
                (90,  'c2600000-0000-4000-8000-000000000009', 'c2610000-0000-4000-8000-000000000009', 'SD_CIUDAD_DE_PANAMA_PANAMA',          'ciudad-de-panama-panama',         'Ciudad de Panama, Panama',        'PA'),
                (100, 'c2600000-0000-4000-8000-000000000010', 'c2610000-0000-4000-8000-000000000010', 'SD_COLON_FREE_ZONE_PANAMA',            'colon-free-zone-panama',          'Colon Free Zone, Panama',         'PA');

            -- Remove semantic duplicates of the ten canonical SD locations before normalizing them.
            DELETE FROM config."CatalogItems" i
            USING config."CatalogGroups" g, "_dhole_sd_routes" d
            WHERE i.catalog_group_id = g.id
              AND g.slug IN ('pol', 'poe')
              AND i.id <> CASE WHEN g.slug = 'pol' THEN d.pol_id ELSE d.poe_id END
              AND (
                    UPPER(i.code) = UPPER(d.code)
                 OR LOWER(i.slug) = LOWER(d.slug)
                 OR LOWER(i.slug) LIKE LOWER(d.slug) || '-sd-%'
                 OR regexp_replace(
                        translate(lower(COALESCE(NULLIF(i.value, ''), i.name)), 'áéíóúüñ', 'aeiouun'),
                        '[^a-z0-9]+', '', 'g'
                    ) = regexp_replace(
                        translate(lower(d.name), 'áéíóúüñ', 'aeiouun'),
                        '[^a-z0-9]+', '', 'g'
                    )
              );

            -- Guarantee the requested SD locations in both POL and POE, preserving the stable ids already used by Pricing.
            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order,
                 is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT
                CASE WHEN g.slug = 'pol' THEN d.pol_id ELSE d.poe_id END,
                g.id,
                d.code,
                d.slug,
                d.name,
                'Ubicación terrestre SD para Pricing.',
                d.name,
                jsonb_build_object(
                    'pricingWorkflow', true,
                    'transportMode', 'Land',
                    'terminalType', 'SD',
                    'routeRole', UPPER(g.slug),
                    'countryCode', d.country_code
                ),
                10000 + d.ordinal,
                TRUE,
                TRUE,
                NOW(),
                'migration-clean-route-catalogs',
                FALSE
            FROM config."CatalogGroups" g
            CROSS JOIN "_dhole_sd_routes" d
            WHERE g.slug IN ('pol', 'poe') AND g.is_deleted = FALSE
            ON CONFLICT (id) DO UPDATE SET
                catalog_group_id = EXCLUDED.catalog_group_id,
                code = EXCLUDED.code,
                slug = EXCLUDED.slug,
                name = EXCLUDED.name,
                description = EXCLUDED.description,
                value = EXCLUDED.value,
                metadata_json = EXCLUDED.metadata_json,
                sort_order = EXCLUDED.sort_order,
                is_system = TRUE,
                is_active = TRUE,
                is_deleted = FALSE,
                updated_at_utc = NOW(),
                updated_by = 'migration-clean-route-catalogs';

            -- Give every active POL/POE item the same metadata contract.
            -- The Panama pseudo-POE keeps one extra boolean because it expands to all real Panama CY POEs.
            UPDATE config."CatalogItems" i
            SET
                metadata_json = jsonb_build_object(
                    'pricingWorkflow', true,
                    'transportMode', CASE
                        WHEN UPPER(COALESCE(i.metadata_json->>'terminalType', '')) = 'SD'
                          OR UPPER(i.code) LIKE 'SD\_%' ESCAPE '\'
                        THEN 'Land' ELSE 'Maritime' END,
                    'terminalType', CASE
                        WHEN UPPER(COALESCE(i.metadata_json->>'terminalType', '')) = 'SD'
                          OR UPPER(i.code) LIKE 'SD\_%' ESCAPE '\'
                        THEN 'SD' ELSE 'CY' END,
                    'routeRole', UPPER(g.slug),
                    'countryCode', COALESCE(
                        NULLIF(UPPER(i.metadata_json->>'countryCode'), ''),
                        CASE
                            WHEN regexp_replace(UPPER(translate(CONCAT_WS(' ', i.code, i.slug, i.name, i.value), 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNAEIOUUN')), '[^A-Z0-9]+', '_', 'g') LIKE '%COSTA_RICA%' THEN 'CR'
                            WHEN regexp_replace(UPPER(translate(CONCAT_WS(' ', i.code, i.slug, i.name, i.value), 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNAEIOUUN')), '[^A-Z0-9]+', '_', 'g') LIKE '%EL_SALVADOR%' THEN 'SV'
                            WHEN regexp_replace(UPPER(translate(CONCAT_WS(' ', i.code, i.slug, i.name, i.value), 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNAEIOUUN')), '[^A-Z0-9]+', '_', 'g') LIKE '%ESTADOS_UNIDOS%' THEN 'US'
                            WHEN regexp_replace(UPPER(translate(CONCAT_WS(' ', i.code, i.slug, i.name, i.value), 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNAEIOUUN')), '[^A-Z0-9]+', '_', 'g') LIKE '%COREA_DEL_SUR%' THEN 'KR'
                            WHEN regexp_replace(UPPER(translate(CONCAT_WS(' ', i.code, i.slug, i.name, i.value), 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNAEIOUUN')), '[^A-Z0-9]+', '_', 'g') LIKE '%EMIRATOS_ARABES_UNIDOS%' THEN 'AE'
                            WHEN regexp_replace(UPPER(translate(CONCAT_WS(' ', i.code, i.slug, i.name, i.value), 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNAEIOUUN')), '[^A-Z0-9]+', '_', 'g') LIKE '%PAISES_BAJOS%' THEN 'NL'
                            WHEN regexp_replace(UPPER(translate(CONCAT_WS(' ', i.code, i.slug, i.name, i.value), 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNAEIOUUN')), '[^A-Z0-9]+', '_', 'g') LIKE '%ARGENTINA%' THEN 'AR'
                            WHEN regexp_replace(UPPER(translate(CONCAT_WS(' ', i.code, i.slug, i.name, i.value), 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNAEIOUUN')), '[^A-Z0-9]+', '_', 'g') LIKE '%ALEMANIA%' THEN 'DE'
                            WHEN regexp_replace(UPPER(translate(CONCAT_WS(' ', i.code, i.slug, i.name, i.value), 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNAEIOUUN')), '[^A-Z0-9]+', '_', 'g') LIKE '%BELGICA%' THEN 'BE'
                            WHEN regexp_replace(UPPER(translate(CONCAT_WS(' ', i.code, i.slug, i.name, i.value), 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNAEIOUUN')), '[^A-Z0-9]+', '_', 'g') LIKE '%BRASIL%' THEN 'BR'
                            WHEN regexp_replace(UPPER(translate(CONCAT_WS(' ', i.code, i.slug, i.name, i.value), 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNAEIOUUN')), '[^A-Z0-9]+', '_', 'g') LIKE '%CANADA%' THEN 'CA'
                            WHEN regexp_replace(UPPER(translate(CONCAT_WS(' ', i.code, i.slug, i.name, i.value), 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNAEIOUUN')), '[^A-Z0-9]+', '_', 'g') LIKE '%CHILE%' THEN 'CL'
                            WHEN regexp_replace(UPPER(translate(CONCAT_WS(' ', i.code, i.slug, i.name, i.value), 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNAEIOUUN')), '[^A-Z0-9]+', '_', 'g') LIKE '%CHINA%' THEN 'CN'
                            WHEN regexp_replace(UPPER(translate(CONCAT_WS(' ', i.code, i.slug, i.name, i.value), 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNAEIOUUN')), '[^A-Z0-9]+', '_', 'g') LIKE '%COLOMBIA%' THEN 'CO'
                            WHEN regexp_replace(UPPER(translate(CONCAT_WS(' ', i.code, i.slug, i.name, i.value), 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNAEIOUUN')), '[^A-Z0-9]+', '_', 'g') LIKE '%ECUADOR%' THEN 'EC'
                            WHEN regexp_replace(UPPER(translate(CONCAT_WS(' ', i.code, i.slug, i.name, i.value), 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNAEIOUUN')), '[^A-Z0-9]+', '_', 'g') LIKE '%ESPANA%' THEN 'ES'
                            WHEN regexp_replace(UPPER(translate(CONCAT_WS(' ', i.code, i.slug, i.name, i.value), 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNAEIOUUN')), '[^A-Z0-9]+', '_', 'g') LIKE '%FRANCIA%' THEN 'FR'
                            WHEN regexp_replace(UPPER(translate(CONCAT_WS(' ', i.code, i.slug, i.name, i.value), 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNAEIOUUN')), '[^A-Z0-9]+', '_', 'g') LIKE '%GUATEMALA%' THEN 'GT'
                            WHEN regexp_replace(UPPER(translate(CONCAT_WS(' ', i.code, i.slug, i.name, i.value), 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNAEIOUUN')), '[^A-Z0-9]+', '_', 'g') LIKE '%HONDURAS%' THEN 'HN'
                            WHEN regexp_replace(UPPER(translate(CONCAT_WS(' ', i.code, i.slug, i.name, i.value), 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNAEIOUUN')), '[^A-Z0-9]+', '_', 'g') LIKE '%INDIA%' THEN 'IN'
                            WHEN regexp_replace(UPPER(translate(CONCAT_WS(' ', i.code, i.slug, i.name, i.value), 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNAEIOUUN')), '[^A-Z0-9]+', '_', 'g') LIKE '%ITALIA%' THEN 'IT'
                            WHEN regexp_replace(UPPER(translate(CONCAT_WS(' ', i.code, i.slug, i.name, i.value), 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNAEIOUUN')), '[^A-Z0-9]+', '_', 'g') LIKE '%MEXICO%' THEN 'MX'
                            WHEN regexp_replace(UPPER(translate(CONCAT_WS(' ', i.code, i.slug, i.name, i.value), 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNAEIOUUN')), '[^A-Z0-9]+', '_', 'g') LIKE '%NICARAGUA%' THEN 'NI'
                            WHEN regexp_replace(UPPER(translate(CONCAT_WS(' ', i.code, i.slug, i.name, i.value), 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNAEIOUUN')), '[^A-Z0-9]+', '_', 'g') LIKE '%PANAMA%' THEN 'PA'
                            WHEN regexp_replace(UPPER(translate(CONCAT_WS(' ', i.code, i.slug, i.name, i.value), 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNAEIOUUN')), '[^A-Z0-9]+', '_', 'g') LIKE '%PERU%' THEN 'PE'
                            WHEN regexp_replace(UPPER(translate(CONCAT_WS(' ', i.code, i.slug, i.name, i.value), 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNAEIOUUN')), '[^A-Z0-9]+', '_', 'g') LIKE '%POLONIA%' THEN 'PL'
                            WHEN regexp_replace(UPPER(translate(CONCAT_WS(' ', i.code, i.slug, i.name, i.value), 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNAEIOUUN')), '[^A-Z0-9]+', '_', 'g') LIKE '%PORTUGAL%' THEN 'PT'
                            WHEN regexp_replace(UPPER(translate(CONCAT_WS(' ', i.code, i.slug, i.name, i.value), 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNAEIOUUN')), '[^A-Z0-9]+', '_', 'g') LIKE '%SUDAFRICA%' THEN 'ZA'
                            WHEN regexp_replace(UPPER(translate(CONCAT_WS(' ', i.code, i.slug, i.name, i.value), 'ÁÉÍÓÚÜÑáéíóúüñ', 'AEIOUUNAEIOUUN')), '[^A-Z0-9]+', '_', 'g') LIKE '%TURQUIA%' THEN 'TR'
                            ELSE NULL
                        END
                    )
                ) || CASE
                    WHEN UPPER(i.code) = 'MULTIMODAL_VIA_PANAMA'
                      OR COALESCE((i.metadata_json->>'multimodalViaPanama')::boolean, FALSE)
                    THEN '{"multimodalViaPanama":true}'::jsonb
                    ELSE '{}'::jsonb
                END,
                updated_at_utc = NOW(),
                updated_by = 'migration-clean-route-catalogs'
            FROM config."CatalogGroups" g
            WHERE i.catalog_group_id = g.id
              AND g.slug IN ('pol', 'poe')
              AND i.is_deleted = FALSE;

            -- Remove repeated CY/SD locations inside the same catalog using a semantic display key.
            -- Prefer reviewed CY_/SD_ system records when two old variants represent the same place.
            WITH normalized AS (
                SELECT
                    i.id,
                    ROW_NUMBER() OVER (
                        PARTITION BY
                            i.catalog_group_id,
                            i.metadata_json->>'terminalType',
                            regexp_replace(
                                regexp_replace(
                                    translate(lower(COALESCE(NULLIF(i.value, ''), i.name)), 'áéíóúüñ', 'aeiouun'),
                                    '^(puerto|port)[[:space:]]+', '', 'g'
                                ),
                                '[^a-z0-9]+', '', 'g'
                            )
                        ORDER BY
                            CASE
                                WHEN UPPER(i.code) LIKE UPPER(i.metadata_json->>'terminalType') || '\_%' ESCAPE '\' THEN 0
                                ELSE 1
                            END,
                            i.is_system DESC,
                            i.created_at_utc ASC,
                            i.id
                    ) AS rn
                FROM config."CatalogItems" i
                JOIN config."CatalogGroups" g ON g.id = i.catalog_group_id
                WHERE g.slug IN ('pol', 'poe')
                  AND i.is_deleted = FALSE
            )
            DELETE FROM config."CatalogItems" i
            USING normalized n
            WHERE i.id = n.id AND n.rn > 1;

            -- CY first and SD second, each section alphabetically ordered without accents.
            WITH ordered AS (
                SELECT
                    i.id,
                    CASE WHEN i.metadata_json->>'terminalType' = 'SD' THEN 10000 ELSE 0 END
                    + ROW_NUMBER() OVER (
                        PARTITION BY i.catalog_group_id, i.metadata_json->>'terminalType'
                        ORDER BY
                            translate(lower(COALESCE(NULLIF(i.value, ''), i.name)), 'áéíóúüñ', 'aeiouun'),
                            i.id
                    ) * 10 AS new_sort_order
                FROM config."CatalogItems" i
                JOIN config."CatalogGroups" g ON g.id = i.catalog_group_id
                WHERE g.slug IN ('pol', 'poe')
                  AND i.is_deleted = FALSE
            )
            UPDATE config."CatalogItems" i
            SET
                sort_order = ordered.new_sort_order,
                updated_at_utc = NOW(),
                updated_by = 'migration-clean-route-catalogs'
            FROM ordered
            WHERE i.id = ordered.id;

            -- These temporary catalogs are no longer read by Web or Pricing.
            -- Hard-delete them so Config exposes only the catalogs that are actually used.
            DELETE FROM config."CatalogGroups"
            WHERE slug IN ('land-pol', 'land-poe');
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Intentionally non-destructive. Route ids can already be referenced by Pricing drafts/rates.
    }
}
