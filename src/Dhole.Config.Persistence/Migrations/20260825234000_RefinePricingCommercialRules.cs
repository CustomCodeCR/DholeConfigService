using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260825234000_RefinePricingCommercialRules")]
public sealed class RefinePricingCommercialRules : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            -- Incoterms® 2020: metadata de etapas comerciales usada por Pricing.
            -- El manual CRC sigue siendo la fuente de verdad para los ítems/montos concretos.
            WITH desired(code, metadata_json) AS (
                VALUES
                    ('EXW', '{"rateSections":["pickup_origin","origin_charges","international_freight","destination_charges","delivery_destination"],"officialModeGroup":"any","sellerPaysMainTransport":false,"sellerInsurance":false}'::jsonb),
                    ('FCA', '{"rateSections":["origin_charges","international_freight","destination_charges","delivery_destination"],"officialModeGroup":"any","sellerPaysMainTransport":false,"sellerInsurance":false}'::jsonb),
                    ('FAS', '{"rateSections":["international_freight","destination_charges","delivery_destination"],"officialModeGroup":"sea","sellerPaysMainTransport":false,"sellerInsurance":false}'::jsonb),
                    ('FOB', '{"rateSections":["international_freight","destination_charges","delivery_destination"],"officialModeGroup":"sea","sellerPaysMainTransport":false,"sellerInsurance":false}'::jsonb),
                    ('CFR', '{"rateSections":["destination_charges","delivery_destination"],"officialModeGroup":"sea","sellerPaysMainTransport":true,"sellerInsurance":false}'::jsonb),
                    ('CIF', '{"rateSections":["destination_charges","delivery_destination"],"officialModeGroup":"sea","sellerPaysMainTransport":true,"sellerInsurance":true}'::jsonb),
                    ('CPT', '{"rateSections":["destination_charges","delivery_destination"],"officialModeGroup":"any","sellerPaysMainTransport":true,"sellerInsurance":false}'::jsonb),
                    ('CIP', '{"rateSections":["destination_charges","delivery_destination"],"officialModeGroup":"any","sellerPaysMainTransport":true,"sellerInsurance":true}'::jsonb),
                    ('DAP', '{"rateSections":["destination_charges"],"officialModeGroup":"any","sellerPaysMainTransport":true,"sellerInsurance":false}'::jsonb),
                    ('DPU', '{"rateSections":["destination_charges"],"officialModeGroup":"any","sellerPaysMainTransport":true,"sellerInsurance":false}'::jsonb),
                    ('DDP', '{"rateSections":["destination_charges"],"officialModeGroup":"any","sellerPaysMainTransport":true,"sellerInsurance":false}'::jsonb)
            )
            UPDATE config."CatalogItems" i
            SET metadata_json = COALESCE(i.metadata_json, '{}'::jsonb) || d.metadata_json,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            FROM config."CatalogGroups" g, desired d
            WHERE i.catalog_group_id = g.id
              AND g.slug = 'incoterms'
              AND i.is_deleted = FALSE
              AND UPPER(i.code) = d.code;

            -- Servicios de Pricing asociados a los RateTermItems compartidos de Pricing.
            WITH desired(code, metadata_json) AS (
                VALUES
                    ('INT_TRANSPORT', '{"rateTermItemIds":["f2aa5290-4223-4eb3-af50-b74cbdc78d7a"]}'::jsonb),
                    ('CUSTOMS_CR', '{"rateTermItemIds":["1a940376-d75c-49d0-88c0-010fa14e3bab"]}'::jsonb),
                    ('CUSTOMS_FOREIGN', '{"rateTermItemIds":["10802df5-1ec8-4d24-ac7c-2d2b51092ec7"]}'::jsonb),
                    ('STORAGE', '{"rateTermItemIds":["3f7ea083-42aa-4f06-a0fe-f9c61ab8730e"]}'::jsonb),
                    ('CARGO_INSURANCE', '{"rateTermItemIds":["f5c9663e-6d8f-4aea-85fa-2e7634ad8c1c"],"rateSections":["destination_charges"],"optional":true,"requiresCargoValue":true,"insuredValueFactor":1.10,"salePercentage":0.85,"saleMinimumUsd":125,"costPercentage":0.20,"costMinimumUsd":35,"formula":"(FOB + FREIGHT) * 110% * 0.85%"}'::jsonb),
                    ('PACKING', '{"rateTermItemIds":["500f1517-13fa-42c8-967c-5dff98dbea45"]}'::jsonb),
                    ('PICKUP', '{"rateTermItemIds":["87107485-3fb8-4bc8-88bc-28e357c10450"]}'::jsonb),
                    ('RECEPTION', '{"rateTermItemIds":["f23dab31-3571-43c5-83e0-8e4d8143325e"]}'::jsonb)
            )
            UPDATE config."CatalogItems" i
            SET metadata_json = COALESCE(i.metadata_json, '{}'::jsonb) || d.metadata_json,
                updated_at_utc = NOW(),
                updated_by = 'migration'
            FROM config."CatalogGroups" g, desired d
            WHERE i.catalog_group_id = g.id
              AND g.slug = 'pricing-services'
              AND i.is_deleted = FALSE
              AND UPPER(i.code) = d.code;
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Metadata evolution is intentionally non-destructive.
    }
}
