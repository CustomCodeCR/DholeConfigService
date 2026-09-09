using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260909040000_UnifyPricingRouteCatalogs")]
public sealed class UnifyPricingRouteCatalogs : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            -- POL and POE are shared catalogs. CY/SD lives at item level and determines
            -- whether the location is maritime or terrestrial.
            UPDATE config."CatalogGroups"
            SET
                name = CASE slug
                    WHEN 'pol' THEN 'POL (CY / SD)'
                    WHEN 'poe' THEN 'POE (CY / SD)'
                    ELSE name
                END,
                description = CASE slug
                    WHEN 'pol' THEN 'Puntos de origen de Pricing. CY para marítimo y SD para terrestre.'
                    WHEN 'poe' THEN 'Puntos de salida/destino de Pricing. CY para marítimo y SD para terrestre.'
                    ELSE description
                END,
                metadata_json = (COALESCE(metadata_json, '{}'::jsonb) - 'transportMode' - 'terminalType') ||
                    CASE slug
                        WHEN 'pol' THEN '{"pricingWorkflow":true,"routeRole":"POL","terminalTypes":["CY","SD"],"transportModes":["Maritime","Land"]}'::jsonb
                        WHEN 'poe' THEN '{"pricingWorkflow":true,"routeRole":"POE","terminalTypes":["CY","SD"],"transportModes":["Maritime","Land"]}'::jsonb
                        ELSE '{}'::jsonb
                    END,
                is_active = TRUE,
                is_deleted = FALSE,
                updated_at_utc = NOW(),
                updated_by = 'migration-unified-route-catalogs'
            WHERE slug IN ('pol', 'poe');

            -- Existing POL/POE records are maritime unless they were already explicitly
            -- classified. This keeps all reviewed CY ports backwards compatible.
            UPDATE config."CatalogItems" i
            SET
                metadata_json = COALESCE(i.metadata_json, '{}'::jsonb) || jsonb_build_object(
                    'transportMode', COALESCE(NULLIF(i.metadata_json->>'transportMode', ''), 'Maritime'),
                    'terminalType', COALESCE(NULLIF(i.metadata_json->>'terminalType', ''), 'CY'),
                    'routeRole', CASE g.slug WHEN 'pol' THEN 'POL' ELSE 'POE' END
                ),
                updated_at_utc = NOW(),
                updated_by = 'migration-unified-route-catalogs'
            FROM config."CatalogGroups" g
            WHERE i.catalog_group_id = g.id
              AND g.slug IN ('pol', 'poe')
              AND i.is_deleted = FALSE;

            -- Move every legacy terrestrial POL item into the shared POL catalog while
            -- preserving the item id. If a manually-created item collides with a maritime
            -- code/slug/name, suffix only that terrestrial record before moving it.
            DO $$
            DECLARE
                source_group uuid;
                target_group uuid;
                r record;
                token text;
                next_code text;
                next_slug text;
                next_name text;
            BEGIN
                SELECT id INTO source_group
                FROM config."CatalogGroups"
                WHERE slug = 'land-pol'
                ORDER BY is_deleted ASC
                LIMIT 1;

                SELECT id INTO target_group
                FROM config."CatalogGroups"
                WHERE slug = 'pol' AND is_deleted = FALSE
                LIMIT 1;

                IF source_group IS NOT NULL AND target_group IS NOT NULL THEN
                    FOR r IN
                        SELECT id, code, slug, name
                        FROM config."CatalogItems"
                        WHERE catalog_group_id = source_group
                          AND is_deleted = FALSE
                    LOOP
                        token := substr(replace(r.id::text, '-', ''), 1, 8);
                        next_code := r.code;
                        next_slug := r.slug;
                        next_name := r.name;

                        IF EXISTS (SELECT 1 FROM config."CatalogItems" t WHERE t.catalog_group_id = target_group AND t.is_deleted = FALSE AND UPPER(t.code) = UPPER(r.code)) THEN
                            next_code := left(r.code, 68) || '_SD_' || token;
                        END IF;
                        IF EXISTS (SELECT 1 FROM config."CatalogItems" t WHERE t.catalog_group_id = target_group AND t.is_deleted = FALSE AND LOWER(t.slug) = LOWER(r.slug)) THEN
                            next_slug := left(r.slug, 187) || '-sd-' || token;
                        END IF;
                        IF EXISTS (SELECT 1 FROM config."CatalogItems" t WHERE t.catalog_group_id = target_group AND t.is_deleted = FALSE AND LOWER(t.name) = LOWER(r.name)) THEN
                            next_name := left(r.name, 236) || ' (SD ' || token || ')';
                        END IF;

                        UPDATE config."CatalogItems"
                        SET
                            catalog_group_id = target_group,
                            code = next_code,
                            slug = next_slug,
                            name = next_name,
                            metadata_json = COALESCE(metadata_json, '{}'::jsonb) || '{"modality":"Land","transportMode":"Land","terminalType":"SD","routeRole":"POL"}'::jsonb,
                            updated_at_utc = NOW(),
                            updated_by = 'migration-unified-route-catalogs'
                        WHERE id = r.id;
                    END LOOP;
                END IF;
            END $$;

            -- Same consolidation for terrestrial POE.
            DO $$
            DECLARE
                source_group uuid;
                target_group uuid;
                r record;
                token text;
                next_code text;
                next_slug text;
                next_name text;
            BEGIN
                SELECT id INTO source_group
                FROM config."CatalogGroups"
                WHERE slug = 'land-poe'
                ORDER BY is_deleted ASC
                LIMIT 1;

                SELECT id INTO target_group
                FROM config."CatalogGroups"
                WHERE slug = 'poe' AND is_deleted = FALSE
                LIMIT 1;

                IF source_group IS NOT NULL AND target_group IS NOT NULL THEN
                    FOR r IN
                        SELECT id, code, slug, name
                        FROM config."CatalogItems"
                        WHERE catalog_group_id = source_group
                          AND is_deleted = FALSE
                    LOOP
                        token := substr(replace(r.id::text, '-', ''), 1, 8);
                        next_code := r.code;
                        next_slug := r.slug;
                        next_name := r.name;

                        IF EXISTS (SELECT 1 FROM config."CatalogItems" t WHERE t.catalog_group_id = target_group AND t.is_deleted = FALSE AND UPPER(t.code) = UPPER(r.code)) THEN
                            next_code := left(r.code, 68) || '_SD_' || token;
                        END IF;
                        IF EXISTS (SELECT 1 FROM config."CatalogItems" t WHERE t.catalog_group_id = target_group AND t.is_deleted = FALSE AND LOWER(t.slug) = LOWER(r.slug)) THEN
                            next_slug := left(r.slug, 187) || '-sd-' || token;
                        END IF;
                        IF EXISTS (SELECT 1 FROM config."CatalogItems" t WHERE t.catalog_group_id = target_group AND t.is_deleted = FALSE AND LOWER(t.name) = LOWER(r.name)) THEN
                            next_name := left(r.name, 236) || ' (SD ' || token || ')';
                        END IF;

                        UPDATE config."CatalogItems"
                        SET
                            catalog_group_id = target_group,
                            code = next_code,
                            slug = next_slug,
                            name = next_name,
                            metadata_json = COALESCE(metadata_json, '{}'::jsonb) || '{"modality":"Land","transportMode":"Land","terminalType":"SD","routeRole":"POE"}'::jsonb,
                            updated_at_utc = NOW(),
                            updated_by = 'migration-unified-route-catalogs'
                        WHERE id = r.id;
                    END LOOP;
                END IF;
            END $$;

            -- Legacy groups remain only as soft-deleted migration history. They no longer
            -- contain active route items and are not exposed as usable catalogs.
            UPDATE config."CatalogGroups"
            SET
                is_active = FALSE,
                is_deleted = TRUE,
                updated_at_utc = NOW(),
                updated_by = 'migration-unified-route-catalogs'
            WHERE slug IN ('land-pol', 'land-poe');

            -- POD remains maritime SD only.
            UPDATE config."CatalogGroups"
            SET
                name = 'POD marítimo (SD)',
                metadata_json = COALESCE(metadata_json, '{}'::jsonb) || '{"pricingWorkflow":true,"transportMode":"Maritime","terminalType":"SD","routeRole":"POD"}'::jsonb,
                updated_at_utc = NOW(),
                updated_by = 'migration-unified-route-catalogs'
            WHERE slug = 'pod' AND is_deleted = FALSE;

            UPDATE config."CatalogItems" i
            SET
                metadata_json = COALESCE(i.metadata_json, '{}'::jsonb) || '{"transportMode":"Maritime","terminalType":"SD","routeRole":"POD"}'::jsonb,
                updated_at_utc = NOW(),
                updated_by = 'migration-unified-route-catalogs'
            FROM config."CatalogGroups" g
            WHERE i.catalog_group_id = g.id
              AND g.slug = 'pod'
              AND i.is_deleted = FALSE;
            """
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Non-destructive by design. Catalog item ids may already be referenced by Pricing.
    }
}
