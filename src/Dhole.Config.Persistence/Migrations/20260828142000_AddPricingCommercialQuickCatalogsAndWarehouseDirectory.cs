using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260828142000_AddPricingCommercialQuickCatalogsAndWarehouseDirectory")]
public sealed class AddPricingCommercialQuickCatalogsAndWarehouseDirectory : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            INSERT INTO config."CatalogGroups"
                (id, code, slug, name, description, metadata_json, is_system, is_active, created_at_utc, created_by, is_deleted)
            VALUES
                ('c2800000-0000-4000-8000-000000000001'::uuid, 'PRICING_SALES_EXECUTIVES', 'pricing-sales-executives', 'Ejecutivos comerciales (temporal)', 'Catálogo temporal de ejecutivos comerciales para Pricing. Se elimina cuando el módulo Comercial sea la fuente de verdad.', '{"pricingWorkflow":true,"temporary":true,"replaceWith":"commercial-module","metadataFields":["email","companyPhone","extension","mobile"]}'::jsonb, FALSE, TRUE, NOW(), 'migration', FALSE),
                ('c2800000-0000-4000-8000-000000000002'::uuid, 'PRICING_CLIENTS', 'pricing-clients', 'Clientes (temporal)', 'Catálogo temporal de clientes para Pricing. Se elimina cuando el módulo Comercial sea la fuente de verdad.', '{"pricingWorkflow":true,"temporary":true,"replaceWith":"commercial-module","metadataFields":["contactName","contactEmail","contactPhone","companyAddress","salesExecutiveId"]}'::jsonb, FALSE, TRUE, NOW(), 'migration', FALSE)
            ON CONFLICT DO NOTHING;

            UPDATE config."CatalogGroups"
            SET metadata_json = COALESCE(metadata_json, '{}'::jsonb) || '{"pricingWorkflow":true,"temporary":true,"replaceWith":"commercial-module"}'::jsonb,
                is_system = FALSE,
                is_active = TRUE,
                is_deleted = FALSE,
                deleted_at_utc = NULL,
                deleted_by = NULL,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            WHERE slug IN ('pricing-sales-executives', 'pricing-clients');

            UPDATE config."CatalogGroups"
            SET metadata_json = COALESCE(metadata_json, '{}'::jsonb) || '{"pricingWorkflow":true,"modality":"Global","locationCatalog":true,"metadataFields":["address","countryCode","schedule","contacts","email","phone","latitude","longitude"]}'::jsonb,
                is_system = FALSE,
                is_active = TRUE,
                is_deleted = FALSE,
                deleted_at_utc = NULL,
                deleted_by = NULL,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            WHERE slug = 'pricing-warehouses';

            WITH desired(id, code, slug, name, value, metadata_json, sort_order) AS (
                VALUES
                    ('c2810000-0000-4000-8000-000000000001'::uuid, 'WHS_XIAMEN', 'whs-xiamen-cn', 'Xiamen, China', 'Xiamen, China', '{"address":"Haoxin Logistics Park Fujian Quanzhou City, Jinjiang City Zhangjing","countryCode":"CN","schedule":"12:00 - 20:00","contacts":"Tiki Wang / Alejandro Sandi","email":"consol.sh@rslog.com / consolidado-china@grupocastrofallas.com"}', 10),
                    ('c2810000-0000-4000-8000-000000000002'::uuid, 'WHS_NINGBO', 'whs-ningbo-cn', 'Ningbo, China', 'Ningbo, China', '{"address":"No. 188, Citong Avenue, Ningbo","countryCode":"CN","schedule":"08:00 - 19:00","contacts":"Tiki Wang / Alejandro Sandi","email":"consol.sh@rslog.com / consolidado-china@grupocastrofallas.com"}', 20),
                    ('c2810000-0000-4000-8000-000000000003'::uuid, 'WHS_DALIAN', 'whs-dalian-cn', 'Dalian, China', 'Dalian, China', '{"address":"No. 6, West North Road, Ganjingzi District, Dalian City, Liaoning Province","countryCode":"CN","schedule":"09:00 - 18:00","contacts":"Tiki Wang / Alejandro Sandi","email":"consol.sh@rslog.com / consolidado-china@grupocastrofallas.com"}', 30),
                    ('c2810000-0000-4000-8000-000000000004'::uuid, 'WHS_QINGDAO', 'whs-qingdao-cn', 'Qingdao, China', 'Qingdao, China', '{"address":"Changqing Logistics Park (50 meters west of intersection of Shuangyuan Road and Hedong Road), North District Warehouse 2-3","countryCode":"CN","schedule":"08:00 - 19:00","contacts":"Tiki Wang / Alejandro Sandi","email":"consol.sh@rslog.com / consolidado-china@grupocastrofallas.com"}', 40),
                    ('c2810000-0000-4000-8000-000000000005'::uuid, 'WHS_XINGANG', 'whs-xingang-cn', 'Xingang, China', 'Xingang, China', '{"address":"Jinya Logistics Park No. 876 East Yangbei Road, Dongli District, Tianjin","countryCode":"CN","schedule":"08:00 - 19:00","contacts":"Tiki Wang / Alejandro Sandi","email":"consol.sh@rslog.com / consolidado-china@grupocastrofallas.com"}', 50),
                    ('c2810000-0000-4000-8000-000000000006'::uuid, 'WHS_SHENZHEN', 'whs-shenzhen-cn', 'Shenzhen, China', 'Shenzhen, China', '{"address":"No. 1-3, Building 6, District B, Jinpeng Logistics Park","countryCode":"CN","schedule":"09:00 - 22:00","contacts":"Tiki Wang / Alejandro Sandi","email":"consol.sh@rslog.com / consolidado-china@grupocastrofallas.com"}', 60),
                    ('c2810000-0000-4000-8000-000000000007'::uuid, 'WHS_GUANGZHOU', 'whs-guangzhou-cn', 'Guangzhou, China', 'Guangzhou, China', '{"address":"Gate 105-106, Building F1, Huabang Logistics Park, Taihe Town, Baiyun District, Guangzhou","countryCode":"CN","schedule":"08:00 - 22:00","contacts":"Tiki Wang / Alejandro Sandi","email":"consol.sh@rslog.com / consolidado-china@grupocastrofallas.com"}', 70),
                    ('c2810000-0000-4000-8000-000000000008'::uuid, 'WHS_FUZHOU', 'whs-fuzhou-cn', 'Fuzhou, China', 'Fuzhou, China', '{"address":"Haoxin Logistics Park Fujian Quanzhou City, Jinjiang City Zhangjing","countryCode":"CN","schedule":"12:00 - 20:00","contacts":"Tiki Wang / Alejandro Sandi","email":"consol.sh@rslog.com / consolidado-china@grupocastrofallas.com"}', 80),
                    ('c2810000-0000-4000-8000-000000000009'::uuid, 'WHS_SHANGHAI', 'whs-shanghai-cn', 'Shanghai, China', 'Shanghai, China', '{"address":"No.269 Tongfa Road, Pudong New Area, Shanghai, P.R.China","countryCode":"CN","schedule":"08:30 - 16:00","contacts":"Tiki Wang / Alejandro Sandi","email":"consol.sh@rslog.com / consolidado-china@grupocastrofallas.com"}', 90),
                    ('c2810000-0000-4000-8000-000000000010'::uuid, 'WHS_BARCELONA', 'whs-barcelona-es', 'Barcelona, España', 'Barcelona, España', '{"address":"C/ Illes Medes, Nave 1, 08192 Sant Quirze, Barcelona, Spain","countryCode":"ES","schedule":"08:00 - 16:00","contacts":"Julio Tomalá / Alejandro Sandi","email":"sea.export@bclgroup.net / espana@grupocastrofallas.com","phone":"+34 93 715 67 60"}', 100),
                    ('c2810000-0000-4000-8000-000000000011'::uuid, 'WHS_MIAMI', 'whs-miami-us', 'Miami, USA', 'Miami, USA', '{"address":"8501 NW 17 Street, Suite 101, Miami, FL 33126","countryCode":"US","schedule":"09:00 - 16:00","contacts":"Tomas Lobotrico / Randy Salazar","email":"tlobotrico@pluscargousa.com / estadosunidos@grupocastrofallas.com"}', 110),
                    ('c2810000-0000-4000-8000-000000000012'::uuid, 'WHS_NEW_YORK', 'whs-new-york-us', 'New York, USA', 'New York, USA', '{"address":"6801 West Side Avenue, North Bergen, NJ 07047","countryCode":"US","schedule":"09:00 - 16:00","contacts":"Tomas Lobotrico / Randy Salazar","email":"tlobotrico@pluscargousa.com / estadosunidos@grupocastrofallas.com"}', 120),
                    ('c2810000-0000-4000-8000-000000000013'::uuid, 'WHS_HOUSTON', 'whs-houston-us', 'Houston, USA', 'Houston, USA', '{"address":"2222 N. Wayside Dr., Houston, TX 77020","countryCode":"US","schedule":"09:00 - 16:00","contacts":"Tomas Lobotrico / Randy Salazar","email":"tlobotrico@pluscargousa.com / estadosunidos@grupocastrofallas.com"}', 130),
                    ('c2810000-0000-4000-8000-000000000014'::uuid, 'WHS_LOS_ANGELES', 'whs-los-angeles-us', 'Los Angeles, USA', 'Los Angeles, USA', '{"address":"19001 Harborgate Way, Torrance, CA 90501","countryCode":"US","schedule":"09:00 - 16:00","contacts":"Tomas Lobotrico / Randy Salazar","email":"tlobotrico@pluscargousa.com / estadosunidos@grupocastrofallas.com"}', 140),
                    ('c2810000-0000-4000-8000-000000000015'::uuid, 'WHS_HUENEME', 'whs-hueneme-us', 'Hueneme, USA', 'Hueneme, USA', '{"address":"710 N Del Norte Blvd, Oxnard, CA 93030","countryCode":"US","schedule":"09:00 - 16:00","contacts":"Tomas Lobotrico / Randy Salazar","email":"tlobotrico@pluscargousa.com / estadosunidos@grupocastrofallas.com"}', 150),
                    ('c2810000-0000-4000-8000-000000000016'::uuid, 'WHS_A257_CURRIDABAT', 'whs-a257-curridabat-cr', 'A257 Curridabat, Costa Rica', 'A257 Curridabat, Costa Rica', '{"address":"Frente a Café Volio, Barrio San José, Curridabat, Costa Rica","countryCode":"CR","schedule":"08:00 - 16:00","contacts":"Josue Alvarado / Andrea Monge","email":"costarica@grupocastrofallas.com"}', 160),
                    ('c2810000-0000-4000-8000-000000000017'::uuid, 'WHS_A287_TIBAS', 'whs-a287-tibas-cr', 'A287 Tibás, Costa Rica', 'A287 Tibás, Costa Rica', '{"address":"Detrás del Hotel El Edén, Tibás, Costa Rica","countryCode":"CR","schedule":"08:00 - 16:00","contacts":"Josue Alvarado / Andrea Monge","email":"costarica@grupocastrofallas.com"}', 170),
                    ('c2810000-0000-4000-8000-000000000018'::uuid, 'WHS_PANAMA', 'whs-panama-pa', 'Panamá, Panamá', 'Panamá, Panamá', '{"address":"France Field, Calle 7ma., Avenida 5ta., al lado de la Bodega de Anker, Panamá","countryCode":"PA","schedule":"08:00 - 16:00","contacts":"Josue Alvarado","email":"panama@grupocastrofallas.com"}', 180)
            )
            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT d.id, g.id, d.code, d.slug, d.name, 'WHS FCA global', d.value, d.metadata_json::jsonb, d.sort_order, FALSE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" g
            CROSS JOIN desired d
            WHERE g.slug = 'pricing-warehouses'
              AND g.is_deleted = FALSE
              AND NOT EXISTS (
                  SELECT 1 FROM config."CatalogItems" i
                  WHERE i.catalog_group_id = g.id
                    AND i.is_deleted = FALSE
                    AND (UPPER(i.code) = UPPER(d.code) OR LOWER(i.slug) = LOWER(d.slug) OR LOWER(i.name) = LOWER(d.name))
              )
            ON CONFLICT DO NOTHING;

            WITH desired(group_slug, id, code, slug, name, value, metadata_json, sort_order) AS (
                VALUES
                    ('pol', 'c2820000-0000-4000-8000-000000000001'::uuid, 'PLGDY', 'gdynia-poland-cy', '⚓ Gdynia, Polonia (CY)', 'Gdynia, Polonia (CY)', '{"countryCode":"PL","terminalType":"CY","locationType":"seaport","icon":"anchor"}', 980),
                    ('poe', 'c2820000-0000-4000-8000-000000000011'::uuid, 'PLGDY', 'gdynia-poland-cy', '⚓ Gdynia, Polonia (CY)', 'Gdynia, Polonia (CY)', '{"countryCode":"PL","terminalType":"CY","locationType":"seaport","icon":"anchor"}', 980),
                    ('pol', 'c2820000-0000-4000-8000-000000000002'::uuid, 'ITMIL', 'milan-italy-icd', '🚚 Milan, Italy (ICD / Inland Point)', 'Milan, Italy (ICD / Inland Point)', '{"countryCode":"IT","terminalType":"ICD","locationType":"inland","icon":"truck"}', 990),
                    ('poe', 'c2820000-0000-4000-8000-000000000012'::uuid, 'ITMIL', 'milan-italy-icd', '🚚 Milan, Italy (ICD / Inland Point)', 'Milan, Italy (ICD / Inland Point)', '{"countryCode":"IT","terminalType":"ICD","locationType":"inland","icon":"truck"}', 990)
            )
            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT d.id, g.id, d.code, d.slug, d.name, 'Ubicación agregada para Pricing.', d.value, d.metadata_json::jsonb, d.sort_order, FALSE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" g
            JOIN desired d ON d.group_slug = g.slug
            WHERE g.is_deleted = FALSE
              AND NOT EXISTS (
                  SELECT 1 FROM config."CatalogItems" i
                  WHERE i.catalog_group_id = g.id
                    AND i.is_deleted = FALSE
                    AND (UPPER(i.code) = UPPER(d.code) OR LOWER(i.slug) = LOWER(d.slug) OR LOWER(i.name) = LOWER(d.name))
              )
            ON CONFLICT DO NOTHING;
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DELETE FROM config."CatalogItems"
            WHERE code IN ('WHS_XIAMEN','WHS_NINGBO','WHS_DALIAN','WHS_QINGDAO','WHS_XINGANG','WHS_SHENZHEN','WHS_GUANGZHOU','WHS_FUZHOU','WHS_SHANGHAI','WHS_BARCELONA','WHS_MIAMI','WHS_NEW_YORK','WHS_HOUSTON','WHS_LOS_ANGELES','WHS_HUENEME','WHS_A257_CURRIDABAT','WHS_A287_TIBAS','WHS_PANAMA','PLGDY','ITMIL')
              AND created_by = 'migration';

            DELETE FROM config."CatalogItems"
            WHERE catalog_group_id IN (
                SELECT id FROM config."CatalogGroups" WHERE slug IN ('pricing-sales-executives','pricing-clients')
            );
            DELETE FROM config."CatalogGroups"
            WHERE slug IN ('pricing-sales-executives','pricing-clients');
            """
        );
    }
}
