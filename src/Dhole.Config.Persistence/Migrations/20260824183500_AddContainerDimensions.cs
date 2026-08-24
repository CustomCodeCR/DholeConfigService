using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260824183500_AddContainerDimensions")]
public sealed class AddContainerDimensions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            INSERT INTO config."CatalogGroups"
                (id, code, slug, name, description, metadata_json, is_system, is_active, created_at_utc, created_by, is_deleted)
            VALUES
                ('c1000000-0000-4000-8000-000000000001', 'CONTAINER_SIZES', 'container-sizes', 'Tamaños de contenedor', 'Tamaños nominales de equipos utilizados por Pricing y Data Extraction.', '{"dimension":"size"}'::jsonb, TRUE, TRUE, NOW(), 'migration', FALSE),
                ('c1000000-0000-4000-8000-000000000002', 'CONTAINER_KINDS', 'container-kinds', 'Tipos de contenedor', 'Tipo físico/comercial del equipo independiente del tamaño.', '{"dimension":"kind"}'::jsonb, TRUE, TRUE, NOW(), 'migration', FALSE),
                ('c1000000-0000-4000-8000-000000000003', 'CONTAINER_TYPES', 'container-types', 'Equipos de contenedor', 'Combinaciones canónicas de tamaño y tipo. Se conserva este slug por compatibilidad con Pricing.', '{"dimension":"equipment","compatibility":"legacy"}'::jsonb, TRUE, TRUE, NOW(), 'migration', FALSE)
            ON CONFLICT DO NOTHING;

            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT v.id, g.id, v.code, v.slug, v.name, v.description, v.value, v.metadata_json::jsonb, v.sort_order, TRUE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" g
            CROSS JOIN (VALUES
                ('c1100000-0000-4000-8000-000000000020'::uuid, '20', '20', '20 pies', 'Contenedor de 20 pies', '20', '{"feet":20}', 20),
                ('c1100000-0000-4000-8000-000000000040'::uuid, '40', '40', '40 pies', 'Contenedor de 40 pies', '40', '{"feet":40}', 40),
                ('c1100000-0000-4000-8000-000000000045'::uuid, '45', '45', '45 pies', 'Contenedor de 45 pies', '45', '{"feet":45}', 45),
                ('c1100000-0000-4000-8000-000000000048'::uuid, '48', '48', '48 pies', 'Contenedor de 48 pies', '48', '{"feet":48}', 48)
            ) AS v(id, code, slug, name, description, value, metadata_json, sort_order)
            WHERE g.slug = 'container-sizes'
            ON CONFLICT DO NOTHING;

            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT v.id, g.id, v.code, v.slug, v.name, v.description, v.value, v.metadata_json::jsonb, v.sort_order, TRUE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" g
            CROSS JOIN (VALUES
                ('c1200000-0000-4000-8000-000000000001'::uuid, 'DV', 'dry-van', 'Dry Van', 'Contenedor seco estándar', 'DV', '{"aliases":["DV","DRY","DRY VAN","GP","GENERAL PURPOSE","STD","STANDARD"]}', 10),
                ('c1200000-0000-4000-8000-000000000002'::uuid, 'HC', 'high-cube', 'High Cube', 'Contenedor de mayor altura', 'HC', '{"aliases":["HC","HQ","HIGH CUBE","HIGHCUBE"]}', 20),
                ('c1200000-0000-4000-8000-000000000003'::uuid, 'OT', 'open-top', 'Open Top', 'Contenedor con techo abierto', 'OT', '{"aliases":["OT","OPEN TOP","OPENTOP"]}', 30),
                ('c1200000-0000-4000-8000-000000000004'::uuid, 'OS', 'open-side', 'Open Side', 'Contenedor con apertura lateral', 'OS', '{"aliases":["OS","OPEN SIDE","OPENSIDE","SIDE OPEN"]}', 40),
                ('c1200000-0000-4000-8000-000000000005'::uuid, 'TK', 'tank', 'Tank', 'Contenedor tanque', 'TK', '{"aliases":["TK","TNK","TANK","ISO TANK","ISOTANK"]}', 50),
                ('c1200000-0000-4000-8000-000000000006'::uuid, 'FR', 'flat-rack', 'Flat Rack', 'Contenedor plataforma Flat Rack', 'FR', '{"aliases":["FR","FLAT RACK","FLATRACK"]}', 60),
                ('c1200000-0000-4000-8000-000000000007'::uuid, 'NOR', 'nor', 'NOR', 'Non Operating Reefer', 'NOR', '{"aliases":["NOR","NON OPERATING REEFER","NON-OPERATING REEFER","NOREFFER"]}', 70)
            ) AS v(id, code, slug, name, description, value, metadata_json, sort_order)
            WHERE g.slug = 'container-kinds'
            ON CONFLICT DO NOTHING;

            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT v.id, g.id, v.code, v.slug, v.name, v.description, v.value, v.metadata_json::jsonb, v.sort_order, TRUE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" g
            CROSS JOIN (VALUES
                ('c1300000-0000-4000-8000-000000000001'::uuid, '20DV', '20-dry-van', '20 Dry Van', '20 pies Dry Van', '20DV', '{"size":"20","kind":"dry-van","kindCode":"DV","aliases":["20","20DV","20DRY","20GP","20STD","20 STANDARD"]}', 10),
                ('c1300000-0000-4000-8000-000000000002'::uuid, '40DV', '40-dry-van', '40 Dry Van', '40 pies Dry Van', '40DV', '{"size":"40","kind":"dry-van","kindCode":"DV","aliases":["40","40DV","40DRY","40GP","40STD","40 STANDARD"]}', 20),
                ('c1300000-0000-4000-8000-000000000003'::uuid, '40HC', '40-high-cube', '40 High Cube', '40 pies High Cube', '40HC', '{"size":"40","kind":"high-cube","kindCode":"HC","aliases":["40HC","40HQ","40 HC","40 HQ","40 HIGH CUBE"]}', 30),
                ('c1300000-0000-4000-8000-000000000004'::uuid, '45HC', '45-high-cube', '45 High Cube', '45 pies High Cube', '45HC', '{"size":"45","kind":"high-cube","kindCode":"HC","aliases":["45HC","45HQ","45 HC","45 HQ","45 HIGH CUBE"]}', 40),
                ('c1300000-0000-4000-8000-000000000005'::uuid, '48HC', '48-high-cube', '48 High Cube', '48 pies High Cube', '48HC', '{"size":"48","kind":"high-cube","kindCode":"HC","aliases":["48HC","48HQ","48 HC","48 HQ","48 HIGH CUBE"]}', 50),
                ('c1300000-0000-4000-8000-000000000006'::uuid, '20OT', '20-open-top', '20 Open Top', '20 pies Open Top', '20OT', '{"size":"20","kind":"open-top","kindCode":"OT","aliases":["20OT","20 OT","20 OPEN TOP"]}', 60),
                ('c1300000-0000-4000-8000-000000000007'::uuid, '40OT', '40-open-top', '40 Open Top', '40 pies Open Top', '40OT', '{"size":"40","kind":"open-top","kindCode":"OT","aliases":["40OT","40 OT","40 OPEN TOP"]}', 70),
                ('c1300000-0000-4000-8000-000000000008'::uuid, '20OS', '20-open-side', '20 Open Side', '20 pies Open Side', '20OS', '{"size":"20","kind":"open-side","kindCode":"OS","aliases":["20OS","20 OS","20 OPEN SIDE"]}', 80),
                ('c1300000-0000-4000-8000-000000000009'::uuid, '40OS', '40-open-side', '40 Open Side', '40 pies Open Side', '40OS', '{"size":"40","kind":"open-side","kindCode":"OS","aliases":["40OS","40 OS","40 OPEN SIDE"]}', 90),
                ('c1300000-0000-4000-8000-000000000010'::uuid, '20TK', '20-tank', '20 Tank', '20 pies Tank', '20TK', '{"size":"20","kind":"tank","kindCode":"TK","aliases":["20TK","20TNK","20 TANK","20 ISO TANK"]}', 100),
                ('c1300000-0000-4000-8000-000000000011'::uuid, '40TK', '40-tank', '40 Tank', '40 pies Tank', '40TK', '{"size":"40","kind":"tank","kindCode":"TK","aliases":["40TK","40TNK","40 TANK","40 ISO TANK"]}', 110),
                ('c1300000-0000-4000-8000-000000000012'::uuid, '20FR', '20-flat-rack', '20 Flat Rack', '20 pies Flat Rack', '20FR', '{"size":"20","kind":"flat-rack","kindCode":"FR","aliases":["20FR","20 FR","20 FLAT RACK"]}', 120),
                ('c1300000-0000-4000-8000-000000000013'::uuid, '40FR', '40-flat-rack', '40 Flat Rack', '40 pies Flat Rack', '40FR', '{"size":"40","kind":"flat-rack","kindCode":"FR","aliases":["40FR","40 FR","40 FLAT RACK"]}', 130),
                ('c1300000-0000-4000-8000-000000000014'::uuid, '20NOR', '20-nor', '20 NOR', '20 pies Non Operating Reefer', '20NOR', '{"size":"20","kind":"nor","kindCode":"NOR","aliases":["20NOR","20 NOR","20 NON OPERATING REEFER"]}', 140),
                ('c1300000-0000-4000-8000-000000000015'::uuid, '40NOR', '40-nor', '40 NOR', '40 pies Non Operating Reefer', '40NOR', '{"size":"40","kind":"nor","kindCode":"NOR","aliases":["40NOR","40 NOR","40 NON OPERATING REEFER"]}', 150)
            ) AS v(id, code, slug, name, description, value, metadata_json, sort_order)
            WHERE g.slug = 'container-types'
            ON CONFLICT DO NOTHING;
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DELETE FROM config."CatalogItems"
            WHERE id::text LIKE 'c13%' OR id::text LIKE 'c12%' OR id::text LIKE 'c11%';

            DELETE FROM config."CatalogGroups"
            WHERE slug IN ('container-sizes', 'container-kinds')
              AND created_by = 'migration';
            """
        );
    }
}
