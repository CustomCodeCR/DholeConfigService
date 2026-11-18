using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20261118183000_ExpandSrmSupplierRouteCategories")]
public sealed class ExpandSrmSupplierRouteCategories : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(), cg.id, 'INTERNATIONAL_AIR', 'international-air', 'Transporte Internacional Aéreo',
  'Categoría logística SRM', 'Transporte Internacional Aéreo', '{}'::jsonb, 200, true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='srm-supplier-types' AND cg.is_deleted=false
AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" ci WHERE ci.catalog_group_id=cg.id AND ci.slug='international-air' AND ci.is_deleted=false);
INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(), cg.id, 'INTERNATIONAL_MARITIME', 'international-maritime', 'Transporte Internacional Marítimo',
  'Categoría logística SRM', 'Transporte Internacional Marítimo', '{}'::jsonb, 210, true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='srm-supplier-types' AND cg.is_deleted=false
AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" ci WHERE ci.catalog_group_id=cg.id AND ci.slug='international-maritime' AND ci.is_deleted=false);
INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(), cg.id, 'INTERNATIONAL_LAND', 'international-land', 'Transporte Internacional Terrestre',
  'Categoría logística SRM', 'Transporte Internacional Terrestre', '{}'::jsonb, 220, true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='srm-supplier-types' AND cg.is_deleted=false
AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" ci WHERE ci.catalog_group_id=cg.id AND ci.slug='international-land' AND ci.is_deleted=false);
INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(), cg.id, 'FISCAL_WAREHOUSE', 'fiscal-warehouse', 'Almacén Fiscal',
  'Categoría logística SRM', 'Almacén Fiscal', '{}'::jsonb, 230, true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='srm-supplier-types' AND cg.is_deleted=false
AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" ci WHERE ci.catalog_group_id=cg.id AND ci.slug='fiscal-warehouse' AND ci.is_deleted=false);
INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(), cg.id, 'GENERAL_WAREHOUSE', 'general-warehouse', 'Almacén General',
  'Categoría logística SRM', 'Almacén General', '{}'::jsonb, 240, true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='srm-supplier-types' AND cg.is_deleted=false
AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" ci WHERE ci.catalog_group_id=cg.id AND ci.slug='general-warehouse' AND ci.is_deleted=false);
INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(), cg.id, 'LAWYER', 'lawyer', 'Abogado',
  'Categoría logística SRM', 'Abogado', '{}'::jsonb, 250, true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='srm-supplier-types' AND cg.is_deleted=false
AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" ci WHERE ci.catalog_group_id=cg.id AND ci.slug='lawyer' AND ci.is_deleted=false);
INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(), cg.id, 'LOCAL_LAND', 'local-land', 'Transporte Local Terrestre',
  'Categoría logística SRM', 'Transporte Local Terrestre', '{}'::jsonb, 260, true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='srm-supplier-types' AND cg.is_deleted=false
AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" ci WHERE ci.catalog_group_id=cg.id AND ci.slug='local-land' AND ci.is_deleted=false);
INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(), cg.id, 'CUSTOMS_AGENCY', 'customs-agency', 'Agencia de Aduanas',
  'Categoría logística SRM', 'Agencia de Aduanas', '{}'::jsonb, 270, true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='srm-supplier-types' AND cg.is_deleted=false
AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" ci WHERE ci.catalog_group_id=cg.id AND ci.slug='customs-agency' AND ci.is_deleted=false);
INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(), cg.id, 'FREIGHT_CONSOLIDATOR', 'freight-consolidator', 'Consolidadora de Carga',
  'Categoría logística SRM', 'Consolidadora de Carga', '{}'::jsonb, 280, true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='srm-supplier-types' AND cg.is_deleted=false
AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" ci WHERE ci.catalog_group_id=cg.id AND ci.slug='freight-consolidator' AND ci.is_deleted=false);
INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(), cg.id, 'INVENTORY_CONTROL', 'inventory-control', 'Control de Inventario',
  'Categoría logística SRM', 'Control de Inventario', '{}'::jsonb, 290, true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='srm-supplier-types' AND cg.is_deleted=false
AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" ci WHERE ci.catalog_group_id=cg.id AND ci.slug='inventory-control' AND ci.is_deleted=false);
-- Retire the ambiguous legacy transport classification without deleting historical supplier metadata.
UPDATE config."CatalogItems" ci SET is_active=false, updated_at_utc=now(), updated_by='migration-srm-route-categories'
FROM config."CatalogGroups" cg WHERE ci.catalog_group_id=cg.id
AND cg.slug='srm-supplier-types'
AND ci.slug IN ('transport','road','road-transport','transportista') AND ci.is_deleted=false;
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Historical supplier links and classifications are deliberately not deleted during rollback.
    }
}
