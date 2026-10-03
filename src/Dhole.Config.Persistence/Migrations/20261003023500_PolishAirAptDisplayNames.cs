using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20261003023500_PolishAirAptDisplayNames")]
public sealed class PolishAirAptDisplayNames : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            -- Keep airport identity and IATA details in metadata, while the visible
            -- POL/POE value stays concise: City, Country.
            UPDATE config."CatalogItems" i
            SET
                -- Do not overwrite CatalogItems.name: that column is unique per catalog
                -- and may already be used by an SD/CY location with the same city name.
                -- Pricing renders CatalogItems.value, so only the visible value is shortened.
                value = CASE UPPER(i.metadata_json->>'iataCode')
                    WHEN 'MIA' THEN 'Miami, Estados Unidos'
                    WHEN 'SJO' THEN 'San José, Costa Rica'
                    WHEN 'MAD' THEN 'Madrid, España'
                    ELSE i.value
                END,
                metadata_json = COALESCE(i.metadata_json, '{}'::jsonb) ||
                    CASE UPPER(i.metadata_json->>'iataCode')
                        WHEN 'MIA' THEN '{"city":"Miami","countryName":"Estados Unidos","displayName":"Miami, Estados Unidos"}'::jsonb
                        WHEN 'SJO' THEN '{"city":"San José","countryName":"Costa Rica","displayName":"San José, Costa Rica"}'::jsonb
                        WHEN 'MAD' THEN '{"city":"Madrid","countryName":"España","displayName":"Madrid, España"}'::jsonb
                        ELSE '{}'::jsonb
                    END,
                updated_at_utc = NOW(),
                updated_by = 'migration-air-apt-display-names'
            FROM config."CatalogGroups" g
            WHERE i.catalog_group_id = g.id
              AND g.slug IN ('pol', 'poe')
              AND i.is_deleted = FALSE
              AND UPPER(COALESCE(i.metadata_json->>'terminalType', '')) = 'APT'
              AND UPPER(COALESCE(i.metadata_json->>'iataCode', '')) IN ('MIA', 'SJO', 'MAD');
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Non-destructive. The shorter display labels are safe for existing Pricing references.
    }
}
