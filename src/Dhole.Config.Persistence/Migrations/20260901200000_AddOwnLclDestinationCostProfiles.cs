using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260901200000_AddOwnLclDestinationCostProfiles")]
public sealed class AddOwnLclDestinationCostProfiles : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            INSERT INTO config."CatalogGroups"
                (id, code, slug, name, description, metadata_json, is_system, is_active, created_at_utc, created_by, is_deleted)
            VALUES
                ('c3000000-0000-4000-8000-000000000001'::uuid,
                 'PRICING_PANAMA_ARRIVAL_PORTS',
                 'pricing-panama-arrival-ports',
                 'Puertos de llegada Panamá - LCL propio',
                 'Puertos de llegada en Panamá usados para resolver automáticamente los cargos en destino de consolidados propios.',
                 '{"pricingWorkflow":true,"ownLcl":true,"automation":"carrier+arrivalPort"}'::jsonb,
                 TRUE, TRUE, NOW(), 'migration', FALSE),
                ('c3000000-0000-4000-8000-000000000002'::uuid,
                 'PRICING_OWN_LCL_DESTINATION_PROFILES',
                 'pricing-own-lcl-destination-profiles',
                 'Perfiles de cargos en destino - LCL propio',
                 'Perfiles versionables por naviera y puerto de llegada. Pricing consume estos valores como costos bloqueados.',
                 '{"pricingWorkflow":true,"ownLcl":true,"automation":"carrier+arrivalPort","costsEditableInPricing":false}'::jsonb,
                 TRUE, TRUE, NOW(), 'migration', FALSE)
            ON CONFLICT DO NOTHING;

            WITH desired(id, code, slug, name, description, value, metadata_json, sort_order) AS (
                VALUES
                    ('c3100000-0000-4000-8000-000000000001'::uuid,
                     'BALBOA',
                     'balboa-panama',
                     'Balboa, Panamá',
                     'Puerto de llegada base para la operación China → Panamá / Centroamérica.',
                     'BALBOA',
                     '{"aliases":["BALBOA","PABLB"],"countryCode":"PA","finalRatePointCode":"CFZ","finalRatePointName":"Colón Free Zone"}',
                     10)
            )
            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT d.id, g.id, d.code, d.slug, d.name, d.description, d.value, d.metadata_json::jsonb,
                   d.sort_order, TRUE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" g
            CROSS JOIN desired d
            WHERE g.slug = 'pricing-panama-arrival-ports'
              AND g.is_deleted = FALSE
              AND NOT EXISTS (
                  SELECT 1 FROM config."CatalogItems" i
                  WHERE i.catalog_group_id = g.id
                    AND i.is_deleted = FALSE
                    AND (UPPER(i.code) = UPPER(d.code) OR LOWER(i.slug) = LOWER(d.slug))
              )
            ON CONFLICT DO NOTHING;

            WITH desired(id, code, slug, name, description, value, metadata_json, sort_order) AS (
                VALUES
                    ('c3200000-0000-4000-8000-000000000001'::uuid,
                     'MAERSK_BALBOA_CFZ',
                     'maersk-balboa-cfz',
                     'Maersk · Balboa → Colón Free Zone',
                     'Perfil automático validado contra las matrices CNCA-023 #048 y CNCA-024 #049 actualizadas.',
                     'MAERSK_BALBOA_CFZ',
                     '{
                        "version":"CNCA-PANAMA-2026-09-v1",
                        "currency":"USD",
                        "carrierAliases":["MAERSK","MAERSK LINE","MSK","MAEU"],
                        "arrivalPortAliases":["BALBOA","PABLB"],
                        "finalRatePointCode":"CFZ",
                        "finalRatePointName":"Colón Free Zone",
                        "defaultIncludeEmptyReturn":true,
                        "costsEditableInPricing":false,
                        "charges":[
                            {"code":"OCEAN_DESTINATION","name":"Ocean Destination Charge","amount":432.00,"basis":"CONTAINER","required":true,"defaultIncluded":true,"components":["THC","Manejos","Documentación","Additional / Operational","Release"]},
                            {"code":"BALBOA_TO_CFZ","name":"Balboa → Colón Free Zone","amount":400.00,"basis":"CONTAINER","required":true,"defaultIncluded":true},
                            {"code":"SYSTEM_FEE","name":"Fondo / System Fee","amount":0.00,"basis":"CONTAINER","required":true,"defaultIncluded":true},
                            {"code":"EMPTY_RETURN","name":"Retiro de vacío","amount":80.00,"basis":"CONTAINER","required":false,"defaultIncluded":true}
                        ],
                        "costaRicaTransfer":{"panamaToCostaRica":2140.00,"bunker":280.00,"baseCbm":95.00},
                        "sourceRefs":["CNCA-023-#048","CNCA-024-#049"],
                        "notes":"Los costos se cargan automáticamente por naviera + puerto de llegada. Pricing puede editar venta, no costo."
                     }',
                     10)
            )
            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT d.id, g.id, d.code, d.slug, d.name, d.description, d.value, d.metadata_json::jsonb,
                   d.sort_order, TRUE, TRUE, NOW(), 'migration', FALSE
            FROM config."CatalogGroups" g
            CROSS JOIN desired d
            WHERE g.slug = 'pricing-own-lcl-destination-profiles'
              AND g.is_deleted = FALSE
              AND NOT EXISTS (
                  SELECT 1 FROM config."CatalogItems" i
                  WHERE i.catalog_group_id = g.id
                    AND i.is_deleted = FALSE
                    AND (UPPER(i.code) = UPPER(d.code) OR LOWER(i.slug) = LOWER(d.slug))
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
            WHERE id IN (
                'c3100000-0000-4000-8000-000000000001'::uuid,
                'c3200000-0000-4000-8000-000000000001'::uuid
            );

            DELETE FROM config."CatalogGroups"
            WHERE id IN (
                'c3000000-0000-4000-8000-000000000001'::uuid,
                'c3000000-0000-4000-8000-000000000002'::uuid
            );
            """
        );
    }
}
