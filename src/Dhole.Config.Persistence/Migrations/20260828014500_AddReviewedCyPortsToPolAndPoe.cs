using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Config.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260828014500_AddReviewedCyPortsToPolAndPoe")]
public sealed class AddReviewedCyPortsToPolAndPoe : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            WITH desired(id, code, slug, value, sort_order) AS (
                VALUES
                    ('3d17a4e4-ef89-5ab5-9091-b6ffd0bbc823'::uuid, 'CY_ALGECIRAS_ESPANA', 'algeciras-espana', 'Algeciras, España', 10),
                    ('71fbe201-9f72-573b-984b-000e059eee0c'::uuid, 'CY_ALTAMIRA_MEXICO', 'altamira-mexico', 'Altamira, México', 20),
                    ('ba5a7769-a90f-527d-999e-9d9ab7537e6d'::uuid, 'CY_AMBARLI_ESTAMBUL_TURQUIA', 'ambarli-estambul-turquia', 'Ambarli / Estambul, Turquía', 30),
                    ('7723ef50-2bfb-5a3c-9eb0-eaebcbd2db29'::uuid, 'CY_ANTWERP_AMBERES_BELGICA', 'antwerp-amberes-belgica', 'Antwerp / Amberes, Bélgica', 40),
                    ('516b8bb9-15c5-51b0-a988-b31bc169a5ff'::uuid, 'CY_BALBOA_PANAMA', 'balboa-panama', 'Balboa, Panamá', 50),
                    ('db87bd8f-c42d-55f4-847a-f8a977ace6b4'::uuid, 'CY_BALTIMORE_ESTADOS_UNIDOS', 'baltimore-estados-unidos', 'Baltimore, Estados Unidos', 60),
                    ('d36f3483-aaf2-5952-b1d2-24a6e38fde1a'::uuid, 'CY_BARCELONA_ESPANA', 'barcelona-espana', 'Barcelona, España', 70),
                    ('859d7cb7-8cce-5c4d-867d-358c592514ea'::uuid, 'CY_BARRANQUILLA_COLOMBIA', 'barranquilla-colombia', 'Barranquilla, Colombia', 80),
                    ('bbf8c467-ff2b-5507-a8a0-7c4c7c14e22a'::uuid, 'CY_BUENAVENTURA_COLOMBIA', 'buenaventura-colombia', 'Buenaventura, Colombia', 90),
                    ('11693075-250e-50c0-875f-31d0851415b9'::uuid, 'CY_BUENOS_AIRES_ARGENTINA', 'buenos-aires-argentina', 'Buenos Aires, Argentina', 100),
                    ('00944508-454c-5cea-beec-6f8a63cb252e'::uuid, 'CY_CALLAO_PERU', 'callao-peru', 'Callao, Perú', 110),
                    ('254f8013-7609-56aa-a42e-a45df5f0ac86'::uuid, 'CY_CARTAGENA_COLOMBIA', 'cartagena-colombia', 'Cartagena, Colombia', 120),
                    ('26140d58-b548-5f1e-bac0-8de3fb124b77'::uuid, 'CY_CHARLESTON_ESTADOS_UNIDOS', 'charleston-estados-unidos', 'Charleston, Estados Unidos', 130),
                    ('1147bec1-d3c1-5c03-9782-ce61eec633a9'::uuid, 'CY_COLON_CONTAINER_PANAMA', 'colon-container-panama', 'Colón Container, Panamá', 140),
                    ('18192463-daa5-577c-b36a-33192838435f'::uuid, 'CY_DUBAI_EMIRATOS_ARABES_UNIDOS', 'dubai-emiratos-arabes-unidos', 'Dubai, Emiratos Árabes Unidos', 150),
                    ('36c2a2d3-5e10-5d25-b601-976e645f3a25'::uuid, 'CY_DURBAN_SUDAFRICA', 'durban-sudafrica', 'Durban, Sudáfrica', 160),
                    ('c420db3a-6946-5cc2-91da-0582d6cabcf5'::uuid, 'CY_GALVESTON_TX_ESTADOS_UNIDOS', 'galveston-tx-estados-unidos', 'Galveston TX, Estados Unidos', 170),
                    ('24e04e48-069d-52cb-b8c8-5a1b3f61215a'::uuid, 'CY_GEBZE_TURQUIA', 'gebze-turquia', 'Gebze, Turquía', 180),
                    ('6030e111-0d10-57f9-8f71-5dd28645b30f'::uuid, 'CY_GENOA_GENOVA_ITALIA', 'genoa-genova-italia', 'Genoa / Génova, Italia', 190),
                    ('934bef8b-176f-5ece-a68b-e47f4642c145'::uuid, 'CY_GUAYAQUIL_ECUADOR', 'guayaquil-ecuador', 'Guayaquil, Ecuador', 200),
                    ('63f5e834-59e7-5f32-bcf6-8426393a7dbb'::uuid, 'CY_GDYNIA_POLONIA', 'gdynia-polonia', 'Gdynia, Polonia', 210),
                    ('2218bed4-6fa3-5fd6-9a39-b7cdf25cd0d9'::uuid, 'CY_HALIFAX_CANADA', 'halifax-canada', 'Halifax, Canadá', 220),
                    ('090ae8bd-b68d-52e3-9ea1-cf2159561a52'::uuid, 'CY_HAMBURGO_ALEMANIA', 'hamburgo-alemania', 'Hamburgo, Alemania', 230),
                    ('b1208bb5-83a2-5cb8-83a5-b7683360d491'::uuid, 'CY_HOUSTON_TX_ESTADOS_UNIDOS', 'houston-tx-estados-unidos', 'Houston TX, Estados Unidos', 240),
                    ('ee624b98-31ef-53aa-af34-6b35df27ac9c'::uuid, 'CY_INCHEON_INCHON_COREA_DEL_SUR', 'incheon-inchon-corea-del-sur', 'Incheon / Inchon, Corea del Sur', 250),
                    ('76d834c2-a136-5957-9d0b-9603eb083bd3'::uuid, 'CY_ESTAMBUL_TURQUIA', 'estambul-turquia', 'Estambul, Turquía', 260),
                    ('4c8b3e03-c066-5d7a-9c92-0403b31ffbe9'::uuid, 'CY_KATTUPALLI_INDIA', 'kattupalli-india', 'Kattupalli, India', 270),
                    ('f429401f-bd02-59c5-912b-108aeed12868'::uuid, 'CY_LA_SPEZIA_ITALIA', 'la-spezia-italia', 'La Spezia, Italia', 280),
                    ('0dfb401a-73ed-5e0f-b48b-a4669886ce8b'::uuid, 'CY_LE_HAVRE_FRANCIA', 'le-havre-francia', 'Le Havre, Francia', 290),
                    ('de43ef16-a836-5dd3-879d-7a563d362c49'::uuid, 'CY_LISBOA_PORTUGAL', 'lisboa-portugal', 'Lisboa, Portugal', 300),
                    ('8c66617a-4d86-53fa-b3d1-3222f0775bd0'::uuid, 'CY_LIVORNO_ITALIA', 'livorno-italia', 'Livorno, Italia', 310),
                    ('2926f24e-6c1a-5b31-9633-53729c4d3fb1'::uuid, 'CY_LOS_ANGELES_ESTADOS_UNIDOS', 'los-angeles-estados-unidos', 'Los Ángeles, Estados Unidos', 320),
                    ('b6138cfc-7da9-5325-a6e7-09b04fa10303'::uuid, 'CY_MANATEE_ESTADOS_UNIDOS', 'manatee-estados-unidos', 'Manatee, Estados Unidos', 330),
                    ('196aa17b-e238-5cb8-8563-2b34ce95d293'::uuid, 'CY_MANZANILLO_MEXICO', 'manzanillo-mexico', 'Manzanillo, México', 340),
                    ('b7fa7009-ee0a-502f-b24c-32ea6e3bc46a'::uuid, 'CY_MANZANILLO_PANAMA', 'manzanillo-panama', 'Manzanillo, Panamá', 350),
                    ('c12fc0f2-8b5f-59db-8877-ab250741dd81'::uuid, 'CY_MERSIN_TURQUIA', 'mersin-turquia', 'Mersin, Turquía', 360),
                    ('031d7570-6844-5750-915c-c589b0a4d611'::uuid, 'CY_MIAMI_ESTADOS_UNIDOS', 'miami-estados-unidos', 'Miami, Estados Unidos', 370),
                    ('5e08003a-2054-5956-af79-58b04a68f86e'::uuid, 'CY_NAVEGANTES_BRASIL', 'navegantes-brasil', 'Navegantes, Brasil', 380),
                    ('b4ee9df6-f0f4-50fd-9e3e-47be5ea65827'::uuid, 'CY_NEW_ORLEANS_ESTADOS_UNIDOS', 'new-orleans-estados-unidos', 'New Orleans, Estados Unidos', 390),
                    ('b7c757aa-58e8-57fa-8ec7-065715100d70'::uuid, 'CY_NEW_YORK_ESTADOS_UNIDOS', 'new-york-estados-unidos', 'New York, Estados Unidos', 400),
                    ('05d98ed8-a812-5adf-bda5-c0b3ca7fd195'::uuid, 'CY_NORFOLK_ESTADOS_UNIDOS', 'norfolk-estados-unidos', 'Norfolk, Estados Unidos', 410),
                    ('16ba80cc-97da-5289-b1f3-f42688404c85'::uuid, 'CY_OAKLAND_ESTADOS_UNIDOS', 'oakland-estados-unidos', 'Oakland, Estados Unidos', 420),
                    ('a14a5552-9686-587e-b0f9-9980bcf0cf5e'::uuid, 'CY_PARANAGUA_BRASIL', 'paranagua-brasil', 'Paranaguá, Brasil', 430),
                    ('d1e7a3b1-77b9-59e4-840b-d20b3dab4e5e'::uuid, 'CY_PECEM_BRASIL', 'pecem-brasil', 'Pecém, Brasil', 440),
                    ('a21de8f9-6312-5674-a65e-1ef391a93051'::uuid, 'CY_PHILADELPHIA_ESTADOS_UNIDOS', 'philadelphia-estados-unidos', 'Philadelphia, Estados Unidos', 450),
                    ('a5182ac7-7234-5300-abfd-ce80924f011e'::uuid, 'CY_PORT_EVERGLADES_ESTADOS_UNIDOS', 'port-everglades-estados-unidos', 'Port Everglades, Estados Unidos', 460),
                    ('32356928-e9d1-5c71-9165-c3cbd49b65d3'::uuid, 'CY_PORT_HUENEME_ESTADOS_UNIDOS', 'port-hueneme-estados-unidos', 'Port Hueneme, Estados Unidos', 470),
                    ('a4984d9d-626a-5853-a18c-06068fdbbd68'::uuid, 'CY_PORT_NEWARK_ELIZABETH_ESTADOS_UNIDOS', 'port-newark-elizabeth-estados-unidos', 'Port Newark–Elizabeth, Estados Unidos', 480),
                    ('de11636e-d765-5082-b977-33910624a319'::uuid, 'CY_LONG_BEACH_ESTADOS_UNIDOS', 'long-beach-estados-unidos', 'Long Beach, Estados Unidos', 490),
                    ('3e043420-8c70-57a2-8d55-39f3e8bb42f6'::uuid, 'CY_SAMSUN_TURQUIA', 'samsun-turquia', 'Samsun, Turquía', 500),
                    ('19a5ce22-340d-579f-ad27-f6517a2dde27'::uuid, 'CY_ACAJUTLA_EL_SALVADOR', 'acajutla-el-salvador', 'Acajutla, El Salvador', 510),
                    ('ddf4073d-1bd8-56c5-a0c6-e9319c330772'::uuid, 'CY_PUERTO_QUETZAL_GUATEMALA', 'puerto-quetzal-guatemala', 'Puerto Quetzal, Guatemala', 520),
                    ('fa274234-6848-594c-a920-e6255f060fa4'::uuid, 'CY_PUERTO_BARRIOS_GUATEMALA', 'puerto-barrios-guatemala', 'Puerto Barrios, Guatemala', 530),
                    ('2a1f3fa8-c8c3-54a4-8266-dfd9b0588d98'::uuid, 'CY_PUERTO_CALDERA_COSTA_RICA', 'puerto-caldera-costa-rica', 'Puerto Caldera, Costa Rica', 540),
                    ('4b405666-168c-5224-8723-99665606fdf6'::uuid, 'CY_PUERTO_LIMON_COSTA_RICA', 'puerto-limon-costa-rica', 'Puerto Limón, Costa Rica', 550),
                    ('b7428516-90e3-5621-86a9-0862bcac2e48'::uuid, 'CY_PUERTO_MOIN_COSTA_RICA', 'puerto-moin-costa-rica', 'Puerto Moín, Costa Rica', 560),
                    ('ac661cdc-f775-5a10-8964-0a032d71adcf'::uuid, 'CY_ITAJAI_BRASIL', 'itajai-brasil', 'Itajaí, Brasil', 570),
                    ('1b59f729-9cd2-5178-bf6e-9c3f75a500ce'::uuid, 'CY_RODMAN_PANAMA', 'rodman-panama', 'Rodman, Panamá', 580),
                    ('efcbd7b7-4311-5023-a3f4-bd0a789a9380'::uuid, 'CY_ROTTERDAM_PAISES_BAJOS', 'rotterdam-paises-bajos', 'Rotterdam, Países Bajos', 590),
                    ('34c96055-cf01-5ece-b4bc-08859733ad62'::uuid, 'CY_SAINT_JOHN_NB_CANADA', 'saint-john-nb-canada', 'Saint John NB, Canadá', 600),
                    ('27db0222-56f6-5df2-af3c-019e8dbb84c3'::uuid, 'CY_SAN_ANTONIO_CHILE', 'san-antonio-chile', 'San Antonio, Chile', 610),
                    ('661425c8-f7dd-5537-8c0e-e657c43ce2ce'::uuid, 'CY_SANTOS_BRASIL', 'santos-brasil', 'Santos, Brasil', 620),
                    ('f974fe68-c885-516e-b8b8-a3bcffceee96'::uuid, 'CY_SAVANNAH_GA_ESTADOS_UNIDOS', 'savannah-ga-estados-unidos', 'Savannah GA, Estados Unidos', 630),
                    ('2d3ec3b8-69ef-5e17-bd5b-52fe62f74244'::uuid, 'CY_TAMPA_ESTADOS_UNIDOS', 'tampa-estados-unidos', 'Tampa, Estados Unidos', 640),
                    ('0e895051-0a1c-51f3-b5a5-52940d780975'::uuid, 'CY_TANJUNG_PRIOK_INDONESIA', 'tanjung-priok-indonesia', 'Tanjung Priok, Indonesia', 650),
                    ('02734cb3-8375-5db3-ab7b-33cd6c8e795e'::uuid, 'CY_TARRAGONA_ESPANA', 'tarragona-espana', 'Tarragona, España', 660),
                    ('5e879020-e5f6-50e8-87d2-beb2cf84d235'::uuid, 'CY_VADO_LIGURE_ITALIA', 'vado-ligure-italia', 'Vado Ligure, Italia', 670),
                    ('a4bd3083-e900-5660-a6d1-419771408eec'::uuid, 'CY_VLISSINGEN_PAISES_BAJOS', 'vlissingen-paises-bajos', 'Vlissingen, Países Bajos', 680),
                    ('e682c91d-3cc7-51d1-878b-14c7674b050e'::uuid, 'CY_VALENCIA_ESPANA', 'valencia-espana', 'Valencia, España', 690),
                    ('7c17d922-ef20-529e-a328-7a146e82bed6'::uuid, 'CY_VALPARAISO_CHILE', 'valparaiso-chile', 'Valparaíso, Chile', 700),
                    ('ab7a30a5-1368-5f61-b9b2-dc70896c1c25'::uuid, 'CY_VANCOUVER_CANADA', 'vancouver-canada', 'Vancouver, Canadá', 710),
                    ('e16829e3-d820-5275-a5f9-465891651646'::uuid, 'CY_VERACRUZ_MEXICO', 'veracruz-mexico', 'Veracruz, México', 720),
                    ('a2c54a48-053e-56bc-a255-2c98a8f6ae23'::uuid, 'CY_WILMINGTON_ESTADOS_UNIDOS', 'wilmington-estados-unidos', 'Wilmington, Estados Unidos', 730)
            )
            INSERT INTO config."CatalogItems"
                (id, catalog_group_id, code, slug, name, description, value, metadata_json, sort_order, is_system, is_active, created_at_utc, created_by, is_deleted)
            SELECT
                CASE
                    WHEN g.slug = 'pol' THEN d.id
                    ELSE (
                        substr(md5(d.id::text || ':poe'), 1, 8) || '-' ||
                        substr(md5(d.id::text || ':poe'), 9, 4) || '-' ||
                        '4' || substr(md5(d.id::text || ':poe'), 14, 3) || '-' ||
                        '8' || substr(md5(d.id::text || ':poe'), 18, 3) || '-' ||
                        substr(md5(d.id::text || ':poe'), 21, 12)
                    )::uuid
                END,
                g.id,
                d.code || CASE WHEN g.slug = 'poe' THEN '_POE' ELSE '' END,
                d.slug || CASE WHEN g.slug = 'poe' THEN '-poe' ELSE '' END,
                d.value,
                'Puerto marítimo de carga internacional · Container Yard (CY).',
                d.value,
                '{"terminalType":"CY","transportMode":"Maritime","internationalCargo":true,"source":"REVISADO Listado_Puertos_Pais_Ancla_CY.xlsx"}'::jsonb,
                d.sort_order,
                FALSE,
                TRUE,
                NOW(),
                'migration-cy-ports-20260828',
                FALSE
            FROM config."CatalogGroups" g
            CROSS JOIN desired d
            WHERE g.slug IN ('pol', 'poe')
              AND g.is_deleted = FALSE
              AND NOT EXISTS (
                  SELECT 1
                  FROM config."CatalogItems" i
                  WHERE i.catalog_group_id = g.id
                    AND i.is_deleted = FALSE
                    AND (
                        LOWER(TRIM(COALESCE(i.value, ''))) = LOWER(d.value)
                        OR LOWER(TRIM(COALESCE(i.name, ''))) = LOWER(d.value)
                        OR LOWER(TRIM(regexp_replace(COALESCE(i.value, ''), '\s*⚓?\s*\(CY\)\s*$', '', 'i'))) = LOWER(d.value)
                        OR LOWER(TRIM(regexp_replace(COALESCE(i.name, ''), '\s*⚓?\s*\(CY\)\s*$', '', 'i'))) = LOWER(d.value)
                    )
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
            WHERE created_by = 'migration-cy-ports-20260828';
            """
        );
    }
}
