using ClosedXML.Excel.Drawings;

namespace KPG.Timesheet.Infrastructure.Reportes;

/// <summary>Decodifica el logo parametrizado (guardado como data URI) para imprimirlo en los reportes.</summary>
public static class LogoDataUri
{
    public static byte[]? DecodeBytes(string? dataUri)
    {
        if (string.IsNullOrWhiteSpace(dataUri)) return null;

        var separador = dataUri.IndexOf(',');
        if (separador < 0) return null;

        try
        {
            return Convert.FromBase64String(dataUri[(separador + 1)..]);
        }
        catch (FormatException)
        {
            return null;
        }
    }

    public static XLPictureFormat? DecodeFormat(string? dataUri)
    {
        if (string.IsNullOrWhiteSpace(dataUri)) return null;

        if (dataUri.StartsWith("data:image/png", StringComparison.OrdinalIgnoreCase))
            return XLPictureFormat.Png;
        if (dataUri.StartsWith("data:image/jpeg", StringComparison.OrdinalIgnoreCase) ||
            dataUri.StartsWith("data:image/jpg", StringComparison.OrdinalIgnoreCase))
            return XLPictureFormat.Jpeg;

        return null;
    }
}
