using System.Globalization;
using System.Text;

namespace KPG.Timesheet.Domain.Common;

/// <summary>
/// Normaliza texto para comparar terminos de forma tolerante a mayusculas, tildes y
/// espacios extra. Lo usan <see cref="Entities.TerminoDescripcion"/> (para el indice unico
/// de <c>TerminoNormalizado</c>) y el evaluador de calidad de descripciones, asi ambos
/// coinciden siempre en el mismo criterio.
/// </summary>
public static class NormalizadorTexto
{
    /// <summary>
    /// Minusculas, sin tildes/diacriticos y con los espacios colapsados y recortados.
    /// "  Reunión  " y "reunion" normalizan igual.
    /// </summary>
    public static string Normalizar(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return string.Empty;

        var descompuesto = texto.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sinTildes = new StringBuilder(descompuesto.Length);
        foreach (var c in descompuesto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sinTildes.Append(c);
        }

        return string.Join(' ', sinTildes.ToString().Normalize(NormalizationForm.FormC)
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}
