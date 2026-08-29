using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260829182500_AddEmployeeDirectoryCatalog")]
public sealed class AddEmployeeDirectoryCatalog : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            INSERT INTO config."CatalogGroups"
                (id, code, slug, name, description, metadata_json, is_system, is_active, created_at_utc, created_by, is_deleted)
            VALUES
                (
                    'c2900000-0000-4000-8000-000000000001'::uuid,
                    'EMPLOYEE_DIRECTORY',
                    'employee-directory',
                    'Directorio de extensiones',
                    'Directorio interno de empleados, departamentos, extensiones, correos y celulares.',
                    '{"directory":true,"metadataFields":["department","extension","email","mobile"],"source":"Extensiones Castro Fallas 2026"}'::jsonb,
                    TRUE,
                    TRUE,
                    NOW(),
                    'migration',
                    FALSE
                )
            ON CONFLICT DO NOTHING;

            UPDATE config."CatalogGroups"
            SET code = 'EMPLOYEE_DIRECTORY',
                name = 'Directorio de extensiones',
                description = 'Directorio interno de empleados, departamentos, extensiones, correos y celulares.',
                metadata_json = COALESCE(metadata_json, '{}'::jsonb)
                    || '{"directory":true,"metadataFields":["department","extension","email","mobile"],"source":"Extensiones Castro Fallas 2026"}'::jsonb,
                is_system = TRUE,
                is_active = TRUE,
                is_deleted = FALSE,
                deleted_at_utc = NULL,
                deleted_by = NULL,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            WHERE slug = 'employee-directory';

            WITH desired(id, code, slug, name, value, metadata_json, sort_order) AS (
                VALUES
                    ('c2910000-0000-4000-8000-000000000001'::uuid, 'EMP_001', 'carl-jensen', 'Carl Jensen', '121', '{"department":"Jefatura","extension":"121","email":"cjenseng@castrofallas.com","mobile":null}', 10),
                    ('c2910000-0000-4000-8000-000000000002'::uuid, 'EMP_002', 'cesar-mesen', 'César Mesén', '120', '{"department":"Jefatura","extension":"120","email":"cemesen@castrofallas.com","mobile":null}', 20),
                    ('c2910000-0000-4000-8000-000000000003'::uuid, 'EMP_003', 'jaime-pacheco', 'Jaime Pacheco', '103', '{"department":"Servicio Al Cliente","extension":"103","email":"facturacion1@castrofallas.com","mobile":"+506 8379-5350"}', 30),
                    ('c2910000-0000-4000-8000-000000000004'::uuid, 'EMP_004', 'nicole-ortega', 'Nicole Ortega', '100', '{"department":"Servicio Al Cliente","extension":"100","email":"facturacion2@castrofallas.com","mobile":"+506 6290-4592"}', 40),
                    ('c2910000-0000-4000-8000-000000000005'::uuid, 'EMP_005', 'fabian-villalobos', 'Fabián Villalobos', '141', '{"department":"Servicio Al Cliente","extension":"141","email":"fvillalobos@castrofallas.com","mobile":"+506 6290-4508"}', 50),
                    ('c2910000-0000-4000-8000-000000000006'::uuid, 'EMP_006', 'diego-najera', 'Diego Nájera', '143', '{"department":"Servicio Al Cliente","extension":"143","email":"servicioalcliente@castrofallas.com","mobile":"+506 7006-3878"}', 60),
                    ('c2910000-0000-4000-8000-000000000007'::uuid, 'EMP_007', 'kevin-vega', 'Kevin Vega', '129', '{"department":"Servicio Al Cliente","extension":"129","email":"servicioalcliente@castrofallas.com","mobile":"+506 7006-3878"}', 70),
                    ('c2910000-0000-4000-8000-000000000008'::uuid, 'EMP_008', 'keybel-herrera', 'Keybel Herrera', '130', '{"department":"Servicio Al Cliente","extension":"130","email":"traficocr@castrofallas.com","mobile":null}', 80),
                    ('c2910000-0000-4000-8000-000000000009'::uuid, 'EMP_009', 'royner-sibaja', 'Royner Sibaja', '150', '{"department":"Consolidadora","extension":"150","email":"rsibaja@castrofallas.com","mobile":"+506 7078-6941"}', 90),
                    ('c2910000-0000-4000-8000-000000000010'::uuid, 'EMP_010', 'josue-alvarado', 'Josué Alvarado', '134', '{"department":"Consolidadora","extension":"134","email":"logistica2@castrofallas.com","mobile":"+506 6119-6970"}', 100),
                    ('c2910000-0000-4000-8000-000000000011'::uuid, 'EMP_011', 'karen-navarro', 'Karen Navarro', '119', '{"department":"Consolidadora","extension":"119","email":"logistica4@castrofallas.com","mobile":"+506 6354-3767"}', 110),
                    ('c2910000-0000-4000-8000-000000000012'::uuid, 'EMP_012', 'keilor-castro', 'Keilor Castro', '122', '{"department":"Consolidadora","extension":"122","email":"logistica10@castrofallas.com","mobile":"+506 6354-1392"}', 120),
                    ('c2910000-0000-4000-8000-000000000013'::uuid, 'EMP_013', 'daniela-gutierrez', 'Daniela Gutiérrez', '122', '{"department":"Consolidadora","extension":"122","email":"logistica1@castrofallas.com","mobile":"+506 6354-1392"}', 130),
                    ('c2910000-0000-4000-8000-000000000014'::uuid, 'EMP_014', 'abraham-zuniga', 'Abraham Zúñiga', '110', '{"department":"Consolidadora","extension":"110","email":"documentacion2@castrofallas.com","mobile":"+506 6354-3767"}', 140),
                    ('c2910000-0000-4000-8000-000000000015'::uuid, 'EMP_015', 'fernanda-solis', 'Fernanda Solís', '110', '{"department":"Consolidadora","extension":"110","email":"documentacion@castrofallas.com","mobile":"+506 6354-3767"}', 150),
                    ('c2910000-0000-4000-8000-000000000016'::uuid, 'EMP_016', 'jean-paul-diaz', 'Jean Paul Díaz', '151', '{"department":"Consolidadora","extension":"151","email":"logistica6@castrofallas.com","mobile":"+506 7007-3334"}', 160),
                    ('c2910000-0000-4000-8000-000000000017'::uuid, 'EMP_017', 'maurice-lang', 'Maurice Lang', '135', '{"department":"Desarrollo","extension":"135","email":"mlang@castrofallas.com","mobile":"+506 7066-3560"}', 170),
                    ('c2910000-0000-4000-8000-000000000018'::uuid, 'EMP_018', 'andrea-monge', 'Andrea Monge', '113', '{"department":"Consolidadora","extension":"113","email":"amonge@castrofallas.com","mobile":"+506 6078-2407"}', 180),
                    ('c2910000-0000-4000-8000-000000000019'::uuid, 'EMP_019', 'randy-salazar', 'Randy Salazar', '113', '{"department":"Consolidadora","extension":"113","email":"logistica8@castrofallas.com","mobile":"+506 6354-9702"}', 190),
                    ('c2910000-0000-4000-8000-000000000020'::uuid, 'EMP_020', 'alejandro-sandi', 'Alejandro Sandí', '124', '{"department":"Consolidadora","extension":"124","email":"logistica7@castrofallas.com","mobile":"+506 6354-9702"}', 200),
                    ('c2910000-0000-4000-8000-000000000021'::uuid, 'EMP_021', 'laura-prado', 'Laura Prado', '124', '{"department":"Consolidadora","extension":"124","email":"logistica5@castrofallas.com","mobile":"+506 6360-9869"}', 210),
                    ('c2910000-0000-4000-8000-000000000022'::uuid, 'EMP_022', 'sonia-quiros', 'Sonia Quirós', '104', '{"department":"Consolidadora","extension":"104","email":"squiros@castrofallas.com","mobile":"+506 6283-8475"}', 220),
                    ('c2910000-0000-4000-8000-000000000023'::uuid, 'EMP_023', 'allison-perez', 'Allison Pérez', '104', '{"department":"Consolidadora","extension":"104","email":"estados@castrofallas.com","mobile":"+506 6462-1330"}', 230),
                    ('c2910000-0000-4000-8000-000000000024'::uuid, 'EMP_024', 'marco-artavia', 'Marco Artavia', '135', '{"department":"Pricing","extension":"135","email":"marco@castrofallas.com","mobile":"+506 7256-5044"}', 240),
                    ('c2910000-0000-4000-8000-000000000025'::uuid, 'EMP_025', 'sebastian-jensen', 'Sebastian Jensen', '107', '{"department":"Pricing","extension":"107","email":"pricing2@castrofallas.com","mobile":"+506 6219-7370"}', 250),
                    ('c2910000-0000-4000-8000-000000000026'::uuid, 'EMP_026', 'jose-pablo-mesen', 'José Pablo Mesén', '145', '{"department":"Comercial","extension":"145","email":"jmesen@castrofallas.com","mobile":"+506 7116-5992 / +506 6111-5804"}', 260),
                    ('c2910000-0000-4000-8000-000000000027'::uuid, 'EMP_027', 'stephanny-redondo', 'Stephanny Redondo', '123', '{"department":"Comercial","extension":"123","email":"sredondo@castrofallas.com","mobile":"+506 6434-6756"}', 270),
                    ('c2910000-0000-4000-8000-000000000028'::uuid, 'EMP_028', 'richard-soto', 'Richard Soto', '142', '{"department":"Comercial","extension":"142","email":"rsoto@castrofallas.com","mobile":"+506 7078-6893"}', 280),
                    ('c2910000-0000-4000-8000-000000000029'::uuid, 'EMP_029', 'yirley-tellez', 'Yirley Tellez', '140', '{"department":"Comercial","extension":"140","email":"ytellez@castrofallas.com","mobile":"+506 6177-3414"}', 290),
                    ('c2910000-0000-4000-8000-000000000030'::uuid, 'EMP_030', 'roberto-chaves', 'Roberto Chaves', '137', '{"department":"Comercial","extension":"137","email":"rchaves@castrofallas.com","mobile":"+506 7005-1261"}', 300),
                    ('c2910000-0000-4000-8000-000000000031'::uuid, 'EMP_031', 'valeria-obando', 'Valeria Obando', '142', '{"department":"Comercial","extension":"142","email":"vobando@castrofallas.com","mobile":"+506 6177-3414"}', 310),
                    ('c2910000-0000-4000-8000-000000000032'::uuid, 'EMP_032', 'carolina-quiros', 'Carolina Quirós', '133', '{"department":"Comercial","extension":"133","email":"cquiros@castrofallas.com","mobile":"+506 6049-4152"}', 320),
                    ('c2910000-0000-4000-8000-000000000033'::uuid, 'EMP_033', 'estefany-munguia', 'Estefany Munguía', '145', '{"department":"Telemercadeo","extension":"145","email":"mercadeo@castrofallas.com","mobile":"+506 7078-6860"}', 330),
                    ('c2910000-0000-4000-8000-000000000034'::uuid, 'EMP_034', 'hannia-villalobos', 'Hannia Villalobos', '139', '{"department":"Pedimentación","extension":"139","email":"hvillalobos@castrofallas.com","mobile":null}', 340),
                    ('c2910000-0000-4000-8000-000000000035'::uuid, 'EMP_035', 'milton-barrantes', 'Milton Barrantes', '132', '{"department":"Pedimentación","extension":"132","email":"mbarrantes@castrofallas.com","mobile":null}', 350),
                    ('c2910000-0000-4000-8000-000000000036'::uuid, 'EMP_036', 'jose-guzman', 'Jose Guzmán', '116', '{"department":"Pedimentación","extension":"116","email":"jguzman@castrofallas.com","mobile":null}', 360),
                    ('c2910000-0000-4000-8000-000000000037'::uuid, 'EMP_037', 'jimena-ceciliano', 'Jimena Ceciliano', '106', '{"department":"Pedimentación","extension":"106","email":"digitacion2@castrofallas.com","mobile":null}', 370),
                    ('c2910000-0000-4000-8000-000000000038'::uuid, 'EMP_038', 'santiago-hurtecho', 'Santiago Hurtecho', '128', '{"department":"Pedimentación","extension":"128","email":"hurtecho@castrofallas.com","mobile":null}', 380),
                    ('c2910000-0000-4000-8000-000000000039'::uuid, 'EMP_039', 'fernando-madrigal', 'Fernando Madrigal', '138', '{"department":"Pedimentación","extension":"138","email":"fmadrigal@castrofallas.com","mobile":null}', 390),
                    ('c2910000-0000-4000-8000-000000000040'::uuid, 'EMP_040', 'genesis-bejarano', 'Génesis Bejarano', '125', '{"department":"Pedimentación","extension":"125","email":"digitacion@castrofallas.com","mobile":null}', 400),
                    ('c2910000-0000-4000-8000-000000000041'::uuid, 'EMP_041', 'oscar-chavarria', 'Óscar Chavarría', '114', '{"department":"Contabilidad","extension":"114","email":"ochavarria@castrofallas.com","mobile":null}', 410),
                    ('c2910000-0000-4000-8000-000000000042'::uuid, 'EMP_042', 'jose-calderon', 'José Calderón', '115', '{"department":"Contabilidad","extension":"115","email":"contabilidad3@castrofallas.com","mobile":null}', 420),
                    ('c2910000-0000-4000-8000-000000000043'::uuid, 'EMP_043', 'pablo-porras', 'Pablo Porras', '112', '{"department":"Contabilidad","extension":"112","email":"contabilidad2@castrofallas.com","mobile":null}', 430),
                    ('c2910000-0000-4000-8000-000000000044'::uuid, 'EMP_044', 'melissa-aguilar', 'Melissa Aguilar', '118', '{"department":"Contabilidad","extension":"118","email":"contabilidad1@castrofallas.com","mobile":null}', 440),
                    ('c2910000-0000-4000-8000-000000000045'::uuid, 'EMP_045', 'jason-vargas', 'Jason Vargas', '118', '{"department":"Contabilidad","extension":"118","email":"contabilidad4@castrofallas.com","mobile":null}', 450),
                    ('c2910000-0000-4000-8000-000000000046'::uuid, 'EMP_046', 'teresa-perez', 'Teresa Pérez', '115', '{"department":"Contabilidad","extension":"115","email":"estados1@castrofallas.com","mobile":"+506 6320-5156"}', 460),
                    ('c2910000-0000-4000-8000-000000000047'::uuid, 'EMP_047', 'anyuri-mora', 'Anyuri Mora', '131', '{"department":"Cargalotodo","extension":"131","email":"logistica@cargalotodousa.com","mobile":null}', 470),
                    ('c2910000-0000-4000-8000-000000000048'::uuid, 'EMP_048', 'jorge-chavarria', 'Jorge Chavarría', '127', '{"department":"Cargalotodo","extension":"127","email":"operaciones@cargalotodousa.com","mobile":null}', 480),
                    ('c2910000-0000-4000-8000-000000000049'::uuid, 'EMP_049', 'henry-chavez', 'Henry Chávez', '109', '{"department":"Cargalotodo","extension":"109","email":"despachos@cargalotodousa.com","mobile":null}', 490),
                    ('c2910000-0000-4000-8000-000000000050'::uuid, 'EMP_050', 'hannielka-mejia', 'Hannielka Mejía', '136', '{"department":"Cargalotodo","extension":"136","email":"asesorias@cargalotodousa.com","mobile":null}', 500)
            )
            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT
                d.id,
                g.id,
                d.code,
                d.slug,
                d.name,
                d.metadata_json::jsonb ->> 'department',
                d.value,
                d.metadata_json::jsonb,
                d.sort_order,
                FALSE,
                TRUE,
                NOW(),
                'migration',
                FALSE
            FROM config."CatalogGroups" g
            CROSS JOIN desired d
            WHERE g.slug = 'employee-directory'
              AND g.is_deleted = FALSE
              AND NOT EXISTS (
                  SELECT 1
                  FROM config."CatalogItems" i
                  WHERE i.catalog_group_id = g.id
                    AND i.is_deleted = FALSE
                    AND (
                        UPPER(i.code) = UPPER(d.code)
                        OR LOWER(i.slug) = LOWER(d.slug)
                        OR LOWER(i.name) = LOWER(d.name)
                    )
              )
            ON CONFLICT DO NOTHING;

            UPDATE config."CatalogItems" i
            SET description = 'Desarrollo',
                metadata_json = COALESCE(i.metadata_json, '{}'::jsonb)
                    || '{"department":"Desarrollo","extension":"135","email":"mlang@castrofallas.com","mobile":"+506 7066-3560"}'::jsonb,
                value = '135',
                updated_at_utc = NOW(),
                updated_by = 'migration'
            FROM config."CatalogGroups" g
            WHERE i.catalog_group_id = g.id
              AND g.slug = 'employee-directory'
              AND LOWER(i.name) = 'maurice lang'
              AND i.is_deleted = FALSE;
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DELETE FROM config."CatalogGroups"
            WHERE slug = 'employee-directory';
            """
        );
    }
}
