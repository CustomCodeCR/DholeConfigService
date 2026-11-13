using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Dhole.Config.Persistence.Migrations;

/// <summary>
/// Introduces commercial activity classifications without changing existing activity IDs.
/// Re-running SQL is safe and existing CRM records are preserved.
/// </summary>
[DbContext(typeof(ServiceDbContext))]
[Migration("20261112213000_AddCrmCommercialActivityTypes")]
public sealed class AddCrmCommercialActivityTypes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
WITH desired(code,slug,name,sort_order) AS (
    VALUES
      ('COMMERCIAL_FOLLOWUP','commercial-followup','Seguimiento comercial',60),
      ('WHATSAPP','whatsapp','WhatsApp',70),
      ('RECOMMENDATION','recommendation','Recomendación',80),
      ('OFFER','offer','Oferta',90)
)
INSERT INTO config."CatalogItems"
 (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,
  is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),g.id,d.code,d.slug,d.name,
       'Tipo de actividad comercial CRM',d.code,
       '{"module":"crm","source":"migration"}'::jsonb,d.sort_order,
       true,true,now(),'migration',false
FROM desired d
JOIN config."CatalogGroups" g ON g.slug='crm-activity-types' AND g.is_deleted=false
WHERE NOT EXISTS (SELECT 1 FROM config."CatalogItems" ci
                  WHERE ci.catalog_group_id=g.id AND upper(ci.code)=d.code
                    AND ci.is_deleted=false)
ON CONFLICT DO NOTHING;

UPDATE config."CatalogItems" ci
SET name='Llamada telefónica',updated_at_utc=now(),updated_by='migration'
FROM config."CatalogGroups" g
WHERE ci.catalog_group_id=g.id AND g.slug='crm-activity-types' AND g.is_deleted=false
  AND upper(ci.code)='CALL' AND ci.name='Llamada' AND ci.is_deleted=false;

UPDATE config."CatalogItems" ci
SET is_active=true,updated_at_utc=now(),updated_by='migration'
FROM config."CatalogGroups" g
WHERE ci.catalog_group_id=g.id AND g.slug='crm-activity-types' AND g.is_deleted=false
  AND upper(ci.code) IN ('COMMERCIAL_FOLLOWUP','WHATSAPP','RECOMMENDATION','OFFER')
  AND ci.is_deleted=false AND ci.is_active=false;
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Keep referenced catalog codes so historical activities remain readable.
    }
}
