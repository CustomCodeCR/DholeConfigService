using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260825150000_AddPricingWorkflowCatalogs")]
public sealed class AddPricingWorkflowCatalogs : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            INSERT INTO config."CatalogGroups"
                (id, code, slug, name, description, metadata_json, is_system, is_active, created_at_utc, created_by, is_deleted)
            VALUES
                ('c2000000-0000-4000-8000-000000000001', 'TRANSPORT_MODALITIES', 'transport-modalities', 'Modalidades de transporte', 'Modalidades disponibles para construir una alternativa de Pricing.', '{"pricingWorkflow":true}'::jsonb, TRUE, TRUE, NOW(), 'migration', FALSE),
                ('c2000000-0000-4000-8000-000000000002', 'SHIPMENT_MODES', 'shipment-modes', 'Tipos de embarque', 'Tipos de embarque permitidos por modalidad.', '{"pricingWorkflow":true}'::jsonb, TRUE, TRUE, NOW(), 'migration', FALSE),
                ('c2000000-0000-4000-8000-000000000003', 'PRICING_SERVICES', 'pricing-services', 'Servicios de Pricing', 'Servicios seleccionables asociados a líneas de tarifa.', '{"pricingWorkflow":true,"association":"rate-sections"}'::jsonb, TRUE, TRUE, NOW(), 'migration', FALSE),
                ('c2000000-0000-4000-8000-000000000004', 'LAND_EQUIPMENT_TYPES', 'land-equipment-types', 'Equipos terrestres', 'Equipos disponibles para transporte terrestre.', '{"pricingWorkflow":true,"modality":"Land"}'::jsonb, TRUE, TRUE, NOW(), 'migration', FALSE)
            ON CONFLICT DO NOTHING;

            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT v.id, g.id, v.code, v.slug, v.name, v.description, v.value, v.metadata_json::jsonb, v.sort_order, TRUE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" g
            CROSS JOIN (VALUES
                ('c2100000-0000-4000-8000-000000000001'::uuid, 'MARITIME', 'maritime', 'Marítimo', 'Transporte marítimo.', 'Maritime', '{"shipmentModes":["FCL","LCL"]}', 10),
                ('c2100000-0000-4000-8000-000000000002'::uuid, 'AIR', 'air', 'Aéreo', 'Transporte aéreo.', 'Air', '{"shipmentModes":["LCL"]}', 20),
                ('c2100000-0000-4000-8000-000000000003'::uuid, 'LAND', 'land', 'Terrestre', 'Transporte terrestre.', 'Land', '{"shipmentModes":["FTL","FCL"]}', 30),
                ('c2100000-0000-4000-8000-000000000004'::uuid, 'MULTIMODAL', 'multimodal', 'Multimodal', 'Combinación marítimo-terrestre.', 'Multimodal', '{"shipmentModes":["FCL","LCL"],"legs":["Maritime","Land"]}', 40)
            ) AS v(id, code, slug, name, description, value, metadata_json, sort_order)
            WHERE g.slug = 'transport-modalities'
            ON CONFLICT (catalog_group_id, code) DO UPDATE
              SET name = EXCLUDED.name, description = EXCLUDED.description, value = EXCLUDED.value,
                  metadata_json = EXCLUDED.metadata_json, is_active = TRUE, updated_at_utc = NOW(), updated_by = 'migration';

            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT v.id, g.id, v.code, v.slug, v.name, v.description, v.value, v.metadata_json::jsonb, v.sort_order, TRUE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" g
            CROSS JOIN (VALUES
                ('c2200000-0000-4000-8000-000000000001'::uuid, 'FCL', 'fcl', 'FCL', 'Carga completa de contenedor.', 'FCL', '{"modalities":["Maritime","Land","Multimodal"],"consolidationOptions":["Own"]}', 10),
                ('c2200000-0000-4000-8000-000000000002'::uuid, 'LCL', 'lcl', 'LCL', 'Carga consolidada.', 'LCL', '{"modalities":["Maritime","Air","Multimodal"],"consolidationOptions":["Coloading","Own"],"multimodalLabel":"Consolidado propio"}', 20),
                ('c2200000-0000-4000-8000-000000000003'::uuid, 'FTL', 'ftl', 'FTL', 'Camión completo.', 'FTL', '{"modalities":["Land"]}', 30),
                ('c2200000-0000-4000-8000-000000000004'::uuid, 'LTL', 'ltl', 'LTL', 'Carga terrestre consolidada. Se conserva para configuración futura.', 'LTL', '{"modalities":[]}', 40)
            ) AS v(id, code, slug, name, description, value, metadata_json, sort_order)
            WHERE g.slug = 'shipment-modes'
            ON CONFLICT (catalog_group_id, code) DO UPDATE
              SET name = EXCLUDED.name, description = EXCLUDED.description, value = EXCLUDED.value,
                  metadata_json = EXCLUDED.metadata_json, is_active = TRUE, updated_at_utc = NOW(), updated_by = 'migration';

            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT v.id, g.id, v.code, v.slug, v.name, v.description, v.value, v.metadata_json::jsonb, v.sort_order, TRUE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" g
            CROSS JOIN (VALUES
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
            ) AS v(id, code, slug, name, description, value, metadata_json, sort_order)
            WHERE g.slug = 'pricing-services'
            ON CONFLICT (catalog_group_id, code) DO UPDATE
              SET name = EXCLUDED.name, description = EXCLUDED.description, value = EXCLUDED.value,
                  metadata_json = EXCLUDED.metadata_json, is_active = TRUE, updated_at_utc = NOW(), updated_by = 'migration';

            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT v.id, g.id, v.code, v.slug, v.name, v.description, v.value, v.metadata_json::jsonb, v.sort_order, TRUE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" g
            CROSS JOIN (VALUES
                ('c2400000-0000-4000-8000-000000000001'::uuid, '20DV', '20-dry-van', '20 Dry Van', 'Contenedor terrestre de 20 pies.', '20DV', '{"size":"20","kind":"dry-van","modalities":["Land","Multimodal"]}', 10),
                ('c2400000-0000-4000-8000-000000000002'::uuid, '40DV', '40-dry-van', '40 Dry Van', 'Contenedor terrestre de 40 pies.', '40DV', '{"size":"40","kind":"dry-van","modalities":["Land","Multimodal"]}', 20),
                ('c2400000-0000-4000-8000-000000000003'::uuid, '40HC', '40-high-cube', '40 High Cube', 'Contenedor terrestre High Cube de 40 pies.', '40HC', '{"size":"40","kind":"high-cube","modalities":["Land","Multimodal"]}', 30),
                ('c2400000-0000-4000-8000-000000000004'::uuid, '45HC', '45-high-cube', '45 High Cube', 'Contenedor terrestre High Cube de 45 pies.', '45HC', '{"size":"45","kind":"high-cube","modalities":["Land","Multimodal"]}', 40),
                ('c2400000-0000-4000-8000-000000000005'::uuid, '48HC', '48-high-cube', '48 High Cube', 'Contenedor terrestre High Cube de 48 pies.', '48HC', '{"size":"48","kind":"high-cube","modalities":["Land","Multimodal"]}', 50)
            ) AS v(id, code, slug, name, description, value, metadata_json, sort_order)
            WHERE g.slug = 'land-equipment-types'
            ON CONFLICT (catalog_group_id, code) DO UPDATE
              SET name = EXCLUDED.name, description = EXCLUDED.description, value = EXCLUDED.value,
                  metadata_json = EXCLUDED.metadata_json, is_active = TRUE, updated_at_utc = NOW(), updated_by = 'migration';

            -- Maritime equipment remains in the canonical container-types catalog. Mark it explicitly by modality.
            UPDATE config."CatalogItems" i
            SET metadata_json = COALESCE(i.metadata_json, '{}'::jsonb) || '{"modalities":["Maritime","Multimodal"],"shipmentModes":["FCL"]}'::jsonb,
                updated_at_utc = NOW(), updated_by = 'migration'
            FROM config."CatalogGroups" g
            WHERE i.catalog_group_id = g.id AND g.slug = 'container-types' AND i.is_deleted = FALSE;

            -- Incoterms drive which major rate sections are visible. Services can add sections on top of this base.
            INSERT INTO config."CatalogGroups"
                (id, code, slug, name, description, metadata_json, is_system, is_active, created_at_utc, created_by, is_deleted)
            VALUES
                ('c2000000-0000-4000-8000-000000000005', 'INCOTERMS', 'incoterms', 'Incoterms', 'Incoterms asociados a las líneas de tarifa.', '{"pricingWorkflow":true,"association":"rate-sections"}'::jsonb, TRUE, TRUE, NOW(), 'migration', FALSE)
            ON CONFLICT DO NOTHING;

            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT v.id, g.id, v.code, v.slug, v.name, NULL, v.code, v.metadata_json::jsonb, v.sort_order, TRUE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" g
            CROSS JOIN (VALUES
                ('c2500000-0000-4000-8000-000000000001'::uuid, 'EXW', 'exw', 'EXW', '{"rateSections":["pickup_origin","origin_charges","international_freight","destination_charges","delivery_destination"]}', 10),
                ('c2500000-0000-4000-8000-000000000002'::uuid, 'FCA', 'fca', 'FCA', '{"rateSections":["origin_charges","international_freight","destination_charges","delivery_destination"]}', 20),
                ('c2500000-0000-4000-8000-000000000003'::uuid, 'FAS', 'fas', 'FAS', '{"rateSections":["international_freight","destination_charges","delivery_destination"]}', 30),
                ('c2500000-0000-4000-8000-000000000004'::uuid, 'FOB', 'fob', 'FOB', '{"rateSections":["international_freight","destination_charges","delivery_destination"]}', 40),
                ('c2500000-0000-4000-8000-000000000005'::uuid, 'CFR', 'cfr', 'CFR', '{"rateSections":["destination_charges","delivery_destination"]}', 50),
                ('c2500000-0000-4000-8000-000000000006'::uuid, 'CIF', 'cif', 'CIF', '{"rateSections":["destination_charges","delivery_destination"]}', 60),
                ('c2500000-0000-4000-8000-000000000007'::uuid, 'CPT', 'cpt', 'CPT', '{"rateSections":["destination_charges","delivery_destination"]}', 70),
                ('c2500000-0000-4000-8000-000000000008'::uuid, 'CIP', 'cip', 'CIP', '{"rateSections":["destination_charges","delivery_destination"]}', 80),
                ('c2500000-0000-4000-8000-000000000009'::uuid, 'DAP', 'dap', 'DAP', '{"rateSections":["destination_charges","delivery_destination"]}', 90),
                ('c2500000-0000-4000-8000-000000000010'::uuid, 'DPU', 'dpu', 'DPU', '{"rateSections":["destination_charges"]}', 100),
                ('c2500000-0000-4000-8000-000000000011'::uuid, 'DDP', 'ddp', 'DDP', '{"rateSections":["destination_charges"]}', 110)
            ) AS v(id, code, slug, name, metadata_json, sort_order)
            WHERE g.slug = 'incoterms'
            ON CONFLICT (catalog_group_id, code) DO UPDATE
              SET name = EXCLUDED.name, value = EXCLUDED.value, metadata_json = EXCLUDED.metadata_json,
                  is_active = TRUE, updated_at_utc = NOW(), updated_by = 'migration';
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DELETE FROM config."CatalogItems"
            WHERE created_by = 'migration'
              AND (id::text LIKE 'c21%' OR id::text LIKE 'c22%' OR id::text LIKE 'c23%' OR id::text LIKE 'c24%' OR id::text LIKE 'c25%');

            DELETE FROM config."CatalogGroups"
            WHERE slug IN ('transport-modalities', 'shipment-modes', 'pricing-services', 'land-equipment-types')
              AND created_by = 'migration';
            """
        );
    }
}
