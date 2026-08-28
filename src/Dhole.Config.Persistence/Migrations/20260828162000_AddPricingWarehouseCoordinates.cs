using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260828162000_AddPricingWarehouseCoordinates")]
public sealed class AddPricingWarehouseCoordinates : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE config."CatalogItems" AS item
            SET metadata_json = COALESCE(item.metadata_json, '{}'::jsonb)
                || jsonb_build_object(
                    'latitude', coordinates.latitude,
                    'longitude', coordinates.longitude,
                    'coordinatePrecision', coordinates.precision,
                    'coordinateSource', 'warehouse-address'
                ),
                updated_at_utc = NOW(),
                updated_by = 'migration'
            FROM config."CatalogGroups" AS catalog_group,
                 (VALUES
                    ('WHS_XIAMEN', 24.726316::numeric, 118.568424::numeric, 'area'),
                    ('WHS_NINGBO', 30.123700::numeric, 121.557200::numeric, 'site'),
                    ('WHS_DALIAN', 39.047188::numeric, 121.681813::numeric, 'area'),
                    ('WHS_QINGDAO', 36.298938::numeric, 120.351063::numeric, 'area'),
                    ('WHS_XINGANG', 39.227161::numeric, 117.352392::numeric, 'site'),
                    ('WHS_SHENZHEN', 22.644340::numeric, 114.140210::numeric, 'site'),
                    ('WHS_GUANGZHOU', 23.281188::numeric, 113.359313::numeric, 'area'),
                    ('WHS_FUZHOU', 24.726316::numeric, 118.568424::numeric, 'area'),
                    ('WHS_SHANGHAI', 31.050688::numeric, 121.846938::numeric, 'area'),
                    ('WHS_BARCELONA', 41.522910::numeric, 2.091970::numeric, 'area'),
                    ('WHS_MIAMI', 25.792429::numeric, -80.335249::numeric, 'site'),
                    ('WHS_NEW_YORK', 40.801277::numeric, -74.029567::numeric, 'site'),
                    ('WHS_HOUSTON', 29.785330::numeric, -95.290254::numeric, 'site'),
                    ('WHS_LOS_ANGELES', 33.857577::numeric, -118.304518::numeric, 'site'),
                    ('WHS_HUENEME', 34.207970::numeric, -119.125795::numeric, 'site'),
                    ('WHS_A257_CURRIDABAT', 9.913570::numeric, -84.045010::numeric, 'site'),
                    ('WHS_A287_TIBAS', 9.948570::numeric, -84.074120::numeric, 'area'),
                    ('WHS_PANAMA', 9.345087::numeric, -79.883564::numeric, 'area')
                 ) AS coordinates(code, latitude, longitude, precision)
            WHERE catalog_group.id = item.catalog_group_id
              AND catalog_group.slug = 'pricing-warehouses'
              AND catalog_group.is_deleted = FALSE
              AND item.is_deleted = FALSE
              AND UPPER(item.code) = coordinates.code;
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE config."CatalogItems" AS item
            SET metadata_json = COALESCE(item.metadata_json, '{}'::jsonb)
                - 'latitude'
                - 'longitude'
                - 'coordinatePrecision'
                - 'coordinateSource',
                updated_at_utc = NOW(),
                updated_by = 'migration'
            FROM config."CatalogGroups" AS catalog_group
            WHERE catalog_group.id = item.catalog_group_id
              AND catalog_group.slug = 'pricing-warehouses'
              AND catalog_group.is_deleted = FALSE
              AND item.is_deleted = FALSE
              AND UPPER(item.code) IN (
                    'WHS_XIAMEN',
                    'WHS_NINGBO',
                    'WHS_DALIAN',
                    'WHS_QINGDAO',
                    'WHS_XINGANG',
                    'WHS_SHENZHEN',
                    'WHS_GUANGZHOU',
                    'WHS_FUZHOU',
                    'WHS_SHANGHAI',
                    'WHS_BARCELONA',
                    'WHS_MIAMI',
                    'WHS_NEW_YORK',
                    'WHS_HOUSTON',
                    'WHS_LOS_ANGELES',
                    'WHS_HUENEME',
                    'WHS_A257_CURRIDABAT',
                    'WHS_A287_TIBAS',
                    'WHS_PANAMA'
              );
            """
        );
    }
}
