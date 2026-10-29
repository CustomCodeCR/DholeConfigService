using System.Globalization;
using System.Text.RegularExpressions;
using Dhole.Config.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Dhole.Config.Persistence.Migrations;

/// <summary>
/// Adds the internationally recognized regions available from .NET/ICU, without
/// replacing or deleting existing country IDs already referenced by CRM/Pricing.
/// </summary>
[DbContext(typeof(ServiceDbContext))]
[Migration("20261029220000_CompleteCountriesForCrm")]
public sealed class CompleteCountriesForCrm : Migration
{
    private static readonly IReadOnlyDictionary<string, string> SpanishNames =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["CR"] = "Costa Rica", ["PA"] = "Panamá", ["GT"] = "Guatemala",
            ["SV"] = "El Salvador", ["HN"] = "Honduras", ["NI"] = "Nicaragua",
            ["BZ"] = "Belice", ["MX"] = "México", ["US"] = "Estados Unidos",
            ["CA"] = "Canadá", ["CN"] = "China", ["TW"] = "Taiwán",
            ["HK"] = "Hong Kong", ["JP"] = "Japón", ["KR"] = "Corea del Sur",
            ["IN"] = "India", ["DE"] = "Alemania", ["ES"] = "España",
            ["FR"] = "Francia", ["IT"] = "Italia", ["NL"] = "Países Bajos",
            ["GB"] = "Reino Unido", ["BR"] = "Brasil", ["CO"] = "Colombia",
            ["PE"] = "Perú", ["CL"] = "Chile", ["AR"] = "Argentina",
            ["UY"] = "Uruguay", ["PY"] = "Paraguay", ["BO"] = "Bolivia",
            ["EC"] = "Ecuador", ["VE"] = "Venezuela", ["DO"] = "República Dominicana",
            ["CU"] = "Cuba", ["HT"] = "Haití", ["PR"] = "Puerto Rico",
            ["JM"] = "Jamaica", ["TT"] = "Trinidad y Tobago", ["GY"] = "Guyana",
            ["SR"] = "Surinam", ["ZA"] = "Sudáfrica", ["AU"] = "Australia",
            ["NZ"] = "Nueva Zelanda", ["AE"] = "Emiratos Árabes Unidos",
            ["SA"] = "Arabia Saudita", ["IL"] = "Israel", ["EG"] = "Egipto",
            ["TR"] = "Turquía", ["RU"] = "Rusia", ["UA"] = "Ucrania",
            ["PL"] = "Polonia", ["PT"] = "Portugal", ["CH"] = "Suiza",
            ["AT"] = "Austria", ["BE"] = "Bélgica", ["SE"] = "Suecia",
            ["NO"] = "Noruega", ["FI"] = "Finlandia", ["DK"] = "Dinamarca",
            ["IE"] = "Irlanda", ["GR"] = "Grecia", ["CZ"] = "Chequia",
            ["HU"] = "Hungría", ["RO"] = "Rumania", ["SG"] = "Singapur",
            ["MY"] = "Malasia", ["TH"] = "Tailandia", ["VN"] = "Vietnam",
            ["ID"] = "Indonesia", ["PH"] = "Filipinas",
        };

    private static string Quote(string value) => "'" + value.Replace("'", "''") + "'";

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
INSERT INTO config."CatalogGroups" (id,code,slug,name,description,metadata_json,is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),'COUNTRIES','countries','Países','Países y territorios ISO para CRM y logística',
       '{"iso":"3166-1","editable":true}'::jsonb,true,true,now(),'migration',false
WHERE NOT EXISTS (SELECT 1 FROM config."CatalogGroups" WHERE slug='countries' AND is_deleted=false);
""");

        // .NET 10 ICU data supplies a comprehensive region index at migration time.
        // Existing catalog entries retain their UUID, name, activation and references.
        var countries = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            .Select(c =>
            {
                try { return new RegionInfo(c.Name); }
                catch (ArgumentException) { return null; }
            })
            .Where(r => r is not null && Regex.IsMatch(r.TwoLetterISORegionName, "^[A-Z]{2}$")
                && r.TwoLetterISORegionName is not ("ZZ" or "EU"))
            .Select(r => r!)
            .GroupBy(r => r.TwoLetterISORegionName, StringComparer.OrdinalIgnoreCase)
            .Select(g => (Code: g.Key.ToUpperInvariant(),
                Name: SpanishNames.TryGetValue(g.Key, out var spanish)
                    ? spanish : g.First().EnglishName))
            .OrderBy(x => x.Name, StringComparer.Ordinal)
            .ToArray();

        if (countries.Length < 180)
            throw new InvalidOperationException("The runtime country/region index is unexpectedly incomplete.");

        var rows = string.Join(",\n", countries.Select((x, index) =>
            $"({Quote(x.Code)}, {Quote(x.Code.ToLowerInvariant())}, {Quote(x.Name)}, {(index + 1) * 10})"));

        migrationBuilder.Sql($"""
WITH desired(code,slug,name,sort_order) AS (
    VALUES
    {rows}
)
INSERT INTO config."CatalogItems"
    (id,catalog_group_id,code,slug,name,description,value,metadata_json,sort_order,
     is_system,is_active,created_at_utc,created_by,is_deleted)
SELECT gen_random_uuid(),g.id,d.code,d.slug,d.name,'País o territorio ISO',d.code,
       jsonb_build_object('iso2',d.code),d.sort_order,true,true,now(),'migration',false
FROM config."CatalogGroups" g CROSS JOIN desired d
WHERE g.slug='countries' AND g.is_deleted=false
 AND NOT EXISTS (
    SELECT 1 FROM config."CatalogItems" existing
    WHERE existing.catalog_group_id=g.id AND (
        upper(existing.code)=d.code
        OR lower(existing.slug)=d.slug
        OR lower(existing.name)=lower(d.name)
    )
 )
ON CONFLICT DO NOTHING;
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Never remove country rows; customers, contacts and quotations hold their IDs.
    }
}
