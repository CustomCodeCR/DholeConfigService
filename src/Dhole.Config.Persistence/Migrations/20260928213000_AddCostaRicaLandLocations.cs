using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260928213000_AddCostaRicaLandLocations")]
public sealed class AddCostaRicaLandLocations : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            -- Ubicaciones terrestres administrables para FTL y LTL.
            -- POL/POE son catálogos compartidos; SD identifica puntos terrestres.
            WITH desired(ordinal, pol_id, poe_id, code, slug, name) AS (
                VALUES
                    (110, 'c2600000-0000-4000-8000-000000000011'::uuid, 'c2610000-0000-4000-8000-000000000011'::uuid, 'SD_ALAJUELA_COSTA_RICA', 'alajuela-costa-rica', 'Alajuela, Costa Rica'),
                    (120, 'c2600000-0000-4000-8000-000000000012'::uuid, 'c2610000-0000-4000-8000-000000000012'::uuid, 'SD_HEREDIA_COSTA_RICA',  'heredia-costa-rica',  'Heredia, Costa Rica'),
                    (130, 'c2600000-0000-4000-8000-000000000013'::uuid, 'c2610000-0000-4000-8000-000000000013'::uuid, 'SD_COYOL_COSTA_RICA',    'coyol-costa-rica',    'Coyol, Costa Rica')
            )
            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order,
                 is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT
                CASE WHEN g.slug = 'pol' THEN d.pol_id ELSE d.poe_id END,
                g.id,
                d.code,
                d.slug || CASE WHEN g.slug = 'poe' THEN '-poe' ELSE '' END,
                d.name,
                'Ubicación terrestre SD editable para Pricing FTL/LTL.',
                d.name,
                jsonb_build_object(
                    'pricingWorkflow', true,
                    'transportMode', 'Land',
                    'terminalType', 'SD',
                    'routeRole', UPPER(g.slug),
                    'countryCode', 'CR'
                ),
                10000 + d.ordinal,
                FALSE,
                TRUE,
                NOW(),
                'migration-land-locations-20260928',
                FALSE
            FROM config."CatalogGroups" g
            CROSS JOIN desired d
            WHERE g.slug IN ('pol', 'poe')
              AND g.is_deleted = FALSE
            ON CONFLICT (id) DO UPDATE SET
                catalog_group_id = EXCLUDED.catalog_group_id,
                code = EXCLUDED.code,
                slug = EXCLUDED.slug,
                name = EXCLUDED.name,
                description = EXCLUDED.description,
                value = EXCLUDED.value,
                metadata_json = EXCLUDED.metadata_json,
                sort_order = EXCLUDED.sort_order,
                is_system = FALSE,
                is_active = TRUE,
                is_deleted = FALSE,
                deleted_at_utc = NULL,
                deleted_by = NULL,
                updated_at_utc = NOW(),
                updated_by = 'migration-land-locations-20260928';
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DELETE FROM config."CatalogItems"
            WHERE id IN (
                'c2600000-0000-4000-8000-000000000011'::uuid,
                'c2610000-0000-4000-8000-000000000011'::uuid,
                'c2600000-0000-4000-8000-000000000012'::uuid,
                'c2610000-0000-4000-8000-000000000012'::uuid,
                'c2600000-0000-4000-8000-000000000013'::uuid,
                'c2610000-0000-4000-8000-000000000013'::uuid
            );
            """
        );
    }
}
