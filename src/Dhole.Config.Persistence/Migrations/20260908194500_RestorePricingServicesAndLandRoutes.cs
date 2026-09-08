using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260908194500_RestorePricingServicesAndLandRoutes")]
public sealed class RestorePricingServicesAndLandRoutes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            INSERT INTO config."CatalogGroups"
                (id, code, slug, name, description, metadata_json, is_system, is_active, created_at_utc, created_by, is_deleted)
            VALUES
                ('c2000000-0000-4000-8000-000000000003', 'PRICING_SERVICES', 'pricing-services', 'Servicios de Pricing', 'Servicios seleccionables asociados a líneas de tarifa.', '{"pricingWorkflow":true,"association":"rate-sections"}'::jsonb, TRUE, TRUE, NOW(), 'migration', FALSE),
                ('c2000000-0000-4000-8000-000000000021', 'LAND_POL', 'land-pol', 'Orígenes terrestres', 'Ciudades de origen disponibles para transporte terrestre.', '{"pricingWorkflow":true,"modality":"Land","routeRole":"POL"}'::jsonb, TRUE, TRUE, NOW(), 'migration', FALSE),
                ('c2000000-0000-4000-8000-000000000022', 'LAND_POE', 'land-poe', 'Destinos terrestres', 'Ciudades de salida/destino disponibles para transporte terrestre.', '{"pricingWorkflow":true,"modality":"Land","routeRole":"POE"}'::jsonb, TRUE, TRUE, NOW(), 'migration', FALSE)
            ON CONFLICT DO NOTHING;

            UPDATE config."CatalogGroups"
            SET is_active = TRUE,
                is_deleted = FALSE,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            WHERE slug IN ('pricing-services', 'land-pol', 'land-poe');

            WITH desired(id, code, slug, name, description, value, metadata_json, sort_order) AS (
                VALUES
                    ('c2300000-0000-4000-8000-000000000001'::uuid, 'INT_TRANSPORT', 'international-transport', 'Transporte Internacional', NULL, 'Transporte Internacional', '{"rateSections":["international_freight"],"defaultSelected":true}', 10),
                    ('c2300000-0000-4000-8000-000000000002'::uuid, 'CUSTOMS_CR', 'customs-agency-cr', 'Agencia de Aduanas CRC', NULL, 'Agencia de Aduanas CRC', '{"rateSections":["destination_charges"],"directionAware":true}', 20),
                    ('c2300000-0000-4000-8000-000000000003'::uuid, 'CUSTOMS_FOREIGN', 'customs-agency-foreign', 'Agencia de Aduanas Exterior', NULL, 'Agencia de Aduanas Exterior', '{"rateSections":["origin_charges"],"directionAware":true}', 30),
                    ('c2300000-0000-4000-8000-000000000004'::uuid, 'STORAGE', 'storage', 'Almacenamiento', NULL, 'Almacenamiento', '{"rateSections":["destination_charges"],"optional":true}', 40),
                    ('c2300000-0000-4000-8000-000000000005'::uuid, 'CARGO_INSURANCE', 'cargo-insurance', 'Seguro de carga', 'Calculado a partir del valor de la carga.', 'Seguro de carga', '{"rateSections":["destination_charges"],"optional":true,"requiresCargoValue":true,"saleFactor":0.65,"saleMinimumUsd":95,"costFactor":0.20,"costMinimumUsd":35}', 50),
                    ('c2300000-0000-4000-8000-000000000006'::uuid, 'INVENTORY_CONTROL', 'inventory-control', 'Control de inventario', NULL, 'Control de inventario', '{"rateSections":["destination_charges"],"optional":true}', 60),
                    ('c2300000-0000-4000-8000-000000000007'::uuid, 'PICKING', 'cargo-picking', 'Picking cargas', NULL, 'Picking cargas', '{"rateSections":["destination_charges"],"optional":true}', 70),
                    ('c2300000-0000-4000-8000-000000000008'::uuid, 'RECEPTION', 'cargo-reception', 'Recepción de carga', NULL, 'Recepción de carga', '{"rateSections":["origin_charges","destination_charges"],"optional":true}', 80),
                    ('c2300000-0000-4000-8000-000000000009'::uuid, 'PACKING', 'cargo-packing', 'Embalaje de carga', NULL, 'Embalaje de carga', '{"rateSections":["origin_charges"],"optional":true}', 90),
                    ('c2300000-0000-4000-8000-000000000010'::uuid, 'EXONERATION', 'exoneration', 'Exoneración', NULL, 'Exoneración', '{"rateSections":["destination_charges"],"optional":true}', 100),
                    ('c2300000-0000-4000-8000-000000000011'::uuid, 'DELIVERY', 'delivery-transport', 'Transporte entrega', NULL, 'Transporte entrega', '{"rateSections":["delivery_destination"]}', 110),
                    ('c2300000-0000-4000-8000-000000000012'::uuid, 'PICKUP', 'pickup-transport', 'Transporte recolección', NULL, 'Transporte recolección', '{"rateSections":["pickup_origin"]}', 120)
            )
            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT d.id, g.id, d.code, d.slug, d.name, d.description, d.value, d.metadata_json::jsonb, d.sort_order, TRUE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" g
            CROSS JOIN desired d
            WHERE g.slug = 'pricing-services'
              AND g.is_deleted = FALSE
              AND NOT EXISTS (
                  SELECT 1 FROM config."CatalogItems" i
                  WHERE i.catalog_group_id = g.id
                    AND i.is_deleted = FALSE
                    AND (UPPER(i.code) = UPPER(d.code) OR LOWER(i.slug) = LOWER(d.slug))
              )
            ON CONFLICT DO NOTHING;

            UPDATE config."CatalogItems" i
            SET is_active = TRUE,
                is_deleted = FALSE,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            FROM config."CatalogGroups" g
            WHERE i.catalog_group_id = g.id
              AND g.slug = 'pricing-services'
              AND i.code IN ('INT_TRANSPORT','CUSTOMS_CR','CUSTOMS_FOREIGN','STORAGE','CARGO_INSURANCE','INVENTORY_CONTROL','PICKING','RECEPTION','PACKING','EXONERATION','DELIVERY','PICKUP');

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
                SELECT id, slug FROM config."CatalogGroups" WHERE slug IN ('land-pol','land-poe') AND is_deleted = FALSE
            )
            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT
                CASE WHEN g.slug = 'land-pol' THEN d.id ELSE d.id + '00000000-0000-0000-0000-000000000100'::uuid END,
                g.id,
                d.code,
                d.slug,
                d.name,
                'Ubicación terrestre para Pricing.',
                d.name,
                jsonb_build_object('modality','Land','countryCode',d.country_code,'routeRole',CASE WHEN g.slug='land-pol' THEN 'POL' ELSE 'POE' END),
                d.sort_order,
                TRUE, TRUE, NOW(), 'migration', FALSE
            FROM target_groups g
            CROSS JOIN desired d
            WHERE NOT EXISTS (
                SELECT 1 FROM config."CatalogItems" i
                WHERE i.catalog_group_id = g.id AND i.is_deleted = FALSE
                  AND (UPPER(i.code) = UPPER(d.code) OR LOWER(i.slug) = LOWER(d.slug))
            )
            ON CONFLICT DO NOTHING;

            -- Selector comercial que permite buscar todos los POE panameños en Pantalla 5.
            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT
                'c2700000-0000-4000-8000-000000000001'::uuid,
                g.id,
                'MULTIMODAL_VIA_PANAMA',
                'multimodal-via-panama',
                'Multimodal Via Panamá',
                'Busca tarifas FCL cuyo POE esté en Panamá.',
                'Multimodal Via Panamá',
                '{"pricingWorkflow":true,"multimodalViaPanama":true,"countryCode":"PA"}'::jsonb,
                5,
                TRUE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" g
            WHERE g.slug = 'poe' AND g.is_deleted = FALSE
              AND NOT EXISTS (
                  SELECT 1 FROM config."CatalogItems" i
                  WHERE i.catalog_group_id = g.id AND i.is_deleted = FALSE
                    AND i.code = 'MULTIMODAL_VIA_PANAMA'
              )
            ON CONFLICT DO NOTHING;
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DELETE FROM config."CatalogItems" WHERE code = 'MULTIMODAL_VIA_PANAMA';
            DELETE FROM config."CatalogItems" i
            USING config."CatalogGroups" g
            WHERE i.catalog_group_id = g.id AND g.slug IN ('land-pol','land-poe');
            DELETE FROM config."CatalogGroups" WHERE slug IN ('land-pol','land-poe');
            """
        );
    }
}
