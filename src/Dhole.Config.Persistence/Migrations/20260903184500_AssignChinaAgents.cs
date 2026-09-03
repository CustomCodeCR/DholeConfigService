using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260903184500_AssignChinaAgents")]
public sealed class AssignChinaAgents : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE config."CatalogItems" AS item
            SET metadata_json = COALESCE(item.metadata_json, '{}'::jsonb) ||
                    '{
                      "country":"China",
                      "countryName":"China",
                      "countryCode":"CN",
                      "countryIso2":"CN",
                      "countryIso3":"CHN",
                      "originCountry":"China",
                      "originCountryName":"China",
                      "originCountryCode":"CN",
                      "countries":["China"],
                      "countryCodes":["CN"]
                    }'::jsonb,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            FROM config."CatalogGroups" AS catalog
            WHERE item.catalog_group_id = catalog.id
              AND catalog.slug = 'agents'
              AND catalog.is_deleted = FALSE
              AND item.is_deleted = FALSE
              AND (
                    UPPER(COALESCE(item.name, '')) IN ('RS', 'WWL')
                 OR UPPER(COALESCE(item.value, '')) IN ('RS', 'WWL')
                 OR LOWER(COALESCE(item.slug, '')) IN ('rs', 'wwl')
              );
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE config."CatalogItems" AS item
            SET metadata_json = COALESCE(item.metadata_json, '{}'::jsonb)
                    - 'country'
                    - 'countryName'
                    - 'countryCode'
                    - 'countryIso2'
                    - 'countryIso3'
                    - 'originCountry'
                    - 'originCountryName'
                    - 'originCountryCode'
                    - 'countries'
                    - 'countryCodes',
                updated_at_utc = NOW(),
                updated_by = 'migration-rollback'
            FROM config."CatalogGroups" AS catalog
            WHERE item.catalog_group_id = catalog.id
              AND catalog.slug = 'agents'
              AND catalog.is_deleted = FALSE
              AND item.is_deleted = FALSE
              AND (
                    UPPER(COALESCE(item.name, '')) IN ('RS', 'WWL')
                 OR UPPER(COALESCE(item.value, '')) IN ('RS', 'WWL')
                 OR LOWER(COALESCE(item.slug, '')) IN ('rs', 'wwl')
              );
            """
        );
    }
}
