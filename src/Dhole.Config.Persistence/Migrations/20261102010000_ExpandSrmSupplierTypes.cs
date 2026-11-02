using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20261102010000_ExpandSrmSupplierTypes")]
public sealed class ExpandSrmSupplierTypes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Categories stay in Config and are safe to apply to existing SRM installations.
        // Supplier profiles are independent CatalogItems in srm-suppliers.
        migrationBuilder.Sql("""
INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'CARRIER','carrier','Naviera','Categoría de proveedor SRM','Naviera','{}'::jsonb,90,true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='srm-supplier-types' AND cg.is_deleted=false
AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" ci WHERE ci.catalog_group_id=cg.id AND ci.slug='carrier' AND ci.is_deleted=false);
INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'AGENT','agent','Agente de carga','Categoría de proveedor SRM','Agente de carga','{}'::jsonb,100,true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='srm-supplier-types' AND cg.is_deleted=false
AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" ci WHERE ci.catalog_group_id=cg.id AND ci.slug='agent' AND ci.is_deleted=false);
INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'AIRLINE','airline','Aerolínea','Categoría de proveedor SRM','Aerolínea','{}'::jsonb,110,true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='srm-supplier-types' AND cg.is_deleted=false
AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" ci WHERE ci.catalog_group_id=cg.id AND ci.slug='airline' AND ci.is_deleted=false);
INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'ROAD_TRANSPORT','road-transport','Transportista terrestre','Categoría de proveedor SRM','Transportista terrestre','{}'::jsonb,120,true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='srm-supplier-types' AND cg.is_deleted=false
AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" ci WHERE ci.catalog_group_id=cg.id AND ci.slug='road-transport' AND ci.is_deleted=false);
INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'CUSTOMS_FOREIGN','customs-foreign','Agencia aduanal extranjera','Categoría de proveedor SRM','Agencia aduanal extranjera','{}'::jsonb,130,true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='srm-supplier-types' AND cg.is_deleted=false
AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" ci WHERE ci.catalog_group_id=cg.id AND ci.slug='customs-foreign' AND ci.is_deleted=false);
INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'BONDED_WAREHOUSE_FOREIGN','bonded-warehouse-foreign','Almacén fiscal extranjero','Categoría de proveedor SRM','Almacén fiscal extranjero','{}'::jsonb,140,true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='srm-supplier-types' AND cg.is_deleted=false
AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" ci WHERE ci.catalog_group_id=cg.id AND ci.slug='bonded-warehouse-foreign' AND ci.is_deleted=false);
INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'SISTER_COMPANY','sister-company','Empresa hermana','Categoría de proveedor SRM','Empresa hermana','{}'::jsonb,150,true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='srm-supplier-types' AND cg.is_deleted=false
AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" ci WHERE ci.catalog_group_id=cg.id AND ci.slug='sister-company' AND ci.is_deleted=false);
INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'COLOADER','coloader','Coloader / consolidador','Categoría de proveedor SRM','Coloader / consolidador','{}'::jsonb,160,true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='srm-supplier-types' AND cg.is_deleted=false
AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" ci WHERE ci.catalog_group_id=cg.id AND ci.slug='coloader' AND ci.is_deleted=false);
INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'FOREIGN_AGENT','foreign-agent','Agente extranjero','Categoría de proveedor SRM','Agente extranjero','{}'::jsonb,170,true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='srm-supplier-types' AND cg.is_deleted=false
AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" ci WHERE ci.catalog_group_id=cg.id AND ci.slug='foreign-agent' AND ci.is_deleted=false);
INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'PORT_OPERATOR','port-operator','Operador portuario','Categoría de proveedor SRM','Operador portuario','{}'::jsonb,180,true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='srm-supplier-types' AND cg.is_deleted=false
AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" ci WHERE ci.catalog_group_id=cg.id AND ci.slug='port-operator' AND ci.is_deleted=false);
INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'GROUND_HANDLER','ground-handler','Agente de handling','Categoría de proveedor SRM','Agente de handling','{}'::jsonb,190,true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='srm-supplier-types' AND cg.is_deleted=false
AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" ci WHERE ci.catalog_group_id=cg.id AND ci.slug='ground-handler' AND ci.is_deleted=false);
""");
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Do not delete types already referenced by supplier profiles.
    }
}
