using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260910170000_ConfigureRsLogisticsChinaFclMatrix")]
public sealed class ConfigureRsLogisticsChinaFclMatrix : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE config."CatalogGroups"
            SET metadata_json = COALESCE(metadata_json, '{}'::jsonb)
                || '{
                      "supportsFclContactMatrix":true,
                      "supportsAgentContactFallback":true,
                      "chinaFclOperationalContactSource":"pricing-warehouses",
                      "chinaFclDefaultAgent":"RS"
                    }'::jsonb,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            WHERE slug = 'agents'
              AND is_deleted = FALSE;

            UPDATE config."CatalogGroups"
            SET metadata_json = COALESCE(metadata_json, '{}'::jsonb)
                || '{
                      "supportsMultipleContacts":true,
                      "supportsRouteContacts":true,
                      "supportsModalityContacts":true,
                      "supportsPolResolution":true,
                      "supportsAgentContacts":true,
                      "fclOnlyChinaMatrix":true,
                      "allIncoterms":true,
                      "fcaRequiresWarehouse":true,
                      "chinaFclDefaultAgent":"RS",
                      "nonRsContactPolicy":"GCF_ONLY"
                    }'::jsonb,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            WHERE slug = 'pricing-warehouses'
              AND is_deleted = FALSE;

            -- RS remains compatible by code/value, but is explicitly the preferred China FCL agent.
            UPDATE config."CatalogItems" AS item
            SET name = 'RS Logistics',
                sort_order = 0,
                metadata_json = (COALESCE(item.metadata_json, '{}'::jsonb) - 'contactDirectory')
                    || '{
                          "country":"China",
                          "countryName":"China",
                          "countryCode":"CN",
                          "countryIso2":"CN",
                          "countryIso3":"CHN",
                          "originCountry":"China",
                          "originCountryName":"China",
                          "originCountryCode":"CN",
                          "countries":["China"],
                          "countryCodes":["CN"],
                          "canonicalName":"RS Logistics",
                          "aliases":["RS","RS Logistics"],
                          "preferredForChinaFcl":true,
                          "fclChinaContactPolicy":"RS_MATRIX_PLUS_GCF",
                          "contactDirectory":[
                            {
                              "name":"Sia Liu",
                              "role":"Latam Trade · RS Logistics Ltd. – Shanghai",
                              "phone":"+86 21 6069 5301 / +86 183 2172 9821",
                              "mobile":"+86 183 2172 9821",
                              "wechat":"8618321729821",
                              "email":"sialiu.sh@rslog.com",
                              "isPrimary":true,
                              "isActive":true,
                              "shipmentModes":["FCL"]
                            },
                            {
                              "name":"Grupo Castro Fallas",
                              "role":"Contacto GCF",
                              "email":"china@grupocastrofallas.com",
                              "phone":"",
                              "isPrimary":false,
                              "isActive":true,
                              "alwaysInclude":true,
                              "shipmentModes":["FCL"]
                            }
                          ],
                          "fclContactMatrix":{
                            "shipmentModes":["FCL"],
                            "allIncoterms":true,
                            "fcaRequiresWarehouse":true,
                            "alwaysCopy":["sialiu.sh@rslog.com","china@grupocastrofallas.com"],
                            "routingSource":"pricing-warehouses"
                          }
                        }'::jsonb,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            FROM config."CatalogGroups" AS catalog
            WHERE item.catalog_group_id = catalog.id
              AND catalog.slug = 'agents'
              AND catalog.is_deleted = FALSE
              AND item.is_deleted = FALSE
              AND (
                    regexp_replace(UPPER(COALESCE(item.code, '')), '[^A-Z0-9]+', '', 'g') IN ('RS', 'RSLOGISTICS')
                 OR regexp_replace(UPPER(COALESCE(item.name, '')), '[^A-Z0-9]+', '', 'g') IN ('RS', 'RSLOGISTICS')
                 OR regexp_replace(UPPER(COALESCE(item.value, '')), '[^A-Z0-9]+', '', 'g') IN ('RS', 'RSLOGISTICS')
                 OR regexp_replace(UPPER(COALESCE(item.slug, '')), '[^A-Z0-9]+', '', 'g') IN ('RS', 'RSLOGISTICS')
              );

            -- For any other China agent, the FCL contact card must expose only GCF.
            -- Existing agent contacts are retained in nonFclContactDirectory for reference/non-FCL use.
            UPDATE config."CatalogItems" AS item
            SET metadata_json = COALESCE(item.metadata_json, '{}'::jsonb)
                    || jsonb_build_object(
                        'nonFclContactDirectory', COALESCE(item.metadata_json->'contactDirectory', '[]'::jsonb),
                        'preferredForChinaFcl', false,
                        'fclChinaContactPolicy', 'GCF_ONLY',
                        'contactDirectory', '[
                          {
                            "name":"Grupo Castro Fallas",
                            "role":"Contacto GCF",
                            "email":"china@grupocastrofallas.com",
                            "phone":"",
                            "isPrimary":true,
                            "isActive":true,
                            "alwaysInclude":true,
                            "shipmentModes":["FCL"]
                          }
                        ]'::jsonb
                    ),
                sort_order = CASE WHEN item.sort_order < 100 THEN item.sort_order + 100 ELSE item.sort_order END,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            FROM config."CatalogGroups" AS catalog
            WHERE item.catalog_group_id = catalog.id
              AND catalog.slug = 'agents'
              AND catalog.is_deleted = FALSE
              AND item.is_deleted = FALSE
              AND (
                    UPPER(COALESCE(item.metadata_json->>'countryCode', '')) = 'CN'
                 OR UPPER(COALESCE(item.metadata_json->>'originCountryCode', '')) = 'CN'
                 OR LOWER(COALESCE(item.metadata_json->>'country', '')) = 'china'
                 OR LOWER(COALESCE(item.metadata_json->>'originCountry', '')) = 'china'
                 OR COALESCE(item.metadata_json->'countryCodes', '[]'::jsonb) ? 'CN'
                 OR COALESCE(item.metadata_json->'countries', '[]'::jsonb) ? 'China'
              )
              AND NOT (
                    regexp_replace(UPPER(COALESCE(item.code, '')), '[^A-Z0-9]+', '', 'g') IN ('RS', 'RSLOGISTICS')
                 OR regexp_replace(UPPER(COALESCE(item.name, '')), '[^A-Z0-9]+', '', 'g') IN ('RS', 'RSLOGISTICS')
                 OR regexp_replace(UPPER(COALESCE(item.value, '')), '[^A-Z0-9]+', '', 'g') IN ('RS', 'RSLOGISTICS')
                 OR regexp_replace(UPPER(COALESCE(item.slug, '')), '[^A-Z0-9]+', '', 'g') IN ('RS', 'RSLOGISTICS')
              );

            CREATE TEMP TABLE desired_rs_china_fcl_warehouses (
                id uuid NOT NULL,
                code text NOT NULL,
                slug text NOT NULL,
                name text NOT NULL,
                value text NOT NULL,
                pol_codes jsonb NOT NULL,
                address text NOT NULL,
                schedule text NOT NULL,
                contact_directory jsonb NOT NULL,
                sort_order integer NOT NULL
            ) ON COMMIT DROP;

            INSERT INTO desired_rs_china_fcl_warehouses
                (id, code, slug, name, value, pol_codes, address, schedule, contact_directory, sort_order)
            VALUES
                (
                    'c2910000-0000-4000-8000-000000000009'::uuid,
                    'WHS_SHANGHAI', 'whs-shanghai-cn', 'Shanghai, China', 'Shanghai, China',
                    '["SHANGHAI"]'::jsonb,
                    'No.269 Tongfa Road, Pudong New Area, Shanghai, P.R.China.',
                    'ALL DAY',
                    '[
                      {"name":"Eartha Yu","role":"Customer Service – Latam Trade · RS Logistics Ltd. – Shanghai","phone":"+86 21 6069 5300","email":"earthayu.sh@rslog.com","isPrimary":true,"isActive":true,"agentCodes":["RS","RS LOGISTICS"],"shipmentModes":["FCL"]},
                      {"name":"Sia Liu","role":"Latam Trade · RS Logistics Ltd. – Shanghai","phone":"+86 21 6069 5301 / +86 183 2172 9821","mobile":"+86 183 2172 9821","wechat":"8618321729821","email":"sialiu.sh@rslog.com","isPrimary":false,"isActive":true,"agentCodes":["RS","RS LOGISTICS"],"shipmentModes":["FCL"]},
                      {"name":"Grupo Castro Fallas","role":"Contacto GCF","phone":"","email":"china@grupocastrofallas.com","isPrimary":false,"isActive":true,"alwaysInclude":true,"shipmentModes":["FCL"]}
                    ]'::jsonb,
                    90
                ),
                (
                    'c2910000-0000-4000-8000-000000000004'::uuid,
                    'WHS_QINGDAO', 'whs-qingdao-cn', 'Qingdao, China', 'Qingdao, China',
                    '["QINGDAO"]'::jsonb,
                    'Changqing Logistics Park (50 meters west of intersection of Shuangyuan Road and Hedong Road) North District Warehouse 2-3',
                    '08:00 - 19:00',
                    '[
                      {"name":"Aimee Yuan","role":"Customer Service – Latam Trade · RS Logistics Ltd. – Qingdao","phone":"+86 532 5855 5299 / +86 152 5322 6230","mobile":"+86 152 5322 6230","email":"aimeeyuan.qd@rslog.com","isPrimary":true,"isActive":true,"agentCodes":["RS","RS LOGISTICS"],"shipmentModes":["FCL"],"routes":["QINGDAO -> CALDERA","QINGDAO -> HONDURAS","QINGDAO -> GUATEMALA","QINGDAO -> NICARAGUA","QINGDAO -> EL SALVADOR"]},
                      {"name":"Sia Liu","role":"Latam Trade · RS Logistics Ltd. – Shanghai","phone":"+86 21 6069 5301 / +86 183 2172 9821","mobile":"+86 183 2172 9821","wechat":"8618321729821","email":"sialiu.sh@rslog.com","isPrimary":false,"isActive":true,"agentCodes":["RS","RS LOGISTICS"],"shipmentModes":["FCL"],"routes":["QINGDAO -> CALDERA","QINGDAO -> HONDURAS","QINGDAO -> GUATEMALA","QINGDAO -> NICARAGUA","QINGDAO -> EL SALVADOR"]},
                      {"name":"Grupo Castro Fallas","role":"Contacto GCF","phone":"","email":"china@grupocastrofallas.com","isPrimary":false,"isActive":true,"alwaysInclude":true,"shipmentModes":["FCL"],"routes":["QINGDAO -> CALDERA","QINGDAO -> HONDURAS","QINGDAO -> GUATEMALA","QINGDAO -> NICARAGUA","QINGDAO -> EL SALVADOR"]},
                      {"name":"Selina Liu","role":"Customer Service – Latam Trade · RS Logistics Ltd. – Qingdao","phone":"+86 532 5855 5282 / +86 151 9202 1928","mobile":"+86 151 9202 1928","email":"selinaliu.qd@rslog.com","isPrimary":true,"isActive":true,"agentCodes":["RS","RS LOGISTICS"],"shipmentModes":["FCL"]},
                      {"name":"Sia Liu","role":"Latam Trade · RS Logistics Ltd. – Shanghai","phone":"+86 21 6069 5301 / +86 183 2172 9821","mobile":"+86 183 2172 9821","wechat":"8618321729821","email":"sialiu.sh@rslog.com","isPrimary":false,"isActive":true,"agentCodes":["RS","RS LOGISTICS"],"shipmentModes":["FCL"]},
                      {"name":"Grupo Castro Fallas","role":"Contacto GCF","phone":"","email":"china@grupocastrofallas.com","isPrimary":false,"isActive":true,"alwaysInclude":true,"shipmentModes":["FCL"]}
                    ]'::jsonb,
                    40
                ),
                (
                    'c2910000-0000-4000-8000-000000000001'::uuid,
                    'WHS_XIAMEN', 'whs-xiamen-cn', 'Xiamen, China', 'Xiamen, China',
                    '["XIAMEN"]'::jsonb,
                    'Haoxin Logistics Park Fujian Quanzhou City Jinjiang City Zhangjing.',
                    '12:00 - 20:00',
                    '[
                      {"name":"Donna Zhang","role":"Customer Service – Latam Trade · RS Logistics Ltd. – Xiamen","phone":"+86 592 555 0175 / +86 152 6022 7636","mobile":"+86 152 6022 7636","email":"donnazhang.xm@rslog.com","isPrimary":true,"isActive":true,"agentCodes":["RS","RS LOGISTICS"],"shipmentModes":["FCL"]},
                      {"name":"Sia Liu","role":"Latam Trade · RS Logistics Ltd. – Shanghai","phone":"+86 21 6069 5301 / +86 183 2172 9821","mobile":"+86 183 2172 9821","wechat":"8618321729821","email":"sialiu.sh@rslog.com","isPrimary":false,"isActive":true,"agentCodes":["RS","RS LOGISTICS"],"shipmentModes":["FCL"]},
                      {"name":"Grupo Castro Fallas","role":"Contacto GCF","phone":"","email":"china@grupocastrofallas.com","isPrimary":false,"isActive":true,"alwaysInclude":true,"shipmentModes":["FCL"]}
                    ]'::jsonb,
                    10
                ),
                (
                    'c2910000-0000-4000-8000-000000000006'::uuid,
                    'WHS_SHENZHEN', 'whs-shenzhen-cn', 'Shenzhen, China', 'Shenzhen, China',
                    '["SHENZHEN","SHEKOU","YANTIAN","FOSHAN"]'::jsonb,
                    'No. 1-3, Building 6, District B, Jinpeng Logistics Park',
                    '09:00 - 22:00',
                    '[
                      {"name":"Fiona Li","role":"Customer Service – Latam Trade · RS Logistics Ltd. – Shenzhen","phone":"+852 3187 3061 / +86 755 2519 8243 Ext. 361","email":"fionali.sz@rslog.com","isPrimary":true,"isActive":true,"agentCodes":["RS","RS LOGISTICS"],"shipmentModes":["FCL"]},
                      {"name":"Sia Liu","role":"Latam Trade · RS Logistics Ltd. – Shanghai","phone":"+86 21 6069 5301 / +86 183 2172 9821","mobile":"+86 183 2172 9821","wechat":"8618321729821","email":"sialiu.sh@rslog.com","isPrimary":false,"isActive":true,"agentCodes":["RS","RS LOGISTICS"],"shipmentModes":["FCL"]},
                      {"name":"Grupo Castro Fallas","role":"Contacto GCF","phone":"","email":"china@grupocastrofallas.com","isPrimary":false,"isActive":true,"alwaysInclude":true,"shipmentModes":["FCL"]}
                    ]'::jsonb,
                    60
                ),
                (
                    'c2910000-0000-4000-8000-000000000007'::uuid,
                    'WHS_GUANGZHOU', 'whs-guangzhou-cn', 'Guangzhou, China', 'Guangzhou, China',
                    '["GUANGZHOU"]'::jsonb,
                    'Haoxin Logistics Park Fujian Quanzhou City Jinjiang City Zhangjing',
                    '12:00 - 20:00',
                    '[
                      {"name":"Fiona Li","role":"Customer Service – Latam Trade · RS Logistics Ltd. – Shenzhen","phone":"+852 3187 3061 / +86 755 2519 8243 Ext. 361","email":"fionali.sz@rslog.com","isPrimary":true,"isActive":true,"agentCodes":["RS","RS LOGISTICS"],"shipmentModes":["FCL"]},
                      {"name":"Sia Liu","role":"Latam Trade · RS Logistics Ltd. – Shanghai","phone":"+86 21 6069 5301 / +86 183 2172 9821","mobile":"+86 183 2172 9821","wechat":"8618321729821","email":"sialiu.sh@rslog.com","isPrimary":false,"isActive":true,"agentCodes":["RS","RS LOGISTICS"],"shipmentModes":["FCL"]},
                      {"name":"Grupo Castro Fallas","role":"Contacto GCF","phone":"","email":"china@grupocastrofallas.com","isPrimary":false,"isActive":true,"alwaysInclude":true,"shipmentModes":["FCL"]}
                    ]'::jsonb,
                    70
                ),
                (
                    'c2910000-0000-4000-8000-000000000008'::uuid,
                    'WHS_FUZHOU', 'whs-fuzhou-cn', 'Fuzhou, China', 'Fuzhou, China',
                    '["FUZHOU"]'::jsonb,
                    'Haoxin Logistics Park Fujian Quanzhou City Jinjiang City Zhangjing.',
                    '12:00 - 20:00',
                    '[
                      {"name":"Fiona Li","role":"Customer Service – Latam Trade · RS Logistics Ltd. – Shenzhen","phone":"+852 3187 3061 / +86 755 2519 8243 Ext. 361","email":"fionali.sz@rslog.com","isPrimary":true,"isActive":true,"agentCodes":["RS","RS LOGISTICS"],"shipmentModes":["FCL"]},
                      {"name":"Sia Liu","role":"Latam Trade · RS Logistics Ltd. – Shanghai","phone":"+86 21 6069 5301 / +86 183 2172 9821","mobile":"+86 183 2172 9821","wechat":"8618321729821","email":"sialiu.sh@rslog.com","isPrimary":false,"isActive":true,"agentCodes":["RS","RS LOGISTICS"],"shipmentModes":["FCL"]},
                      {"name":"Grupo Castro Fallas","role":"Contacto GCF","phone":"","email":"china@grupocastrofallas.com","isPrimary":false,"isActive":true,"alwaysInclude":true,"shipmentModes":["FCL"]}
                    ]'::jsonb,
                    80
                ),
                (
                    'c2910000-0000-4000-8000-000000000019'::uuid,
                    'WHS_CHONGQING', 'whs-chongqing-cn', 'Chongqing, China', 'Chongqing, China',
                    '["CHONGQING"]'::jsonb,
                    '',
                    '',
                    '[
                      {"name":"Eartha Yu","role":"Customer Service – Latam Trade · RS Logistics Ltd. – Shanghai","phone":"+86 21 6069 5300","email":"earthayu.sh@rslog.com","isPrimary":true,"isActive":true,"agentCodes":["RS","RS LOGISTICS"],"shipmentModes":["FCL"]},
                      {"name":"Sia Liu","role":"Latam Trade · RS Logistics Ltd. – Shanghai","phone":"+86 21 6069 5301 / +86 183 2172 9821","mobile":"+86 183 2172 9821","wechat":"8618321729821","email":"sialiu.sh@rslog.com","isPrimary":false,"isActive":true,"agentCodes":["RS","RS LOGISTICS"],"shipmentModes":["FCL"]},
                      {"name":"Grupo Castro Fallas","role":"Contacto GCF","phone":"","email":"china@grupocastrofallas.com","isPrimary":false,"isActive":true,"alwaysInclude":true,"shipmentModes":["FCL"]}
                    ]'::jsonb,
                    85
                ),
                (
                    'c2910000-0000-4000-8000-000000000005'::uuid,
                    'WHS_XINGANG', 'whs-xingang-cn', 'Xingang / Tianjin, China', 'Xingang, China',
                    '["XINGANG","TIANJIN"]'::jsonb,
                    'Jinya Logistics Park No.876 East Yangbei Road, Dongli District, Tianjin.',
                    '08:00 - 19:00',
                    '[
                      {"name":"Alison Hu","role":"Customer Service Executive – All Trade · RS Logistics Ltd. – Tianjin","phone":"+86 22 8219 2750","email":"alisonhu.tj@rslog.com","isPrimary":true,"isActive":true,"agentCodes":["RS","RS LOGISTICS"],"shipmentModes":["FCL"]},
                      {"name":"Sia Liu","role":"Latam Trade · RS Logistics Ltd. – Shanghai","phone":"+86 21 6069 5301 / +86 183 2172 9821","mobile":"+86 183 2172 9821","wechat":"8618321729821","email":"sialiu.sh@rslog.com","isPrimary":false,"isActive":true,"agentCodes":["RS","RS LOGISTICS"],"shipmentModes":["FCL"]},
                      {"name":"Grupo Castro Fallas","role":"Contacto GCF","phone":"","email":"china@grupocastrofallas.com","isPrimary":false,"isActive":true,"alwaysInclude":true,"shipmentModes":["FCL"]}
                    ]'::jsonb,
                    50
                ),
                (
                    'c2910000-0000-4000-8000-000000000003'::uuid,
                    'WHS_DALIAN', 'whs-dalian-cn', 'Dalian, China', 'Dalian, China',
                    '["DALIAN"]'::jsonb,
                    'No. 6, West North Road, Ganjingzi District, Dalian City, Liaoning Province',
                    '09:00 - 18:00',
                    '[
                      {"name":"Alison Hu","role":"Customer Service Executive – All Trade · RS Logistics Ltd. – Dalian","phone":"+86 22 8219 2750","email":"alisonhu.tj@rslog.com","isPrimary":true,"isActive":true,"agentCodes":["RS","RS LOGISTICS"],"shipmentModes":["FCL"]},
                      {"name":"Sia Liu","role":"Latam Trade · RS Logistics Ltd. – Shanghai","phone":"+86 21 6069 5301 / +86 183 2172 9821","mobile":"+86 183 2172 9821","wechat":"8618321729821","email":"sialiu.sh@rslog.com","isPrimary":false,"isActive":true,"agentCodes":["RS","RS LOGISTICS"],"shipmentModes":["FCL"]},
                      {"name":"Grupo Castro Fallas","role":"Contacto GCF","phone":"","email":"china@grupocastrofallas.com","isPrimary":false,"isActive":true,"alwaysInclude":true,"shipmentModes":["FCL"]}
                    ]'::jsonb,
                    30
                ),
                (
                    'c2910000-0000-4000-8000-000000000002'::uuid,
                    'WHS_NINGBO', 'whs-ningbo-cn', 'Ningbo, China', 'Ningbo, China',
                    '["NINGBO"]'::jsonb,
                    'No. 188, Citong Avenue, Ningbo.',
                    '08:00 - 19:00',
                    '[
                      {"name":"Charz Zhou","role":"Customer Service Executive – All Trade · RS Logistics Ltd. – Ningbo","phone":"+86 574 2772 0076 / +86 134 2939 0185","mobile":"+86 134 2939 0185","email":"charzzhou.nb@rslog.com","isPrimary":true,"isActive":true,"agentCodes":["RS","RS LOGISTICS"],"shipmentModes":["FCL"]},
                      {"name":"Sia Liu","role":"Latam Trade · RS Logistics Ltd. – Shanghai","phone":"+86 21 6069 5301 / +86 183 2172 9821","mobile":"+86 183 2172 9821","wechat":"8618321729821","email":"sialiu.sh@rslog.com","isPrimary":false,"isActive":true,"agentCodes":["RS","RS LOGISTICS"],"shipmentModes":["FCL"]},
                      {"name":"Grupo Castro Fallas","role":"Contacto GCF","phone":"","email":"china@grupocastrofallas.com","isPrimary":false,"isActive":true,"alwaysInclude":true,"shipmentModes":["FCL"]}
                    ]'::jsonb,
                    20
                );

            UPDATE config."CatalogItems" AS item
            SET name = desired.name,
                value = desired.value,
                description = 'WHS China · matriz operativa RS Logistics para FCL',
                metadata_json = (
                    COALESCE(item.metadata_json, '{}'::jsonb)
                        - 'contacts'
                        - 'contact'
                        - 'email'
                        - 'phone'
                        - 'contactDirectory'
                        - 'polCodes'
                        - 'shipmentModes'
                        - 'address'
                        - 'schedule'
                ) || jsonb_build_object(
                    'address', desired.address,
                    'schedule', desired.schedule,
                    'country', 'China',
                    'countryCode', 'CN',
                    'polCodes', desired.pol_codes,
                    'modalities', '["Maritime","Multimodal"]'::jsonb,
                    'shipmentModes', '["FCL"]'::jsonb,
                    'allIncoterms', true,
                    'fcaRequiresWarehouse', true,
                    'defaultAgentCode', 'RS',
                    'defaultAgentName', 'RS Logistics',
                    'contactPolicy', '{"RS":"RS_MATRIX_PLUS_GCF","OTHER":"GCF_ONLY"}'::jsonb,
                    'nonRsContactDirectory', '[{"name":"Grupo Castro Fallas","role":"Contacto GCF","email":"china@grupocastrofallas.com","phone":"","isPrimary":true,"isActive":true,"shipmentModes":["FCL"]}]'::jsonb,
                    'contactDirectory', desired.contact_directory,
                    'warehouseAssignmentRequired', desired.code = 'WHS_CHONGQING'
                ),
                sort_order = desired.sort_order,
                is_active = TRUE,
                is_deleted = FALSE,
                deleted_at_utc = NULL,
                deleted_by = NULL,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            FROM desired_rs_china_fcl_warehouses AS desired,
                 config."CatalogGroups" AS catalog
            WHERE catalog.slug = 'pricing-warehouses'
              AND catalog.is_deleted = FALSE
              AND item.catalog_group_id = catalog.id
              AND item.is_deleted = FALSE
              AND (
                    UPPER(item.code) = UPPER(desired.code)
                 OR LOWER(item.slug) = LOWER(desired.slug)
              );

            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order,
                 is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT
                desired.id,
                catalog.id,
                desired.code,
                desired.slug,
                desired.name,
                'WHS China · matriz operativa RS Logistics para FCL',
                desired.value,
                jsonb_build_object(
                    'address', desired.address,
                    'schedule', desired.schedule,
                    'country', 'China',
                    'countryCode', 'CN',
                    'polCodes', desired.pol_codes,
                    'modalities', '["Maritime","Multimodal"]'::jsonb,
                    'shipmentModes', '["FCL"]'::jsonb,
                    'allIncoterms', true,
                    'fcaRequiresWarehouse', true,
                    'defaultAgentCode', 'RS',
                    'defaultAgentName', 'RS Logistics',
                    'contactPolicy', '{"RS":"RS_MATRIX_PLUS_GCF","OTHER":"GCF_ONLY"}'::jsonb,
                    'nonRsContactDirectory', '[{"name":"Grupo Castro Fallas","role":"Contacto GCF","email":"china@grupocastrofallas.com","phone":"","isPrimary":true,"isActive":true,"shipmentModes":["FCL"]}]'::jsonb,
                    'contactDirectory', desired.contact_directory,
                    'warehouseAssignmentRequired', desired.code = 'WHS_CHONGQING'
                ),
                desired.sort_order,
                FALSE,
                TRUE,
                NOW(),
                'migration',
                FALSE
            FROM desired_rs_china_fcl_warehouses AS desired
            JOIN config."CatalogGroups" AS catalog
              ON catalog.slug = 'pricing-warehouses'
             AND catalog.is_deleted = FALSE
            WHERE NOT EXISTS (
                SELECT 1
                FROM config."CatalogItems" AS item
                WHERE item.catalog_group_id = catalog.id
                  AND item.is_deleted = FALSE
                  AND (
                        UPPER(item.code) = UPPER(desired.code)
                     OR LOWER(item.slug) = LOWER(desired.slug)
                  )
            );
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Operational catalog repair: do not restore stale contacts or remove WHS data on rollback.
    }
}
