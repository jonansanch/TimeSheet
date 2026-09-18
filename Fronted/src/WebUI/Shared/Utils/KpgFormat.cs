using System.Globalization;

namespace KPG.Timesheet.WebUI.Shared.Utils;

/// <summary>
/// Formato numerico acordado con el cliente: el punto es separador decimal y la coma
/// separador de miles (1,234.50), independientemente del idioma de la interfaz.
/// Sin esto, es-ES invierte ambos separadores y los totales se leen mal.
/// </summary>
public static class KpgFormat
{
    private static readonly CultureInfo Numerica = CultureInfo.InvariantCulture;

    /// <summary>Horas con separador de miles, p. ej. 1,234.5.</summary>
    public static string Horas(decimal valor, int decimales = 1) =>
        valor.ToString($"N{decimales}", Numerica);

    /// <summary>Cantidad entera con separador de miles, p. ej. 1,234.</summary>
    public static string Entero(int valor) => valor.ToString("N0", Numerica);

    /// <summary>Duracion en minutos expresada como h:mm.</summary>
    public static string HorasMinutos(int totalMinutos) =>
        $"{totalMinutos / 60}:{totalMinutos % 60:00}";
}
