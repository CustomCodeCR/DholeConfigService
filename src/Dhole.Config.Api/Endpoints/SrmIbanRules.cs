using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Dhole.Config.Api.Endpoints;

/// <summary>
/// ISO 13616 IBAN lengths and MOD-97 control digits by bank country.
/// Countries without an IBAN standard keep their national account numbers.
/// Financial data is not logged, broadcast, or stored in catalog metadata.
/// </summary>
public static class SrmIbanRules
{
    private static readonly IReadOnlyDictionary<string, int> Lengths = new Dictionary<string, int>(
        StringComparer.OrdinalIgnoreCase)
    {
        ["AL"] = 28,
        ["AD"] = 24,
        ["AT"] = 20,
        ["AZ"] = 28,
        ["BH"] = 22,
        ["BY"] = 28,
        ["BE"] = 16,
        ["BA"] = 20,
        ["BR"] = 29,
        ["VG"] = 24,
        ["BG"] = 22,
        ["BI"] = 27,
        ["CR"] = 22,
        ["HR"] = 21,
        ["CY"] = 28,
        ["CZ"] = 24,
        ["DK"] = 18,
        ["DJ"] = 27,
        ["DO"] = 28,
        ["EG"] = 29,
        ["SV"] = 28,
        ["EE"] = 20,
        ["FK"] = 18,
        ["FO"] = 18,
        ["FI"] = 18,
        ["FR"] = 27,
        ["GE"] = 22,
        ["DE"] = 22,
        ["GI"] = 23,
        ["GR"] = 27,
        ["GL"] = 18,
        ["GT"] = 28,
        ["VA"] = 22,
        ["HN"] = 28,
        ["HU"] = 28,
        ["IS"] = 26,
        ["IQ"] = 23,
        ["IE"] = 22,
        ["IL"] = 23,
        ["IT"] = 27,
        ["JO"] = 30,
        ["KZ"] = 20,
        ["XK"] = 20,
        ["KW"] = 30,
        ["LV"] = 21,
        ["LB"] = 28,
        ["LY"] = 25,
        ["LI"] = 21,
        ["LT"] = 20,
        ["LU"] = 20,
        ["MT"] = 31,
        ["MR"] = 27,
        ["MU"] = 30,
        ["MD"] = 24,
        ["MC"] = 27,
        ["MN"] = 20,
        ["ME"] = 22,
        ["NL"] = 18,
        ["NI"] = 28,
        ["MK"] = 19,
        ["NO"] = 15,
        ["OM"] = 23,
        ["PK"] = 24,
        ["PS"] = 29,
        ["PL"] = 28,
        ["PT"] = 25,
        ["QA"] = 29,
        ["RO"] = 24,
        ["RU"] = 33,
        ["LC"] = 32,
        ["SM"] = 27,
        ["ST"] = 25,
        ["SA"] = 24,
        ["RS"] = 22,
        ["SC"] = 31,
        ["SK"] = 24,
        ["SI"] = 19,
        ["SO"] = 23,
        ["ES"] = 24,
        ["SD"] = 18,
        ["SE"] = 24,
        ["CH"] = 21,
        ["TL"] = 23,
        ["TN"] = 24,
        ["TR"] = 26,
        ["UA"] = 29,
        ["AE"] = 23,
        ["GB"] = 22,
        ["YE"] = 30,
    };

    private static readonly IReadOnlyDictionary<string, string> CountryAliases = BuildCountryAliases();
    private static readonly Regex NationalAccount = new(@"^[A-Z0-9]{5,44}$", RegexOptions.Compiled);

    private static string NormalizeCountryName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";
        var decomposed = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var ch in decomposed)
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark &&
                char.IsLetterOrDigit(ch)) builder.Append(char.ToUpperInvariant(ch));
        return builder.ToString();
    }

    private static IReadOnlyDictionary<string, string> BuildCountryAliases()
    {
        var aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var iso2 in Lengths.Keys) aliases[iso2] = iso2;
        foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
        {
            try
            {
                var region = new RegionInfo(culture.Name);
                var iso2 = region.TwoLetterISORegionName;
                if (!Lengths.ContainsKey(iso2)) continue;
                foreach (var name in new[] {
                    region.ThreeLetterISORegionName, region.EnglishName,
                    region.DisplayName, region.NativeName })
                {
                    var normalized = NormalizeCountryName(name);
                    if (normalized.Length > 0) aliases[normalized] = iso2;
                }
            }
            catch (ArgumentException) { /* Invalid regional culture: ignore. */ }
        }
        foreach (var (name, iso2) in new Dictionary<string, string> {
            ["CRI"] = "CR", ["COSTARICA"] = "CR", ["GTM"] = "GT",
            ["SLV"] = "SV", ["ELSALVADOR"] = "SV", ["HND"] = "HN",
            ["NIC"] = "NI", ["DOM"] = "DO", ["REPUBLICADOMINICANA"] = "DO",
            ["BRA"] = "BR", ["BRASIL"] = "BR", ["ESP"] = "ES",
            ["DEU"] = "DE", ["ALEMANIA"] = "DE", ["GBR"] = "GB",
            ["REINOUNIDO"] = "GB", ["NLD"] = "NL", ["PAISESBAJOS"] = "NL",
            ["TURQUIA"] = "TR", ["TURKIYE"] = "TR", ["XKX"] = "XK",
        }) aliases[name] = iso2;
        return aliases;
    }

    public static bool TryGetFormat(
        string? code, string? value, string? slug, string? name,
        out string iso2, out int length)
    {
        foreach (var field in new[] { code, value, slug, name })
        {
            if (!CountryAliases.TryGetValue(NormalizeCountryName(field), out var resolved))
                continue;
            iso2 = resolved;
            length = Lengths[resolved];
            return true;
        }
        iso2 = "";
        length = 0;
        return false;
    }

    public static string NormalizeAccount(string? value) =>
        Regex.Replace((value ?? "").Trim().ToUpperInvariant(), @"[\s\-]", "");

    public static bool IsValid(string? number,
        string? code, string? value, string? slug, string? name)
    {
        var account = NormalizeAccount(number);
        if (!NationalAccount.IsMatch(account)) return false;
        if (!TryGetFormat(code, value, slug, name, out var iso2, out var length))
            return true; // E.g. US: routing/national account, not an ISO IBAN.
        if (account.Length != length || !account.StartsWith(iso2, StringComparison.Ordinal)
            || account.Length < 5 || !char.IsAsciiDigit(account[2]) ||
            !char.IsAsciiDigit(account[3]))
            return false;
        var shifted = account[4..] + account[..4];
        var remainder = 0;
        foreach (var ch in shifted)
        {
            if (ch is >= '0' and <= '9')
                remainder = (remainder * 10 + ch - '0') % 97;
            else if (ch is >= 'A' and <= 'Z')
            {
                var digits = ch - 'A' + 10;
                remainder = (remainder * 10 + digits / 10) % 97;
                remainder = (remainder * 10 + digits % 10) % 97;
            }
            else return false;
        }
        return remainder == 1;
    }
}
