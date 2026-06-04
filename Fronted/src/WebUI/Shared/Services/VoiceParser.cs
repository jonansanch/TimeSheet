using System.Text;
using System.Text.RegularExpressions;
using KPG.Timesheet.WebUI.Shared.Models;

namespace KPG.Timesheet.WebUI.Shared.Services;

public class VoiceParser
{
    // Patrón "de X a Y"
    private static readonly Regex TimeRangeRegex = new(
        @"de\s+(\d{1,2})(?::(\d{2}))?\s+a\s+(\d{1,2})(?::(\d{2}))?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Patrón "entrada X salida Y"
    private static readonly Regex EntradaSalidaRegex = new(
        @"entrada\s+(\d{1,2})(?::(\d{2}))?\s+salida\s+(\d{1,2})(?::(\d{2}))?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Patrón para una sola hora: "entrada X" o "salida X"
    private static readonly Regex SingleTimeRegex = new(
        @"(entrada|salida)\s+(\d{1,2})(?::(\d{2}))?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Números en palabras → dígitos
    private static readonly Dictionary<string, string> NumberWords = new(StringComparer.OrdinalIgnoreCase)
    {
        ["uno"] = "1",  ["una"] = "1",
        ["dos"] = "2",
        ["tres"] = "3",
        ["cuatro"] = "4",
        ["cinco"] = "5",
        ["seis"] = "6",
        ["siete"] = "7",
        ["ocho"] = "8",
        ["nueve"] = "9",
        ["diez"] = "10",
        ["once"] = "11",
        ["doce"] = "12",
        ["trece"] = "13",
        ["catorce"] = "14",
        ["quince"] = "15",
        ["dieciseis"] = "16",
        ["diecisiete"] = "17",
        ["dieciocho"] = "18",
        ["diecinueve"] = "19",
        ["veinte"] = "20",
        ["veintiuno"] = "21",  ["veintiuna"] = "21",
        ["veintidos"] = "22",
        ["veintitres"] = "23",
        ["veinticuatro"] = "24",
        ["veinticinco"] = "25",
        ["veintiseis"] = "26",
        ["veintisiete"] = "27",
        ["veintiocho"] = "28",
        ["veintinueve"] = "29",
        ["treinta y uno"] = "31", ["treinta y una"] = "31",
        ["treinta"] = "30",
        ["primero"] = "1",  ["primer"] = "1",
    };

    // [día] de [mes] [del/de] [año]  — opera sobre texto ya normalizado (sin acentos, números en palabras → dígitos)
    private static readonly Regex FechaExplicitaRegex = new(
        @"(\d{1,2})\s+de\s+(enero|febrero|marzo|abril|mayo|junio|julio|agosto|septiembre|setiembre|octubre|noviembre|diciembre)(?:\s+del?\s+(\d{4}))?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Dictionary<string, int> Meses = new(StringComparer.OrdinalIgnoreCase)
    {
        ["enero"] = 1,  ["febrero"] = 2,  ["marzo"] = 3,    ["abril"] = 4,
        ["mayo"]  = 5,  ["junio"]   = 6,  ["julio"]  = 7,   ["agosto"] = 8,
        ["septiembre"] = 9, ["setiembre"] = 9,
        ["octubre"] = 10, ["noviembre"] = 11, ["diciembre"] = 12,
    };

    public VoiceParseResult Parse(string transcript, VoiceCatalog catalogo)
    {
        var result = new VoiceParseResult();
        var normalized = NormalizeWords(Normalize(transcript));

        result.Fecha       = ExtractFecha(normalized);
        ExtractHoras(normalized, result);
        (result.Cliente, result.Proyecto) = ExtractClienteYProyecto(normalized, catalogo);
        result.Modalidad   = ExtractModalidad(normalized, catalogo.Modalidades);
        result.Recurso     = ExtractRecurso(normalized, catalogo.Recursos);
        result.Lugar       = ExtractLugar(normalized, catalogo.Lugares);
        result.Descripcion = ExtractDescripcion(transcript);

        result.CamposNoDetectados = ResolverNoDetectados(result);
        return result;
    }

    // ── Normalización ──────────────────────────────────────────────────────────

    internal static string Normalize(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;
        var sb = new StringBuilder(s.ToLowerInvariant());
        sb.Replace('á', 'a').Replace('à', 'a').Replace('ä', 'a')
          .Replace('é', 'e').Replace('è', 'e').Replace('ë', 'e')
          .Replace('í', 'i').Replace('ì', 'i').Replace('ï', 'i')
          .Replace('ó', 'o').Replace('ò', 'o').Replace('ö', 'o')
          .Replace('ú', 'u').Replace('ù', 'u').Replace('ü', 'u')
          .Replace('ñ', 'n');
        return sb.ToString();
    }

    // Convierte palabras a su forma canónica y números en palabras a dígitos
    private static string NormalizeWords(string normalized)
    {
        // Variaciones de "entrada" y "salida"
        normalized = Regex.Replace(normalized, @"\bentrad[ao]\b", "entrada");
        normalized = Regex.Replace(normalized, @"\bsalid[ao]\b",  "salida");
        normalized = Regex.Replace(normalized, @"\bentr[ao]\b",   "entrada");

        // Números en palabras → dígitos (orden de mayor a menor para "dieciseis" antes de "seis")
        foreach (var (word, digit) in NumberWords.OrderByDescending(kv => kv.Key.Length))
            normalized = Regex.Replace(normalized, $@"\b{Regex.Escape(word)}\b", digit);

        return normalized;
    }

    // ── Fecha ──────────────────────────────────────────────────────────────────

    private static DateOnly? ExtractFecha(string normalized)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        if (normalized.Contains("hoy"))                                          return hoy;
        if (normalized.Contains("ayer"))                                         return hoy.AddDays(-1);
        if (normalized.Contains("anteayer") || normalized.Contains("antier"))   return hoy.AddDays(-2);

        var m = FechaExplicitaRegex.Match(normalized);
        if (m.Success &&
            int.TryParse(m.Groups[1].Value, out int dia) &&
            Meses.TryGetValue(m.Groups[2].Value, out int mes))
        {
            int anio = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : hoy.Year;
            try { return new DateOnly(anio, mes, dia); } catch { }
        }

        return null;
    }

    // ── Horas ──────────────────────────────────────────────────────────────────

    private static void ExtractHoras(string normalized, VoiceParseResult result)
    {
        bool hayTarde = normalized.Contains("tarde") || normalized.Contains(" pm");
        bool hayManana = normalized.Contains("manana") || normalized.Contains(" am");

        // Intento 1: patrón "de X a Y" (puede haber dos pares)
        var rangeMatches = TimeRangeRegex.Matches(normalized);
        if (rangeMatches.Count > 0)
        {
            AplicarPares(rangeMatches, normalized, result);
            return;
        }

        // Intento 2: patrón "entrada X salida Y"
        var esMatcher = EntradaSalidaRegex.Match(normalized);
        if (esMatcher.Success)
        {
            int hEnt = int.Parse(esMatcher.Groups[1].Value);
            int mEnt = esMatcher.Groups[2].Success ? int.Parse(esMatcher.Groups[2].Value) : 0;
            int hSal = int.Parse(esMatcher.Groups[3].Value);
            int mSal = esMatcher.Groups[4].Success ? int.Parse(esMatcher.Groups[4].Value) : 0;

            bool esPM = hayTarde && !hayManana;
            if (esPM) { if (hEnt < 8) hEnt += 12; if (hSal < 8) hSal += 12; }

            var entrada = new TimeOnly(hEnt, mEnt);
            var salida  = new TimeOnly(hSal, mSal);
            if (esPM) { result.HoraEntradaPM = entrada; result.HoraSalidaPM = salida; }
            else      { result.HoraEntradaAM = entrada; result.HoraSalidaAM = salida; }
            return;
        }

        // Intento 3: horas sueltas "entrada X" y/o "salida X"
        var singles = SingleTimeRegex.Matches(normalized);
        foreach (Match m in singles)
        {
            string tipo = m.Groups[1].Value;
            int h = int.Parse(m.Groups[2].Value);
            int min = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : 0;

            // Contexto local para AM/PM
            int start  = Math.Max(0, m.Index - 30);
            int length = Math.Min(normalized.Length - start, m.Length + 60);
            string local = normalized.Substring(start, length);
            bool localTarde  = local.Contains("tarde") || local.Contains("pm");
            bool localManana = local.Contains("manana") || local.Contains("am");
            bool esPM = localTarde && !localManana;

            if (esPM && h < 8) h += 12;
            var tiempo = new TimeOnly(h, min);

            if (tipo == "entrada")
            {
                if (esPM) result.HoraEntradaPM = tiempo;
                else      result.HoraEntradaAM = tiempo;
            }
            else
            {
                if (esPM) result.HoraSalidaPM = tiempo;
                else      result.HoraSalidaAM = tiempo;
            }
        }
    }

    private static void AplicarPares(MatchCollection matches, string normalized, VoiceParseResult result)
    {
        for (int i = 0; i < matches.Count && i < 2; i++)
        {
            var m = matches[i];
            int hEnt = int.Parse(m.Groups[1].Value);
            int mEnt = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 0;
            int hSal = int.Parse(m.Groups[3].Value);
            int mSal = m.Groups[4].Success ? int.Parse(m.Groups[4].Value) : 0;

            int start  = Math.Max(0, m.Index - 40);
            int length = Math.Min(normalized.Length - start, m.Length + 80);
            string local = normalized.Substring(start, length);

            bool esManana = local.Contains("manana") || local.Contains(" am");
            bool esTarde  = local.Contains("tarde")  || local.Contains(" pm");

            bool asignarAM = i == 0 ? (!esTarde || esManana) : (esManana && !esTarde);

            if (!asignarAM) { if (hEnt < 8) hEnt += 12; if (hSal < 8) hSal += 12; }

            var entrada = new TimeOnly(hEnt, mEnt);
            var salida  = new TimeOnly(hSal, mSal);

            if (asignarAM) { result.HoraEntradaAM = entrada; result.HoraSalidaAM = salida; }
            else           { result.HoraEntradaPM = entrada; result.HoraSalidaPM = salida; }
        }
    }

    // ── Catálogo — helpers ─────────────────────────────────────────────────────

    private static string? ExtractDesdeListaMasLargo(string normalized, List<string> opciones)
    {
        string? encontrado = null;
        int maxLen = 0;
        foreach (var opcion in opciones)
        {
            var normOpcion = Normalize(opcion);
            if (normOpcion.Length > maxLen && normalized.Contains(normOpcion))
            {
                encontrado = opcion;
                maxLen     = normOpcion.Length;
            }
        }
        return encontrado;
    }

    private static (string? cliente, string? proyecto) ExtractClienteYProyecto(string normalized, VoiceCatalog catalogo)
    {
        // Paso 1: detectar cliente explícito en el transcript
        var cliente = ExtractDesdeListaMasLargo(normalized, catalogo.Clientes);

        // Paso 2: si hay cliente, buscar proyecto dentro de ese cliente
        if (cliente is not null && catalogo.ProyectosPorCliente.TryGetValue(cliente, out var proyectosCliente))
        {
            var proyecto = ExtractDesdeListaMasLargo(normalized, proyectosCliente);
            if (proyecto is not null) return (cliente, proyecto);
        }

        // Paso 3: buscar proyecto en TODOS los clientes (resuelve el caso donde el
        // cliente no se detectó pero el proyecto sí está en el transcript)
        foreach (var (c, proyectos) in catalogo.ProyectosPorCliente)
        {
            var proyecto = ExtractDesdeListaMasLargo(normalized, proyectos);
            if (proyecto is not null)
                return (cliente ?? c, proyecto); // inferir cliente si no fue detectado
        }

        return (cliente, null);
    }

    private static string? ExtractModalidad(string normalized, List<string> modalidades)
    {
        var match = ExtractDesdeListaMasLargo(normalized, modalidades);
        if (match is not null) return match;

        if (normalized.Contains("hibrido"))    return modalidades.FirstOrDefault(m => Normalize(m).Contains("hibrido"));
        if (normalized.Contains("remoto"))     return modalidades.FirstOrDefault(m => Normalize(m).Contains("remoto"));
        if (normalized.Contains("presencial")) return modalidades.FirstOrDefault(m => Normalize(m).Contains("presencial"));
        return null;
    }

    private static string? ExtractRecurso(string normalized, List<string> recursos)
    {
        var ordenados = recursos.OrderByDescending(r => r.Length).ToList();
        return ExtractDesdeListaMasLargo(normalized, ordenados);
    }

    private static string? ExtractLugar(string normalized, List<string> lugares)
    {
        var match = ExtractDesdeListaMasLargo(normalized, lugares);
        if (match is not null) return match;

        if (normalized.Contains("viaje"))      return lugares.FirstOrDefault(l => Normalize(l).Contains("viaje"));
        if (normalized.Contains("oficina"))    return lugares.FirstOrDefault(l => Normalize(l).Contains("oficina"));
        if (normalized.Contains("cliente"))    return lugares.FirstOrDefault(l => Normalize(l).Contains("cliente"));
        if (normalized.Contains("remoto"))     return lugares.FirstOrDefault(l => Normalize(l).Contains("remoto"));
        return null;
    }

    // ── Descripción ────────────────────────────────────────────────────────────

    private static string? ExtractDescripcion(string transcript)
    {
        var lower = transcript.ToLowerInvariant();
        foreach (var marcador in new[] { "descripción:", "descripcion:", "descripción ", "descripcion " })
        {
            int idx = lower.IndexOf(marcador, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                var texto = transcript[(idx + marcador.Length)..].Trim();
                return string.IsNullOrWhiteSpace(texto) ? null : texto;
            }
        }
        return null;
    }

    // ── Campos no detectados ───────────────────────────────────────────────────

    private static List<string> ResolverNoDetectados(VoiceParseResult r)
    {
        var faltantes = new List<string>();
        if (r.Fecha is null)                                     faltantes.Add("Fecha");
        if (r.HoraEntradaAM is null && r.HoraEntradaPM is null) faltantes.Add("Horas");
        if (r.Cliente is null)                                   faltantes.Add("Cliente");
        if (r.Proyecto is null)                                  faltantes.Add("Proyecto");
        if (r.Modalidad is null)                                 faltantes.Add("Modalidad");
        if (r.Recurso is null)                                   faltantes.Add("Recurso");
        if (r.Lugar is null)                                     faltantes.Add("Lugar");
        if (string.IsNullOrWhiteSpace(r.Descripcion))            faltantes.Add("Descripción");
        return faltantes;
    }
}
