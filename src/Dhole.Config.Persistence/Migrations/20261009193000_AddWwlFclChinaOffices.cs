using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20261009193000_AddWwlFclChinaOffices")]
public sealed class AddWwlFclChinaOffices : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            -- Contacts sourced from WWL SEA FREIGHT(LAT), received 2026-10-09.
            -- Existing China office photos and contacts are RS assets; do not copy them to WWL.
            UPDATE config."CatalogItems" AS item
            SET metadata_json = COALESCE(item.metadata_json, '{}'::jsonb)
                || '{"agentCodes":["RS"],"agentNames":["RS Logistics"]}'::jsonb,
                updated_at_utc = NOW(), updated_by = 'migration'
            FROM config."CatalogGroups" AS grp
            WHERE item.catalog_group_id = grp.id
              AND grp.slug = 'pricing-warehouses'
              AND item.is_deleted = FALSE
              AND item.is_active = TRUE
              AND (
                item.metadata_json->>'countryCode' = 'CN'
                OR UPPER(COALESCE(item.metadata_json->>'country', '')) = 'CHINA'
              )
              AND item.code NOT LIKE 'WHS_WWL_%'
              AND NOT item.metadata_json ? 'agentCodes';
            
            INSERT INTO config."CatalogItems"
            (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT gen_random_uuid(), grp.id, 'WHS_WWL_SHANGHAI', 'whs-wwl-shanghai-cn', 'Shanghai WWL, China',
                   'Oficina WWL para FCL', 'Shanghai, China', '{"address":"14F-16F Junjiang International Tower, No.228 Ning Guo Road, Yangpu District, Shanghai, China","city":"Shanghai","country":"China","countryCode":"CN","agentCode":"WWL","agentName":"Worldwide Logistics","agentCodes":["WWL"],"agentNames":["Worldwide Logistics"],"shipmentModes":["FCL"],"polValues":["Shanghai, China","Shanghai"],"contactDirectory":[{"name":"Mr. Perry Guan","role":"Customer Service","email":"ngb.perryguan@wwl.sg","phone":"13957841789","shipmentModes":["FCL"],"modalities":["FCL"],"routes":[],"isPrimary":true,"isActive":true}],"images":[],"source":"WWL SEA FREIGHT(LAT) contact list 2026-10-09"}'::jsonb,
                   1000, FALSE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" grp
            WHERE grp.slug = 'pricing-warehouses' AND grp.is_deleted = FALSE
            AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" existing WHERE existing.catalog_group_id = grp.id AND existing.code = 'WHS_WWL_SHANGHAI' AND existing.is_deleted = FALSE);
            
            INSERT INTO config."CatalogItems"
            (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT gen_random_uuid(), grp.id, 'WHS_WWL_NINGBO', 'whs-wwl-ningbo-cn', 'Ningbo WWL, China',
                   'Oficina WWL para FCL', 'Ningbo, China', '{"address":"14F-16F Junjiang International Tower, No.228 Ning Guo Road, Yangpu District, Shanghai, China","city":"Ningbo","country":"China","countryCode":"CN","agentCode":"WWL","agentName":"Worldwide Logistics","agentCodes":["WWL"],"agentNames":["Worldwide Logistics"],"shipmentModes":["FCL"],"polValues":["Ningbo, China","Ningbo"],"contactDirectory":[{"name":"Mr. Perry Guan","role":"Customer Service","email":"ngb.perryguan@wwl.sg","phone":"13957841789","shipmentModes":["FCL"],"modalities":["FCL"],"routes":[],"isPrimary":true,"isActive":true}],"images":[],"source":"WWL SEA FREIGHT(LAT) contact list 2026-10-09"}'::jsonb,
                   1010, FALSE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" grp
            WHERE grp.slug = 'pricing-warehouses' AND grp.is_deleted = FALSE
            AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" existing WHERE existing.catalog_group_id = grp.id AND existing.code = 'WHS_WWL_NINGBO' AND existing.is_deleted = FALSE);
            
            INSERT INTO config."CatalogItems"
            (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT gen_random_uuid(), grp.id, 'WHS_WWL_NANJING', 'whs-wwl-nanjing-cn', 'Nanjing WWL, China',
                   'Oficina WWL para FCL', 'Nanjing, China', '{"address":"14F-16F Junjiang International Tower, No.228 Ning Guo Road, Yangpu District, Shanghai, China","city":"Nanjing","country":"China","countryCode":"CN","agentCode":"WWL","agentName":"Worldwide Logistics","agentCodes":["WWL"],"agentNames":["Worldwide Logistics"],"shipmentModes":["FCL"],"polValues":["Nanjing, China","Nanjing"],"contactDirectory":[{"name":"Mr. Perry Guan","role":"Customer Service","email":"ngb.perryguan@wwl.sg","phone":"13957841789","shipmentModes":["FCL"],"modalities":["FCL"],"routes":[],"isPrimary":true,"isActive":true}],"images":[],"source":"WWL SEA FREIGHT(LAT) contact list 2026-10-09"}'::jsonb,
                   1020, FALSE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" grp
            WHERE grp.slug = 'pricing-warehouses' AND grp.is_deleted = FALSE
            AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" existing WHERE existing.catalog_group_id = grp.id AND existing.code = 'WHS_WWL_NANJING' AND existing.is_deleted = FALSE);
            
            INSERT INTO config."CatalogItems"
            (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT gen_random_uuid(), grp.id, 'WHS_WWL_QINGDAO', 'whs-wwl-qingdao-cn', 'Qingdao WWL, China',
                   'Oficina WWL para FCL', 'Qingdao, China', '{"address":"36F, Block A, China Resources Building, No.6 Shandong Road, Qingdao, China","city":"Qingdao","country":"China","countryCode":"CN","agentCode":"WWL","agentName":"Worldwide Logistics","agentCodes":["WWL"],"agentNames":["Worldwide Logistics"],"shipmentModes":["FCL"],"polValues":["Qingdao, China","Qingdao"],"contactDirectory":[{"name":"Ms. Mia Yuan","role":"Customer Service","email":"tao.miayuan@wwl.sg","phone":"13780600677","shipmentModes":["FCL"],"modalities":["FCL"],"routes":[],"isPrimary":true,"isActive":true}],"images":[],"source":"WWL SEA FREIGHT(LAT) contact list 2026-10-09"}'::jsonb,
                   1030, FALSE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" grp
            WHERE grp.slug = 'pricing-warehouses' AND grp.is_deleted = FALSE
            AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" existing WHERE existing.catalog_group_id = grp.id AND existing.code = 'WHS_WWL_QINGDAO' AND existing.is_deleted = FALSE);
            
            INSERT INTO config."CatalogItems"
            (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT gen_random_uuid(), grp.id, 'WHS_WWL_LIANYUNGANG', 'whs-wwl-lianyungang-cn', 'Lianyungang WWL, China',
                   'Oficina WWL para FCL', 'Lianyungang, China', '{"address":"36F, Block A, China Resources Building, No.6 Shandong Road, Qingdao, China","city":"Lianyungang","country":"China","countryCode":"CN","agentCode":"WWL","agentName":"Worldwide Logistics","agentCodes":["WWL"],"agentNames":["Worldwide Logistics"],"shipmentModes":["FCL"],"polValues":["Lianyungang, China","Lianyungang"],"contactDirectory":[{"name":"Ms. Mia Yuan","role":"Customer Service","email":"tao.miayuan@wwl.sg","phone":"13780600677","shipmentModes":["FCL"],"modalities":["FCL"],"routes":[],"isPrimary":true,"isActive":true}],"images":[],"source":"WWL SEA FREIGHT(LAT) contact list 2026-10-09"}'::jsonb,
                   1040, FALSE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" grp
            WHERE grp.slug = 'pricing-warehouses' AND grp.is_deleted = FALSE
            AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" existing WHERE existing.catalog_group_id = grp.id AND existing.code = 'WHS_WWL_LIANYUNGANG' AND existing.is_deleted = FALSE);
            
            INSERT INTO config."CatalogItems"
            (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT gen_random_uuid(), grp.id, 'WHS_WWL_TIANJIN', 'whs-wwl-tianjin-cn', 'Tianjin WWL, China',
                   'Oficina WWL para FCL', 'Tianjin, China', '{"address":"36F, Block A, China Resources Building, No.6 Shandong Road, Qingdao, China","city":"Tianjin","country":"China","countryCode":"CN","agentCode":"WWL","agentName":"Worldwide Logistics","agentCodes":["WWL"],"agentNames":["Worldwide Logistics"],"shipmentModes":["FCL"],"polValues":["Tianjin, China","Tianjin"],"contactDirectory":[{"name":"Ms. Mia Yuan","role":"Customer Service","email":"tao.miayuan@wwl.sg","phone":"13780600677","shipmentModes":["FCL"],"modalities":["FCL"],"routes":[],"isPrimary":true,"isActive":true}],"images":[],"source":"WWL SEA FREIGHT(LAT) contact list 2026-10-09"}'::jsonb,
                   1050, FALSE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" grp
            WHERE grp.slug = 'pricing-warehouses' AND grp.is_deleted = FALSE
            AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" existing WHERE existing.catalog_group_id = grp.id AND existing.code = 'WHS_WWL_TIANJIN' AND existing.is_deleted = FALSE);
            
            INSERT INTO config."CatalogItems"
            (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT gen_random_uuid(), grp.id, 'WHS_WWL_DALIAN', 'whs-wwl-dalian-cn', 'Dalian WWL, China',
                   'Oficina WWL para FCL', 'Dalian, China', '{"address":"36F, Block A, China Resources Building, No.6 Shandong Road, Qingdao, China","city":"Dalian","country":"China","countryCode":"CN","agentCode":"WWL","agentName":"Worldwide Logistics","agentCodes":["WWL"],"agentNames":["Worldwide Logistics"],"shipmentModes":["FCL"],"polValues":["Dalian, China","Dalian"],"contactDirectory":[{"name":"Ms. Mia Yuan","role":"Customer Service","email":"tao.miayuan@wwl.sg","phone":"13780600677","shipmentModes":["FCL"],"modalities":["FCL"],"routes":[],"isPrimary":true,"isActive":true}],"images":[],"source":"WWL SEA FREIGHT(LAT) contact list 2026-10-09"}'::jsonb,
                   1060, FALSE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" grp
            WHERE grp.slug = 'pricing-warehouses' AND grp.is_deleted = FALSE
            AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" existing WHERE existing.catalog_group_id = grp.id AND existing.code = 'WHS_WWL_DALIAN' AND existing.is_deleted = FALSE);
            
            INSERT INTO config."CatalogItems"
            (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT gen_random_uuid(), grp.id, 'WHS_WWL_XINGANG', 'whs-wwl-xingang-cn', 'Xingang WWL, China',
                   'Oficina WWL para FCL', 'Xingang, China', '{"address":"36F, Block A, China Resources Building, No.6 Shandong Road, Qingdao, China","city":"Xingang","country":"China","countryCode":"CN","agentCode":"WWL","agentName":"Worldwide Logistics","agentCodes":["WWL"],"agentNames":["Worldwide Logistics"],"shipmentModes":["FCL"],"polValues":["Xingang, China","Xingang"],"contactDirectory":[{"name":"Ms. Mia Yuan","role":"Customer Service","email":"tao.miayuan@wwl.sg","phone":"13780600677","shipmentModes":["FCL"],"modalities":["FCL"],"routes":[],"isPrimary":true,"isActive":true}],"images":[],"source":"WWL SEA FREIGHT(LAT) contact list 2026-10-09"}'::jsonb,
                   1070, FALSE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" grp
            WHERE grp.slug = 'pricing-warehouses' AND grp.is_deleted = FALSE
            AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" existing WHERE existing.catalog_group_id = grp.id AND existing.code = 'WHS_WWL_XINGANG' AND existing.is_deleted = FALSE);
            
            INSERT INTO config."CatalogItems"
            (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT gen_random_uuid(), grp.id, 'WHS_WWL_SHENZHEN', 'whs-wwl-shenzhen-cn', 'Shenzhen WWL, China',
                   'Oficina WWL para FCL', 'Shenzhen, China', '{"address":"Loft B 701-702, UpperHills, No.5001 Huanggang Rd, Futian District, Shenzhen, China","city":"Shenzhen","country":"China","countryCode":"CN","agentCode":"WWL","agentName":"Worldwide Logistics","agentCodes":["WWL"],"agentNames":["Worldwide Logistics"],"shipmentModes":["FCL"],"polValues":["Shenzhen, China","Shenzhen"],"contactDirectory":[{"name":"Ms. Kiki Li","role":"Customer Service","email":"kiki.li@wwl.sg","phone":"15170792313","shipmentModes":["FCL"],"modalities":["FCL"],"routes":[],"isPrimary":true,"isActive":true}],"images":[],"source":"WWL SEA FREIGHT(LAT) contact list 2026-10-09"}'::jsonb,
                   1080, FALSE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" grp
            WHERE grp.slug = 'pricing-warehouses' AND grp.is_deleted = FALSE
            AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" existing WHERE existing.catalog_group_id = grp.id AND existing.code = 'WHS_WWL_SHENZHEN' AND existing.is_deleted = FALSE);
            
            INSERT INTO config."CatalogItems"
            (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT gen_random_uuid(), grp.id, 'WHS_WWL_HONG_KONG', 'whs-wwl-hong-kong-cn', 'Hong Kong WWL, China',
                   'Oficina WWL para FCL', 'Hong Kong, China', '{"address":"Loft B 701-702, UpperHills, No.5001 Huanggang Rd, Futian District, Shenzhen, China","city":"Hong Kong","country":"China","countryCode":"CN","agentCode":"WWL","agentName":"Worldwide Logistics","agentCodes":["WWL"],"agentNames":["Worldwide Logistics"],"shipmentModes":["FCL"],"polValues":["Hong Kong, China","Hong Kong"],"contactDirectory":[{"name":"Ms. Kiki Li","role":"Customer Service","email":"kiki.li@wwl.sg","phone":"15170792313","shipmentModes":["FCL"],"modalities":["FCL"],"routes":[],"isPrimary":true,"isActive":true}],"images":[],"source":"WWL SEA FREIGHT(LAT) contact list 2026-10-09"}'::jsonb,
                   1090, FALSE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" grp
            WHERE grp.slug = 'pricing-warehouses' AND grp.is_deleted = FALSE
            AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" existing WHERE existing.catalog_group_id = grp.id AND existing.code = 'WHS_WWL_HONG_KONG' AND existing.is_deleted = FALSE);
            
            INSERT INTO config."CatalogItems"
            (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT gen_random_uuid(), grp.id, 'WHS_WWL_XIAMEN', 'whs-wwl-xiamen-cn', 'Xiamen WWL, China',
                   'Oficina WWL para FCL', 'Xiamen, China', '{"address":"414A Building No. 2, No.35-39 Xiangxing Four Road, Xiamen Free Trade Zone, China","city":"Xiamen","country":"China","countryCode":"CN","agentCode":"WWL","agentName":"Worldwide Logistics","agentCodes":["WWL"],"agentNames":["Worldwide Logistics"],"shipmentModes":["FCL"],"polValues":["Xiamen, China","Xiamen"],"contactDirectory":[{"name":"Ms. Cassie","role":"Customer Service","email":"xmn.cassie@wwl.sg","phone":"18750521309","shipmentModes":["FCL"],"modalities":["FCL"],"routes":[],"isPrimary":true,"isActive":true}],"images":[],"source":"WWL SEA FREIGHT(LAT) contact list 2026-10-09"}'::jsonb,
                   1100, FALSE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" grp
            WHERE grp.slug = 'pricing-warehouses' AND grp.is_deleted = FALSE
            AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" existing WHERE existing.catalog_group_id = grp.id AND existing.code = 'WHS_WWL_XIAMEN' AND existing.is_deleted = FALSE);
            
            INSERT INTO config."CatalogItems"
            (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT gen_random_uuid(), grp.id, 'WHS_WWL_FUZHOU', 'whs-wwl-fuzhou-cn', 'Fuzhou WWL, China',
                   'Oficina WWL para FCL', 'Fuzhou, China', '{"address":"Room 2401 Rongdu International Building, Wusi Road, Gulou District, Fuzhou, China","city":"Fuzhou","country":"China","countryCode":"CN","agentCode":"WWL","agentName":"Worldwide Logistics","agentCodes":["WWL"],"agentNames":["Worldwide Logistics"],"shipmentModes":["FCL"],"polValues":["Fuzhou, China","Fuzhou"],"contactDirectory":[{"name":"Ms. Kiki Li","role":"Customer Service","email":"kiki.li@wwl.sg","phone":"15170792313","shipmentModes":["FCL"],"modalities":["FCL"],"routes":[],"isPrimary":true,"isActive":true}],"images":[],"source":"WWL SEA FREIGHT(LAT) contact list 2026-10-09"}'::jsonb,
                   1110, FALSE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" grp
            WHERE grp.slug = 'pricing-warehouses' AND grp.is_deleted = FALSE
            AND NOT EXISTS (SELECT 1 FROM config."CatalogItems" existing WHERE existing.catalog_group_id = grp.id AND existing.code = 'WHS_WWL_FUZHOU' AND existing.is_deleted = FALSE);
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Preserve office records and contact changes in operational catalog data.
    }
}
