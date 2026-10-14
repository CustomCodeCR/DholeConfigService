using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20261014212000_AddCrmPhase3CommercialCatalogs")]
public sealed class AddCrmPhase3CommercialCatalogs : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
INSERT INTO config."CatalogGroups" (id,code,slug,name,description,metadata_json,is_system,is_active,created_at_utc,created_by,is_deleted)
VALUES (gen_random_uuid(),'CRM_ACTIVITY_TYPES','crm-activity-types','Tipos de actividad','Catálogo CRM Fase 3','{"crm":true,"phase":3}'::jsonb,true,true,now(),'migration',false)
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'VISIT','visit','Visita','Opción comercial CRM','Visita','{}'::jsonb,10,true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='crm-activity-types' AND cg.is_deleted=false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'CALL','call','Llamada','Opción comercial CRM','Llamada','{}'::jsonb,20,true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='crm-activity-types' AND cg.is_deleted=false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'EMAIL','email','Correo electrónico','Opción comercial CRM','Correo electrónico','{}'::jsonb,30,true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='crm-activity-types' AND cg.is_deleted=false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'MEETING','meeting','Reunión','Opción comercial CRM','Reunión','{}'::jsonb,40,true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='crm-activity-types' AND cg.is_deleted=false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'OTHER','other','Otra actividad','Opción comercial CRM','Otra actividad','{}'::jsonb,50,true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='crm-activity-types' AND cg.is_deleted=false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogGroups" (id,code,slug,name,description,metadata_json,is_system,is_active,created_at_utc,created_by,is_deleted)
VALUES (gen_random_uuid(),'CRM_ACTIVITY_OUTCOMES','crm-activity-outcomes','Resultados de actividad','Catálogo CRM Fase 3','{"crm":true,"phase":3}'::jsonb,true,true,now(),'migration',false)
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'COMPLETED','completed','Completada','Opción comercial CRM','Completada','{}'::jsonb,10,true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='crm-activity-outcomes' AND cg.is_deleted=false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'NO_ANSWER','no-answer','Sin respuesta','Opción comercial CRM','Sin respuesta','{}'::jsonb,20,true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='crm-activity-outcomes' AND cg.is_deleted=false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'RESCHEDULED','rescheduled','Reprogramada','Opción comercial CRM','Reprogramada','{}'::jsonb,30,true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='crm-activity-outcomes' AND cg.is_deleted=false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'FOLLOW_UP','follow-up','Requiere seguimiento','Opción comercial CRM','Requiere seguimiento','{}'::jsonb,40,true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='crm-activity-outcomes' AND cg.is_deleted=false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'OTHER','other','Otro resultado','Opción comercial CRM','Otro resultado','{}'::jsonb,50,true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='crm-activity-outcomes' AND cg.is_deleted=false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogGroups" (id,code,slug,name,description,metadata_json,is_system,is_active,created_at_utc,created_by,is_deleted)
VALUES (gen_random_uuid(),'CRM_MARKETS','crm-markets','Mercados configurables','Catálogo CRM Fase 3','{"crm":true,"phase":3}'::jsonb,true,true,now(),'migration',false)
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'OTHER','other','Otro mercado','Opción comercial CRM','Otro mercado','{}'::jsonb,10,true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='crm-markets' AND cg.is_deleted=false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogGroups" (id,code,slug,name,description,metadata_json,is_system,is_active,created_at_utc,created_by,is_deleted)
VALUES (gen_random_uuid(),'CRM_OPPORTUNITY_LOSS_REASONS','crm-opportunity-loss-reasons','Motivos de pérdida','Catálogo CRM Fase 3','{"crm":true,"phase":3}'::jsonb,true,true,now(),'migration',false)
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'PRICE','price','Precio','Opción comercial CRM','Precio','{}'::jsonb,10,true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='crm-opportunity-loss-reasons' AND cg.is_deleted=false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'TIMING','timing','Momento','Opción comercial CRM','Momento','{}'::jsonb,20,true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='crm-opportunity-loss-reasons' AND cg.is_deleted=false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'COMPETITOR','competitor','Competidor','Opción comercial CRM','Competidor','{}'::jsonb,30,true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='crm-opportunity-loss-reasons' AND cg.is_deleted=false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'NO_RESPONSE','no-response','Sin respuesta','Opción comercial CRM','Sin respuesta','{}'::jsonb,40,true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='crm-opportunity-loss-reasons' AND cg.is_deleted=false
ON CONFLICT DO NOTHING;

INSERT INTO config."CatalogItems" (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),cg.id,'OTHER','other','Otro motivo','Opción comercial CRM','Otro motivo','{}'::jsonb,50,true,true,now(),'migration',false
FROM config."CatalogGroups" cg WHERE cg.slug='crm-opportunity-loss-reasons' AND cg.is_deleted=false
ON CONFLICT DO NOTHING;
""");
    }
    protected override void Down(MigrationBuilder migrationBuilder) =>
        throw new NotSupportedException("Do not remove shared CRM commercial catalog data.");
}
