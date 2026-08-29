using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260829185500_EnsureEmployeeDirectoryCatalog")]
public sealed class EnsureEmployeeDirectoryCatalog : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            INSERT INTO config."CatalogGroups"
                (id, code, slug, name, description, metadata_json, is_system, is_active, created_at_utc, created_by, is_deleted)
            VALUES
                ('c2920000-0000-4000-8000-000000000001'::uuid,
                 'INTERNAL_DIRECTORY',
                 'internal-directory',
                 'Directorio interno',
                 'Directorio interno de empleados, departamentos, extensiones, correos y celulares.',
                 '{"directory":true,"metadataFields":["department","extension","email","mobile"],"source":"Extensiones Castro Fallas 2026.xlsx"}'::jsonb,
                 FALSE, TRUE, NOW(), 'migration', FALSE)
            ON CONFLICT DO NOTHING;

            UPDATE config."CatalogGroups"
            SET code = 'INTERNAL_DIRECTORY',
                name = 'Directorio interno',
                description = 'Directorio interno de empleados, departamentos, extensiones, correos y celulares.',
                metadata_json = COALESCE(metadata_json, '{}'::jsonb)
                    || '{"directory":true,"metadataFields":["department","extension","email","mobile"],"source":"Extensiones Castro Fallas 2026.xlsx"}'::jsonb,
                is_system = FALSE,
                is_active = TRUE,
                is_deleted = FALSE,
                deleted_at_utc = NULL,
                deleted_by = NULL,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            WHERE slug = 'internal-directory';

            WITH desired(code, slug, name, extension, department, email, mobile, sort_order) AS (
                VALUES
                    ('EMP_001','carl-jensen','Carl Jensen','121','Jefatura','cjenseng@castrofallas.com',NULL,10),
                    ('EMP_002','cesar-mesen','César Mesén','120','Jefatura','cemesen@castrofallas.com',NULL,20),
                    ('EMP_003','jaime-pacheco','Jaime Pacheco','103','Servicio Al Cliente','facturacion1@castrofallas.com','+506 8379-5350',30),
                    ('EMP_004','nicole-ortega','Nicole Ortega','100','Servicio Al Cliente','facturacion2@castrofallas.com','+506 6290-4592',40),
                    ('EMP_005','fabian-villalobos','Fabián Villalobos','141','Servicio Al Cliente','fvillalobos@castrofallas.com','+506 6290-4508',50),
                    ('EMP_006','diego-najera','Diego Nájera','143','Servicio Al Cliente','servicioalcliente@castrofallas.com','+506 7006-3878',60),
                    ('EMP_007','kevin-vega','Kevin Vega','129','Servicio Al Cliente','servicioalcliente@castrofallas.com','+506 7006-3878',70),
                    ('EMP_008','keybel-herrera','Keybel Herrera','130','Servicio Al Cliente','traficocr@castrofallas.com',NULL,80),
                    ('EMP_009','royner-sibaja','Royner Sibaja','150','Consolidadora','rsibaja@castrofallas.com','+506 7078-6941',90),
                    ('EMP_010','josue-alvarado','Josué Alvarado','134','Consolidadora','logistica2@castrofallas.com','+506 6119-6970',100),
                    ('EMP_011','karen-navarro','Karen Navarro','119','Consolidadora','logistica4@castrofallas.com','+506 6354-3767',110),
                    ('EMP_012','keilor-castro','Keilor Castro','122','Consolidadora','logistica10@castrofallas.com','+506 6354-1392',120),
                    ('EMP_013','daniela-gutierrez','Daniela Gutiérrez','122','Consolidadora','logistica1@castrofallas.com','+506 6354-1392',130),
                    ('EMP_014','abraham-zuniga','Abraham Zúñiga','110','Consolidadora','documentacion2@castrofallas.com','+506 6354-3767',140),
                    ('EMP_015','fernanda-solis','Fernanda Solís','110','Consolidadora','documentacion@castrofallas.com','+506 6354-3767',150),
                    ('EMP_016','jean-paul-diaz','Jean Paul Díaz','151','Consolidadora','logistica6@castrofallas.com','+506 7007-3334',160),
                    ('EMP_017','maurice-lang','Maurice Lang','135','Desarrollo','mlang@castrofallas.com','+506 7066-3560',170),
                    ('EMP_018','andrea-monge','Andrea Monge','113','Consolidadora','amonge@castrofallas.com','+506 6078-2407',180),
                    ('EMP_019','randy-salazar','Randy Salazar','113','Consolidadora','logistica8@castrofallas.com','+506 6354-9702',190),
                    ('EMP_020','alejandro-sandi','Alejandro Sandí','124','Consolidadora','logistica7@castrofallas.com','+506 6354-9702',200),
                    ('EMP_021','laura-prado','Laura Prado','124','Consolidadora','logistica5@castrofallas.com','+506 6360-9869',210),
                    ('EMP_022','sonia-quiros','Sonia Quirós','104','Consolidadora','squiros@castrofallas.com','+506 6283-8475',220),
                    ('EMP_023','allison-perez','Allison Pérez','104','Consolidadora','estados@castrofallas.com','+506 6462-1330',230),
                    ('EMP_024','marco-artavia','Marco Artavia','135','Pricing','marco@castrofallas.com','+506 7256-5044',240),
                    ('EMP_025','sebastian-jensen','Sebastian Jensen','107','Pricing','pricing2@castrofallas.com','+506 6219-7370',250),
                    ('EMP_026','jose-pablo-mesen','José Pablo Mesén','145','Comercial','jmesen@castrofallas.com','+506 7116-5992 / +506 6111-5804',260),
                    ('EMP_027','stephanny-redondo','Stephanny Redondo','123','Comercial','sredondo@castrofallas.com','+506 6434-6756',270),
                    ('EMP_028','richard-soto','Richard Soto','142','Comercial','rsoto@castrofallas.com','+506 7078-6893',280),
                    ('EMP_029','yirley-tellez','Yirley Tellez','140','Comercial','ytellez@castrofallas.com','+506 6177-3414',290),
                    ('EMP_030','roberto-chaves','Roberto Chaves','137','Comercial','rchaves@castrofallas.com','+506 7005-1261',300),
                    ('EMP_031','valeria-obando','Valeria Obando','142','Comercial','vobando@castrofallas.com','+506 6177-3414',310),
                    ('EMP_032','carolina-quiros','Carolina Quirós','133','Comercial','cquiros@castrofallas.com','+506 6049-4152',320),
                    ('EMP_033','estefany-munguia','Estefany Munguía','145','Telemercadeo','mercadeo@castrofallas.com','+506 7078-6860',330),
                    ('EMP_034','hannia-villalobos','Hannia Villalobos','139','Pedimentación','hvillalobos@castrofallas.com',NULL,340),
                    ('EMP_035','milton-barrantes','Milton Barrantes','132','Pedimentación','mbarrantes@castrofallas.com',NULL,350),
                    ('EMP_036','jose-guzman','Jose Guzmán','116','Pedimentación','jguzman@castrofallas.com',NULL,360),
                    ('EMP_037','jimena-ceciliano','Jimena Ceciliano','106','Pedimentación','digitacion2@castrofallas.com',NULL,370),
                    ('EMP_038','santiago-hurtecho','Santiago Hurtecho','128','Pedimentación','hurtecho@castrofallas.com',NULL,380),
                    ('EMP_039','fernando-madrigal','Fernando Madrigal','138','Pedimentación','fmadrigal@castrofallas.com',NULL,390),
                    ('EMP_040','genesis-bejarano','Génesis Bejarano','125','Pedimentación','digitacion@castrofallas.com',NULL,400),
                    ('EMP_041','oscar-chavarria','Óscar Chavarría','114','Contabilidad','ochavarria@castrofallas.com',NULL,410),
                    ('EMP_042','jose-calderon','José Calderón','115','Contabilidad','contabilidad3@castrofallas.com',NULL,420),
                    ('EMP_043','pablo-porras','Pablo Porras','112','Contabilidad','contabilidad2@castrofallas.com',NULL,430),
                    ('EMP_044','melissa-aguilar','Melissa Aguilar','118','Contabilidad','contabilidad1@castrofallas.com',NULL,440),
                    ('EMP_045','jason-vargas','Jason Vargas','118','Contabilidad','contabilidad4@castrofallas.com',NULL,450),
                    ('EMP_046','teresa-perez','Teresa Pérez','115','Contabilidad','estados1@castrofallas.com','+506 6320-5156',460),
                    ('EMP_047','anyuri-mora','Anyuri Mora','131','Cargalotodo','logistica@cargalotodousa.com',NULL,470),
                    ('EMP_048','jorge-chavarria','Jorge Chavarría','127','Cargalotodo','operaciones@cargalotodousa.com',NULL,480),
                    ('EMP_049','henry-chavez','Henry Chávez','109','Cargalotodo','despachos@cargalotodousa.com',NULL,490),
                    ('EMP_050','hannielka-mejia','Hannielka Mejía','136','Cargalotodo','asesorias@cargalotodousa.com',NULL,500)
            )
            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT
                gen_random_uuid(), g.id, d.code, d.slug, d.name, d.department, d.extension,
                jsonb_build_object('department', d.department, 'extension', d.extension, 'email', d.email, 'mobile', d.mobile),
                d.sort_order, FALSE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" g
            CROSS JOIN desired d
            WHERE g.slug = 'internal-directory'
              AND g.is_deleted = FALSE
              AND NOT EXISTS (
                  SELECT 1 FROM config."CatalogItems" i
                  WHERE i.catalog_group_id = g.id
                    AND i.is_deleted = FALSE
                    AND (LOWER(i.slug) = LOWER(d.slug) OR LOWER(i.name) = LOWER(d.name))
              );
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DELETE FROM config.\"CatalogGroups\" WHERE slug = 'internal-directory';");
    }
}
