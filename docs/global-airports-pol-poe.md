# Aeropuertos internacionales para Pricing aéreo LCL

## Alcance

- Los catálogos existentes **pol** y **poe** contienen el **mismo conjunto** de 850 aeropuertos internacionales y regionales (incluidos MIA, MAD y SJO preexistentes).
- Se usa `code = APT_<IATA>`, `terminalType=APT`, `transportMode=Air`, `routeRole=POL|POE`.
- `name` muestra `Ciudad, País (IATA)` y `value` muestra `Ciudad, País`. El nombre de la instalación aeroportuaria queda exclusivamente en `metadata_json.airportName`.
- Metadatos adicionales: códigos IATA/ICAO, país ISO, ciudad, coordenadas cuando están disponibles, categoría y origen.
- La migración **no elimina** puertos marítimos (CY), rutas terrestres (SD), datos modificados manualmente ni aeropuertos ya existentes.
- El selector de Config ahora devuelve hasta 2500 ubicaciones para `pol` y `poe`, sin cambiar el límite de 50 de otros grupos. El buscador de Pricing filtra localmente por código, ciudad y país.

## Procedencia y atribución

**FreeGeoDB**, copyright (c) delight.im <info@delight.im>, bajo [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/), archivo [Distribution/JSON/airports.json](https://github.com/delight-im/FreeGeoDB/blob/master/Distribution/JSON/airports.json).
Datos transformados: selección de aeropuertos con IATA, normalización de ciudad/país, creación de entradas simétricas para POL/POE, y adición manual de centros aéreos de carga actuales y aeropuertos costarricenses ausentes.

El conjunto es una referencia geográfica, **no** garantiza vuelos activos, disponibilidad de carga aérea ni rutas directas. Los administradores pueden gestionar los registros de cada catálogo en Config.

## Validación en PostgreSQL

Ejecutar después del despliegue de Config y sus migraciones:

```sql
SELECT g.slug, COUNT(*) AS airports, COUNT(DISTINCT i.metadata_json->>'iataCode') AS unique_iata
FROM config."CatalogItems" i
JOIN config."CatalogGroups" g ON g.id = i.catalog_group_id
WHERE g.slug IN ('pol', 'poe') AND NOT i.is_deleted AND i.is_active
  AND i.metadata_json->>'terminalType' = 'APT'
GROUP BY g.slug ORDER BY g.slug;

WITH airports AS (
  SELECT g.slug, i.metadata_json->>'iataCode' AS iata
  FROM config."CatalogItems" i JOIN config."CatalogGroups" g ON g.id = i.catalog_group_id
  WHERE g.slug IN ('pol', 'poe') AND NOT i.is_deleted AND i.is_active
    AND i.metadata_json->>'terminalType' = 'APT'
)
SELECT COALESCE(pol.iata, poe.iata) AS missing_iata,
       CASE WHEN pol.iata IS NULL THEN 'POL' ELSE 'POE' END AS missing_from
FROM (SELECT DISTINCT iata FROM airports WHERE slug = 'pol') pol
FULL JOIN (SELECT DISTINCT iata FROM airports WHERE slug = 'poe') poe
  USING (iata)
WHERE pol.iata IS NULL OR poe.iata IS NULL;

SELECT g.slug, i.code, i.name, i.value, i.metadata_json->>'countryCode' AS country
FROM config."CatalogItems" i JOIN config."CatalogGroups" g ON g.id = i.catalog_group_id
WHERE g.slug IN ('pol', 'poe') AND i.code IN
 ('APT_MIA', 'APT_SJO', 'APT_LIR', 'APT_BKK', 'APT_HND', 'APT_MAD')
ORDER BY g.slug, i.code;
```

Un número menor de altas nuevas puede ser normal si ya había aeropuertos con esos IATA en el entorno. Si la comparación de conjuntos arroja diferencias, revisar los registros manuales o colisiones de unicidad antes de corregirlos. El método `Down` es intencionadamente no destructivo para conservar referencias de Pricing.
