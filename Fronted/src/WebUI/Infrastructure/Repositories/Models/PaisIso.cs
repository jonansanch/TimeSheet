using System.Globalization;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

public sealed record PaisIso(string Codigo, string Nombre)
{
    public string Bandera => string.Concat(Codigo.ToUpperInvariant().Select(
        letra => char.ConvertFromUtf32(0x1F1E6 + letra - 'A')));
}

public static class PaisesIso
{
    private const string Todos = """
        AD AE AF AG AI AL AM AO AQ AR AS AT AU AW AX AZ
        BA BB BD BE BF BG BH BI BJ BL BM BN BO BQ BR BS BT BV BW BY BZ
        CA CC CD CF CG CH CI CK CL CM CN CO CR CU CV CW CX CY CZ
        DE DJ DK DM DO DZ EC EE EG EH ER ES ET FI FJ FK FM FO FR
        GA GB GD GE GF GG GH GI GL GM GN GP GQ GR GS GT GU GW GY
        HK HM HN HR HT HU ID IE IL IM IN IO IQ IR IS IT JE JM JO JP
        KE KG KH KI KM KN KP KR KW KY KZ LA LB LC LI LK LR LS LT LU LV LY
        MA MC MD ME MF MG MH MK ML MM MN MO MP MQ MR MS MT MU MV MW MX MY MZ
        NA NC NE NF NG NI NL NO NP NR NU NZ OM PA PE PF PG PH PK PL PM PN PR PS PT PW PY
        QA RE RO RS RU RW SA SB SC SD SE SG SH SI SJ SK SL SM SN SO SR SS ST SV SX SY SZ
        TC TD TF TG TH TJ TK TL TM TN TO TR TT TV TW TZ UA UG UM US UY UZ
        VA VC VE VG VI VN VU WF WS YE YT ZA ZM ZW
        """;

    private static readonly Lazy<IReadOnlyList<PaisIso>> Paises = new(Crear);

    public static IReadOnlyList<PaisIso> Obtener() => Paises.Value;

    private static IReadOnlyList<PaisIso> Crear()
    {
        var nombres = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            .Select(c =>
            {
                try
                {
                    var region = new RegionInfo(c.Name);
                    return new { Codigo = region.TwoLetterISORegionName.ToUpperInvariant(), region.DisplayName };
                }
                catch (ArgumentException) { return null; }
            })
            .Where(p => p is not null)
            .Select(p => p!)
            .GroupBy(p => p.Codigo, StringComparer.Ordinal)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(p => p.DisplayName, StringComparer.CurrentCultureIgnoreCase).First().DisplayName,
                StringComparer.Ordinal);

        return Todos.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Select(codigo => new PaisIso(codigo, nombres.GetValueOrDefault(codigo) ?? NombreFallback(codigo)))
            .OrderBy(p => p.Nombre, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private static string NombreFallback(string codigo) =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "es"
            ? $"País {codigo}"
            : $"Country {codigo}";

    public static string? Nombre(string? codigo) =>
        codigo is null ? null : Paises.Value.FirstOrDefault(p => p.Codigo == codigo.ToUpperInvariant())?.Nombre;

    public static string? Bandera(string? codigo) =>
        codigo is null ? null : Paises.Value.FirstOrDefault(p => p.Codigo == codigo.ToUpperInvariant())?.Bandera;
}
