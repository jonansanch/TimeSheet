using System.Globalization;
using ClosedXML.Excel;
using KPG.Timesheet.Application.Common.Interfaces;

namespace KPG.Timesheet.Infrastructure.Reportes;

/// <summary>
/// Lee la plantilla de timesheet de los consultores.
///
/// <para>
/// El formato no es una tabla limpia: el consultor y el mes viven en la cabecera, la fecha
/// es solo el numero de dia, y la banda de datos trae filas vacias con formulas. Por eso
/// se localizan las cosas por su rotulo en lugar de por coordenadas fijas: una plantilla
/// con una fila de mas arriba sigue funcionando.
/// </para>
/// </summary>
public class TimesheetImportParser : ITimesheetImportParser
{
    private const int MaxFilasCabecera = 20;
    private const int MaxFilasDatos    = 200;

    public TimesheetImportado Parse(Stream contenido)
    {
        using var wb = new XLWorkbook(contenido);

        var hoja = LocalizarHoja(wb)
            ?? throw new TimesheetImportFormatException(
                "El archivo no contiene una hoja de timesheet: no se encontro la fila de encabezados con 'Fecha' y 'Cliente'.");

        var filaEncabezado = LocalizarFilaEncabezado(hoja)!.Value;
        var columnas = MapearColumnas(hoja, filaEncabezado);

        ExigirColumnas(columnas);

        var (mes, anio) = LocalizarMesYAnio(hoja, filaEncabezado);
        var consultor   = LocalizarConsultor(hoja, filaEncabezado);

        if (mes is null || anio is null)
            throw new TimesheetImportFormatException(
                "No se pudo determinar el mes y el ano del timesheet: falta la celda 'Mes' en la cabecera.");

        var filas   = new List<FilaTimesheet>();
        var errores = new List<ErrorImportacion>();

        for (var r = filaEncabezado + 1; r <= filaEncabezado + MaxFilasDatos; r++)
        {
            var textoFila = hoja.Row(r).CellsUsed().Select(c => c.GetString()).ToList();

            // La fila de totales cierra la banda de datos.
            if (textoFila.Any(t => t.Contains("Total de horas", StringComparison.OrdinalIgnoreCase)))
                break;

            var dia = LeerEntero(hoja, r, columnas["fecha"]);
            var cliente = LeerTexto(hoja, r, columnas["cliente"]);

            // Fila vacia de la plantilla (trae formulas pero ningun dato): se ignora en silencio.
            if (dia is null && string.IsNullOrWhiteSpace(cliente))
                continue;

            if (dia is null)
            {
                errores.Add(new ErrorImportacion(r, "La fila tiene datos pero le falta el dia."));
                continue;
            }

            if (dia < 1 || dia > DateTime.DaysInMonth(anio.Value, mes.Value))
            {
                errores.Add(new ErrorImportacion(r, $"El dia {dia} no existe en el mes indicado."));
                continue;
            }

            var bloques = columnas["bloques"] as List<(int Entrada, int Salida)> ?? [];
            var horas = bloques
                .Select(b => (Entrada: LeerHora(hoja, r, b.Entrada), Salida: LeerHora(hoja, r, b.Salida)))
                .ToList();

            if (!horas.Any(h => h.Entrada.HasValue && h.Salida.HasValue))
            {
                errores.Add(new ErrorImportacion(r, "La fila no tiene ningun horario completo (entrada y salida)."));
                continue;
            }

            filas.Add(new FilaTimesheet(
                r,
                new DateOnly(anio.Value, mes.Value, dia.Value),
                Bloque(horas, 0).Entrada, Bloque(horas, 0).Salida,
                Bloque(horas, 1).Entrada, Bloque(horas, 1).Salida,
                Bloque(horas, 2).Entrada, Bloque(horas, 2).Salida,
                cliente.Trim(),
                LeerTexto(hoja, r, columnas["proyecto"]).Trim(),
                LeerTexto(hoja, r, columnas["modalidad"]).Trim(),
                LeerTexto(hoja, r, columnas["recurso"]).Trim(),
                LeerTexto(hoja, r, columnas["lugar"]).Trim(),
                LeerTexto(hoja, r, columnas["descripcion"]).Trim()));
        }

        return new TimesheetImportado(consultor, mes, anio, filas, errores);
    }

    private static (TimeOnly? Entrada, TimeOnly? Salida) Bloque(
        List<(TimeOnly? Entrada, TimeOnly? Salida)> horas, int indice) =>
        indice < horas.Count ? horas[indice] : (null, null);

    // ── Localizacion por rotulo ──────────────────────────────────────────────

    private static IXLWorksheet? LocalizarHoja(XLWorkbook wb) =>
        wb.Worksheets.FirstOrDefault(h => LocalizarFilaEncabezado(h) is not null);

    private static int? LocalizarFilaEncabezado(IXLWorksheet hoja)
    {
        for (var r = 1; r <= MaxFilasCabecera; r++)
        {
            var textos = hoja.Row(r).CellsUsed().Select(c => Normalizar(c.GetString())).ToList();
            if (textos.Contains("fecha") && textos.Contains("cliente"))
                return r;
        }
        return null;
    }

    /// <summary>
    /// Empareja encabezados con columnas. Las parejas Entrada/Salida se acumulan en orden,
    /// asi la misma logica sirve para plantillas de dos o de tres horarios.
    /// </summary>
    private static Dictionary<string, object> MapearColumnas(IXLWorksheet hoja, int filaEncabezado)
    {
        var columnas = new Dictionary<string, object>();
        var bloques  = new List<(int Entrada, int Salida)>();
        int? entradaPendiente = null;

        foreach (var celda in hoja.Row(filaEncabezado).CellsUsed())
        {
            var texto = Normalizar(celda.GetString());
            var col   = celda.Address.ColumnNumber;

            switch (texto)
            {
                case "fecha":       columnas["fecha"] = col; break;
                case "cliente":     columnas["cliente"] = col; break;
                case "proyecto":    columnas["proyecto"] = col; break;
                case "modalidad":   columnas["modalidad"] = col; break;
                case "recurso":     columnas["recurso"] = col; break;
                case "lugar":       columnas["lugar"] = col; break;
                case "entrada":     entradaPendiente = col; break;
                case "salida":
                    if (entradaPendiente is { } e) { bloques.Add((e, col)); entradaPendiente = null; }
                    break;
                default:
                    if (texto.StartsWith("descripcion")) columnas["descripcion"] = col;
                    break;
            }
        }

        columnas["bloques"] = bloques;
        return columnas;
    }

    private static void ExigirColumnas(Dictionary<string, object> columnas)
    {
        string[] obligatorias = ["fecha", "cliente", "proyecto", "modalidad", "recurso", "lugar", "descripcion"];
        var faltantes = obligatorias.Where(c => !columnas.ContainsKey(c)).ToList();

        if (faltantes.Count > 0)
            throw new TimesheetImportFormatException(
                $"Al encabezado del archivo le faltan columnas: {string.Join(", ", faltantes)}.");

        if (columnas["bloques"] is not List<(int, int)> { Count: > 0 })
            throw new TimesheetImportFormatException(
                "El encabezado no tiene ninguna pareja de columnas Entrada/Salida.");
    }

    /// <summary>Busca la celda rotulada "Mes" y toma el mes y el ano de las celdas a su derecha.</summary>
    private static (int? Mes, int? Anio) LocalizarMesYAnio(IXLWorksheet hoja, int filaEncabezado)
    {
        for (var r = 1; r < filaEncabezado; r++)
        {
            foreach (var celda in hoja.Row(r).CellsUsed())
            {
                if (Normalizar(celda.GetString()) != "mes") continue;

                int? mes = null, anio = null;
                for (var c = celda.Address.ColumnNumber + 1; c <= celda.Address.ColumnNumber + 8; c++)
                {
                    var valor = hoja.Cell(r, c);
                    if (valor.IsEmpty()) continue;

                    if (mes is null && ParsearMes(valor.GetString()) is { } m) { mes = m; continue; }
                    if (anio is null && int.TryParse(valor.GetString(), out var a) && a is >= 2000 and <= 2100)
                        anio = a;
                }
                if (mes is not null) return (mes, anio);
            }
        }
        return (null, null);
    }

    /// <summary>Acepta el nombre del mes en espanol o su numero.</summary>
    private static int? ParsearMes(string texto)
    {
        texto = texto.Trim();
        if (string.IsNullOrEmpty(texto)) return null;

        if (int.TryParse(texto, out var numero))
            return numero is >= 1 and <= 12 ? numero : null;

        var cultura = new CultureInfo("es-ES");
        for (var m = 1; m <= 12; m++)
        {
            var nombre = cultura.DateTimeFormat.GetMonthName(m);
            if (Normalizar(nombre) == Normalizar(texto)) return m;
        }
        return null;
    }

    private static string? LocalizarConsultor(IXLWorksheet hoja, int filaEncabezado)
    {
        for (var r = 1; r < filaEncabezado; r++)
        {
            foreach (var celda in hoja.Row(r).CellsUsed())
            {
                if (Normalizar(celda.GetString()) is not ("consultor" or "empleado")) continue;

                for (var c = celda.Address.ColumnNumber + 1; c <= celda.Address.ColumnNumber + 8; c++)
                {
                    var valor = hoja.Cell(r, c).GetString().Trim();
                    if (!string.IsNullOrWhiteSpace(valor)) return valor;
                }
            }
        }
        return null;
    }

    // ── Lectura de celdas ────────────────────────────────────────────────────

    private static string LeerTexto(IXLWorksheet hoja, int fila, object columna) =>
        hoja.Cell(fila, (int)columna).GetString();

    private static int? LeerEntero(IXLWorksheet hoja, int fila, object columna)
    {
        var celda = hoja.Cell(fila, (int)columna);
        if (celda.IsEmpty()) return null;
        return int.TryParse(celda.GetString().Trim(), out var valor) ? valor : null;
    }

    /// <summary>
    /// Excel guarda las horas como TimeSpan o como fraccion de dia segun como se escribieran;
    /// tambien admite texto "08:00". Se contemplan los tres casos.
    /// </summary>
    private static TimeOnly? LeerHora(IXLWorksheet hoja, int fila, int columna)
    {
        var celda = hoja.Cell(fila, columna);
        if (celda.IsEmpty()) return null;

        try
        {
            if (celda.DataType == XLDataType.DateTime)
                return TimeOnly.FromDateTime(celda.GetDateTime());

            if (celda.DataType == XLDataType.TimeSpan)
                return TimeOnly.FromTimeSpan(celda.GetTimeSpan());

            if (celda.DataType == XLDataType.Number)
            {
                var fraccion = celda.GetDouble();
                if (fraccion is >= 0 and < 1)
                    return TimeOnly.FromTimeSpan(TimeSpan.FromDays(fraccion));
            }
        }
        catch { /* cae al parseo de texto */ }

        var texto = celda.GetString().Trim();
        if (TimeOnly.TryParse(texto, CultureInfo.InvariantCulture, out var hora)) return hora;
        if (TimeSpan.TryParse(texto, CultureInfo.InvariantCulture, out var ts) && ts < TimeSpan.FromDays(1))
            return TimeOnly.FromTimeSpan(ts);

        return null;
    }

    /// <summary>Minusculas sin tildes, para comparar rotulos sin depender de como se escribieron.</summary>
    private static string Normalizar(string texto)
    {
        var limpio = texto.Trim().ToLowerInvariant()
            .Replace('á', 'a').Replace('é', 'e').Replace('í', 'i')
            .Replace('ó', 'o').Replace('ú', 'u').Replace('ñ', 'n');
        return limpio;
    }
}
