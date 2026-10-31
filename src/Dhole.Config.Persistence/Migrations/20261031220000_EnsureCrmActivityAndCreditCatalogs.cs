using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Dhole.Config.Persistence.Migrations;

/// <summary>
/// Ensures the exact catalog group and item codes required by Dhole CRM's
/// gRPC validation of activity types, outcomes and credit currencies.
/// Idempotent and non-destructive: existing catalog references keep their IDs.
/// </summary>
[DbContext(typeof(ServiceDbContext))]
[Migration("20261031220000_EnsureCrmActivityAndCreditCatalogs")]
public sealed class EnsureCrmActivityAndCreditCatalogs : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
WITH desired(code,slug,name,description) AS (
    VALUES
      ('CRM_ACTIVITY_TYPES','crm-activity-types','Tipos de actividad CRM','Tipos admitidos por la agenda comercial'),
      ('CRM_ACTIVITY_OUTCOMES','crm-activity-outcomes','Resultados de actividad CRM','Resultados para cerrar visitas, llamadas y reuniones'),
      ('CURRENCIES','currencies','Monedas','Códigos monetarios ISO para tarifas, CRM y crédito')
)
INSERT INTO config."CatalogGroups"
 (id,code,slug,name,description,metadata_json,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),d.code,d.slug,d.name,d.description,
       '{"module":"crm","editable":true}'::jsonb,true,true,now(),'migration',false
FROM desired d
WHERE NOT EXISTS (SELECT 1 FROM config."CatalogGroups" g
                  WHERE g.slug=d.slug AND g.is_deleted=false)
ON CONFLICT DO NOTHING;

WITH desired(group_slug,code,slug,name,sort_order) AS (
    VALUES
      ('crm-activity-types','VISIT','visit','Visita',10),
      ('crm-activity-types','CALL','call','Llamada',20),
      ('crm-activity-types','MEETING','meeting','Reunión',30),
      ('crm-activity-types','EMAIL','email','Correo',40),
      ('crm-activity-types','OTHER','other','Otra actividad',50),
      ('crm-activity-outcomes','COMPLETED','completed','Completado',10),
      ('crm-activity-outcomes','SUCCESSFUL','successful','Exitosa',20),
      ('crm-activity-outcomes','UNSUCCESSFUL','unsuccessful','Sin éxito',30),
      ('crm-activity-outcomes','NO_SHOW','no-show','No asistió',40),
      ('crm-activity-outcomes','OTHER','other','Otro resultado',50),
      ('currencies','USD','usd','Dólar estadounidense',10),
      ('currencies','CRC','crc','Colón costarricense',20)
)
INSERT INTO config."CatalogItems"
 (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,
  is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),g.id,d.code,d.slug,d.name,
       'Catálogo requerido por Dhole CRM',d.code,
       '{"module":"crm","source":"migration"}'::jsonb,d.sort_order,
       true,true,now(),'migration',false
FROM desired d JOIN config."CatalogGroups" g ON g.slug=d.group_slug AND g.is_deleted=false
WHERE NOT EXISTS (SELECT 1 FROM config."CatalogItems" ci
                  WHERE ci.catalog_group_id=g.id AND upper(ci.code)=d.code AND ci.is_deleted=false)
ON CONFLICT DO NOTHING;

-- Re-enable the required canonical codes if a previous configuration disabled them.
UPDATE config."CatalogGroups" g
SET is_active=true,updated_at_utc=now(),updated_by='migration'
WHERE g.slug IN ('crm-activity-types','crm-activity-outcomes','currencies')
  AND g.is_deleted=false AND g.is_active=false;
UPDATE config."CatalogItems" ci
SET is_active=true,updated_at_utc=now(),updated_by='migration'
FROM config."CatalogGroups" g
WHERE ci.catalog_group_id=g.id AND g.is_deleted=false AND ci.is_deleted=false
  AND ((g.slug='crm-activity-types' AND upper(ci.code) IN ('VISIT','CALL','MEETING','EMAIL','OTHER'))
    OR (g.slug='crm-activity-outcomes' AND upper(ci.code) IN ('COMPLETED','SUCCESSFUL','UNSUCCESSFUL','NO_SHOW','OTHER'))
    OR (g.slug='currencies' AND upper(ci.code) IN ('USD','CRC')))
  AND ci.is_active=false;
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Preserve CRM audit history, relationships and system reference IDs on rollback.
    }
}
