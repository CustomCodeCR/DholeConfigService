using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260901190000_ExtendPricingWarehouseRoutingAndContacts")]
public sealed class ExtendPricingWarehouseRoutingAndContacts : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE config."CatalogGroups"
            SET metadata_json = COALESCE(metadata_json, '{}'::jsonb)
                || '{"supportsMultipleContacts":true,"supportsRouteContacts":true,"supportsModalityContacts":true,"supportsPolResolution":true}'::jsonb,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            WHERE slug = 'pricing-warehouses'
              AND is_deleted = FALSE;

            UPDATE config."CatalogItems" AS item
            SET metadata_json = COALESCE(item.metadata_json, '{}'::jsonb)
                || jsonb_build_object(
                    'polCodes', routing.pol_codes,
                    'modalities', '["Maritime","Multimodal"]'::jsonb,
                    'shipmentModes', '["FCL","LCL"]'::jsonb,
                    'contactDirectory', CASE
                        WHEN COALESCE(item.metadata_json->>'contacts','') = '' THEN '[]'::jsonb
                        ELSE jsonb_build_array(
                            jsonb_build_object(
                                'name', item.metadata_json->>'contacts',
                                'email', item.metadata_json->>'email',
                                'phone', item.metadata_json->>'phone',
                                'isPrimary', true,
                                'isActive', true,
                                'modalities', '["Maritime","Multimodal"]'::jsonb,
                                'shipmentModes', '["FCL","LCL"]'::jsonb
                            )
                        )
                    END
                ),
                updated_at_utc = NOW(),
                updated_by = 'migration'
            FROM config."CatalogGroups" AS catalog_group,
                 (VALUES
                    ('WHS_XIAMEN', '["XIAMEN"]'::jsonb),
                    ('WHS_NINGBO', '["NINGBO"]'::jsonb),
                    ('WHS_DALIAN', '["DALIAN"]'::jsonb),
                    ('WHS_QINGDAO', '["QINGDAO"]'::jsonb),
                    ('WHS_XINGANG', '["XINGANG","TIANJIN"]'::jsonb),
                    ('WHS_SHENZHEN', '["SHENZHEN","SHEKOU"]'::jsonb),
                    ('WHS_GUANGZHOU', '["GUANGZHOU"]'::jsonb),
                    ('WHS_FUZHOU', '["FUZHOU"]'::jsonb),
                    ('WHS_SHANGHAI', '["SHANGHAI"]'::jsonb),
                    ('WHS_BARCELONA', '["BARCELONA"]'::jsonb),
                    ('WHS_MIAMI', '["MIAMI"]'::jsonb),
                    ('WHS_NEW_YORK', '["NEW YORK","NEW_YORK"]'::jsonb),
                    ('WHS_HOUSTON', '["HOUSTON"]'::jsonb),
                    ('WHS_LOS_ANGELES', '["LOS ANGELES","LOS_ANGELES"]'::jsonb),
                    ('WHS_HUENEME', '["HUENEME"]'::jsonb),
                    ('WHS_PANAMA', '["BALBOA","COLON","COLÓN"]'::jsonb)
                 ) AS routing(code, pol_codes)
            WHERE catalog_group.id = item.catalog_group_id
              AND catalog_group.slug = 'pricing-warehouses'
              AND catalog_group.is_deleted = FALSE
              AND item.is_deleted = FALSE
              AND UPPER(item.code) = routing.code;
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE config."CatalogItems" AS item
            SET metadata_json = COALESCE(item.metadata_json, '{}'::jsonb)
                - 'polCodes'
                - 'modalities'
                - 'shipmentModes'
                - 'contactDirectory',
                updated_at_utc = NOW(),
                updated_by = 'migration'
            FROM config."CatalogGroups" AS catalog_group
            WHERE catalog_group.id = item.catalog_group_id
              AND catalog_group.slug = 'pricing-warehouses'
              AND catalog_group.is_deleted = FALSE
              AND item.is_deleted = FALSE;

            UPDATE config."CatalogGroups"
            SET metadata_json = COALESCE(metadata_json, '{}'::jsonb)
                - 'supportsMultipleContacts'
                - 'supportsRouteContacts'
                - 'supportsModalityContacts'
                - 'supportsPolResolution',
                updated_at_utc = NOW(),
                updated_by = 'migration'
            WHERE slug = 'pricing-warehouses'
              AND is_deleted = FALSE;
            """
        );
    }
}
