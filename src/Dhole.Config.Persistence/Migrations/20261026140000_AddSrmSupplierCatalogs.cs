using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20261026140000_AddSrmSupplierCatalogs")]
public sealed class AddSrmSupplierCatalogs : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
INSERT INTO config."CatalogGroups" (id,code,slug,name,description,metadata_json,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),'SRM_SUPPLIERS','srm-suppliers','Proveedores SRM','Directorio independiente de proveedores: transportistas, navieras, agentes y otras empresas','{"module":"srm","editable":true}'::jsonb,true,true,now(),'migration',false
WHERE NOT EXISTS (SELECT 1 FROM config."CatalogGroups" WHERE slug='srm-suppliers' AND is_deleted=false);
INSERT INTO config."CatalogGroups" (id,code,slug,name,description,metadata_json,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),'SRM_SUPPLIER_TYPES','srm-supplier-types','Tipos de proveedor SRM','Clasificación extensible del SRM independiente de agents/carriers','{"module":"srm","editable":true}'::jsonb,true,true,now(),'migration',false
WHERE NOT EXISTS (SELECT 1 FROM config."CatalogGroups" WHERE slug='srm-supplier-types' AND is_deleted=false);
INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'ROAD','road','Transporte terrestre','Categoría de proveedor SRM','Transporte terrestre','{}'::jsonb,10,true,true,now(),'migration',false FROM config."CatalogGroups" cg WHERE cg.slug='srm-supplier-types' AND cg.is_deleted=false AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" ci WHERE ci.catalog_group_id=cg.id AND ci.code='ROAD' AND ci.is_deleted=false);
INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'SEA','sea','Transporte marítimo','Categoría de proveedor SRM','Transporte marítimo','{}'::jsonb,20,true,true,now(),'migration',false FROM config."CatalogGroups" cg WHERE cg.slug='srm-supplier-types' AND cg.is_deleted=false AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" ci WHERE ci.catalog_group_id=cg.id AND ci.code='SEA' AND ci.is_deleted=false);
INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'AIR','air','Transporte aéreo','Categoría de proveedor SRM','Transporte aéreo','{}'::jsonb,30,true,true,now(),'migration',false FROM config."CatalogGroups" cg WHERE cg.slug='srm-supplier-types' AND cg.is_deleted=false AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" ci WHERE ci.catalog_group_id=cg.id AND ci.code='AIR' AND ci.is_deleted=false);
INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'CUSTOMS','customs','Servicios aduanales','Categoría de proveedor SRM','Servicios aduanales','{}'::jsonb,40,true,true,now(),'migration',false FROM config."CatalogGroups" cg WHERE cg.slug='srm-supplier-types' AND cg.is_deleted=false AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" ci WHERE ci.catalog_group_id=cg.id AND ci.code='CUSTOMS' AND ci.is_deleted=false);
INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'WAREHOUSE','warehouse','Almacenaje','Categoría de proveedor SRM','Almacenaje','{}'::jsonb,50,true,true,now(),'migration',false FROM config."CatalogGroups" cg WHERE cg.slug='srm-supplier-types' AND cg.is_deleted=false AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" ci WHERE ci.catalog_group_id=cg.id AND ci.code='WAREHOUSE' AND ci.is_deleted=false);
INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'COURIER','courier','Mensajería','Categoría de proveedor SRM','Mensajería','{}'::jsonb,60,true,true,now(),'migration',false FROM config."CatalogGroups" cg WHERE cg.slug='srm-supplier-types' AND cg.is_deleted=false AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" ci WHERE ci.catalog_group_id=cg.id AND ci.code='COURIER' AND ci.is_deleted=false);
INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'INSURANCE','insurance','Seguros','Categoría de proveedor SRM','Seguros','{}'::jsonb,70,true,true,now(),'migration',false FROM config."CatalogGroups" cg WHERE cg.slug='srm-supplier-types' AND cg.is_deleted=false AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" ci WHERE ci.catalog_group_id=cg.id AND ci.code='INSURANCE' AND ci.is_deleted=false);
INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'OTHER','other','Otros servicios','Categoría de proveedor SRM','Otros servicios','{}'::jsonb,80,true,true,now(),'migration',false FROM config."CatalogGroups" cg WHERE cg.slug='srm-supplier-types' AND cg.is_deleted=false AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" ci WHERE ci.catalog_group_id=cg.id AND ci.code='OTHER' AND ci.is_deleted=false);
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Data preservation: suppliers and their classifications must never be dropped on rollback.
    }
}
