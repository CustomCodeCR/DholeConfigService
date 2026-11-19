using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Dhole.Config.Persistence.Migrations;

/// <summary>Config-managed acquisition sources, non-destructive and idempotent.</summary>
[DbContext(typeof(ServiceDbContext))]
[Migration("20261118235500_AddCrmLeadSources")]
public sealed class AddCrmLeadSources : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
WITH desired(code,slug,name,description) AS (
    VALUES ('CRM_LEAD_SOURCES','crm-lead-sources','Fuentes de captación CRM',
            'Origen de captación de prospectos comerciales')
)
INSERT INTO config."CatalogGroups"
 (id,code,slug,name,description,metadata_json,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),d.code,d.slug,d.name,d.description,
       '{"module":"crm","editable":true}'::jsonb,true,true,now(),'migration',false
FROM desired d
WHERE NOT EXISTS (SELECT 1 FROM config."CatalogGroups" g
                  WHERE g.slug=d.slug AND g.is_deleted=false)
ON CONFLICT DO NOTHING;

WITH desired(code,slug,name,sort_order) AS (
    VALUES
      ('COLD_VISIT','cold-visit','Visita en frío',10),
      ('REFERRAL','referral','Referido',20),
      ('SOCIAL_MEDIA','social-media','Redes sociales',30),
      ('ZONE_MAPPING','zone-mapping','Mapeo por zona',40),
      ('PHONE_CALL','phone-call','Llamada',50),
      ('WEBSITE','website','Página web',60),
      ('EVENT','event','Evento o feria',70),
      ('OTHER','other','Otro',80)
)
INSERT INTO config."CatalogItems"
 (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,
  is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),g.id,d.code,d.slug,d.name,
       'Fuente comercial de prospectos CRM',d.code,
       '{"module":"crm","source":"migration"}'::jsonb,d.sort_order,
       true,true,now(),'migration',false
FROM desired d
JOIN config."CatalogGroups" g ON g.slug='crm-lead-sources' AND g.is_deleted=false
WHERE NOT EXISTS (SELECT 1 FROM config."CatalogItems" ci
                  WHERE ci.catalog_group_id=g.id AND upper(ci.code)=d.code
                    AND ci.is_deleted=false)
ON CONFLICT DO NOTHING;
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Retain historically referenced CRM acquisition codes.
    }
}
