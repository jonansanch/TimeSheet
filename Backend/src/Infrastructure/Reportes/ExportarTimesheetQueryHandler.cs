using System.Data;
using System.Globalization;
using ClosedXML.Excel;
using Dapper;
using KPG.Timesheet.Application.Features.Reportes.Queries.ExportarTimesheet;
using MediatR;

namespace KPG.Timesheet.Infrastructure.Reportes;

public class ExportarTimesheetQueryHandler(IDbConnection db)
    : IRequestHandler<ExportarTimesheetQuery, ExportarTimesheetResult>
{
    private const string SqlNombre = """
        SELECT ISNULL(NombreCompleto, Email) FROM AspNetUsers WHERE Id = @UserId
        """;

    private const string Sql = """
        SELECT r.FechaRegistro,
               r.HoraEntradaAM AS EntradaAM,
               r.HoraSalidaAM  AS SalidaAM,
               r.HoraEntradaPM AS EntradaPM,
               r.HoraSalidaPM  AS SalidaPM,
               r.Cliente,
               r.Proyecto,
               r.Modalidad,
               r.Recurso,
               r.Descripcion
        FROM   RegistrosHoras r
        WHERE  r.UserId = @UserId
          AND  r.FechaRegistro >= @Desde
          AND  r.FechaRegistro < @HastaExclusivo
        ORDER  BY r.FechaRegistro
        """;

    public async Task<ExportarTimesheetResult> Handle(
        ExportarTimesheetQuery request,
        CancellationToken cancellationToken)
    {
        var desde = new DateOnly(request.Anio, request.Mes, 1);
        var hastaExclusivo = desde.AddMonths(1);

        var nombre = await db.ExecuteScalarAsync<string>(SqlNombre, new { request.UserId }) ?? request.UserId;
        var rows   = (await db.QueryAsync<RawRow>(Sql, new { request.UserId, Desde = desde, HastaExclusivo = hastaExclusivo })).ToList();

        var bytes    = GenerarExcel(nombre, request.Mes, request.Anio, rows);
        var cultura  = new CultureInfo("es-ES");
        var mesNombre = new DateTime(request.Anio, request.Mes, 1).ToString("MMMM", cultura);
        mesNombre = char.ToUpper(mesNombre[0]) + mesNombre[1..];

        var nombreArchivo = nombre.Replace(" ", "-").ToLowerInvariant();
        var fileName = $"timesheet-{nombreArchivo}-{mesNombre.ToLower()}-{request.Anio}.xlsx";

        return new ExportarTimesheetResult(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    private static byte[] GenerarExcel(string nombre, int mes, int anio, List<RawRow> rows)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Timesheet");

        // Anchos de columna
        ws.Column(1).Width  = 8;   // Fecha
        ws.Column(2).Width  = 12;  // Entrada AM
        ws.Column(3).Width  = 12;  // Salida AM
        ws.Column(4).Width  = 12;  // Entrada PM
        ws.Column(5).Width  = 12;  // Salida PM
        ws.Column(6).Width  = 10;  // Total hh:mm
        ws.Column(7).Width  = 10;  // Total decimal
        ws.Column(8).Width  = 22;  // Cliente
        ws.Column(9).Width  = 34;  // Proyecto
        ws.Column(10).Width = 16;  // Modalidad
        ws.Column(11).Width = 22;  // Recurso
        ws.Column(12).Width = 55;  // Descripción

        var azulOscuro = XLColor.FromHtml("#1F3864");
        var cultura    = new CultureInfo("es-ES");
        int r = 1;

        // ── Fila 1: Título ─────────────────────────────────────────────
        var title = ws.Range(r, 1, r, 12).Merge();
        title.Value = "Timesheet KPG";
        title.Style.Font.Bold          = true;
        title.Style.Font.FontSize      = 16;
        title.Style.Font.FontColor     = azulOscuro;
        title.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        title.Style.Alignment.Vertical   = XLAlignmentVerticalValues.Center;
        ws.Row(r).Height = 32;
        r++;
        r++; // fila vacía

        // ── Fila 3: Consultor / Mes / Año ──────────────────────────────
        ws.Cell(r, 1).Value = "Consultor";
        ws.Cell(r, 1).Style.Font.Bold = true;
        ws.Range(r, 2, r, 5).Merge().Value = nombre;

        ws.Cell(r, 6).Value = "Mes";
        ws.Cell(r, 6).Style.Font.Bold = true;

        var mesNombre = new DateTime(anio, mes, 1).ToString("MMMM", cultura);
        mesNombre = char.ToUpper(mesNombre[0]) + mesNombre[1..];
        ws.Cell(r, 7).Value = mesNombre;
        ws.Cell(r, 9).Value = anio;
        r++;
        r++; // fila vacía

        // ── Fila 5: Encabezados ────────────────────────────────────────
        string[] headers = ["Fecha", "Entrada", "Salida", "Entrada", "Salida",
                             "Total Diario", "Total Diario",
                             "Cliente", "Proyecto", "Modalidad", "Recurso",
                             "Descripción Tarea Diaria"];

        for (int col = 1; col <= 12; col++)
        {
            var cell = ws.Cell(r, col);
            cell.Value = headers[col - 1];
            cell.Style.Font.Bold              = true;
            cell.Style.Fill.BackgroundColor   = azulOscuro;
            cell.Style.Font.FontColor         = XLColor.White;
            cell.Style.Alignment.Horizontal   = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical     = XLAlignmentVerticalValues.Center;
            cell.Style.Border.OutsideBorder   = XLBorderStyleValues.Thin;
            cell.Style.Border.OutsideBorderColor = XLColor.FromHtml("#4472C4");
        }

        // Sub-encabezados AM / PM bajo Entrada y Salida
        ws.Row(r).Height = 22;
        r++;

        // Fila de sub-encabezados AM/PM
        ws.Cell(r, 1).Value = "";
        void SubHeader(int col, string txt)
        {
            var c = ws.Cell(r, col);
            c.Value = txt;
            c.Style.Font.Bold            = true;
            c.Style.Font.FontSize        = 8;
            c.Style.Fill.BackgroundColor = XLColor.FromHtml("#4472C4");
            c.Style.Font.FontColor       = XLColor.White;
            c.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            c.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            c.Style.Border.OutsideBorderColor = XLColor.FromHtml("#4472C4");
        }
        SubHeader(2, "AM"); SubHeader(3, "AM");
        SubHeader(4, "PM"); SubHeader(5, "PM");
        ws.Row(r).Height = 14;
        r++;

        // ── Filas de datos ────────────────────────────────────────────
        decimal totalDecimal = 0;
        bool alternate = false;

        foreach (var row in rows)
        {
            alternate = !alternate;
            var rowBg = alternate ? XLColor.White : XLColor.FromHtml("#EEF3FA");

            // Calcular total minutos del día
            int totalMin = 0;
            if (row.EntradaAM.HasValue && row.SalidaAM.HasValue)
                totalMin += (int)(row.SalidaAM.Value - row.EntradaAM.Value).TotalMinutes;
            if (row.EntradaPM.HasValue && row.SalidaPM.HasValue)
                totalMin += (int)(row.SalidaPM.Value - row.EntradaPM.Value).TotalMinutes;

            var dec = Math.Round(totalMin / 60.0m, 2);
            totalDecimal += dec;

            // Contenido de celdas
            ws.Cell(r, 1).Value = row.FechaRegistro.Day;

            if (row.EntradaAM.HasValue) ws.Cell(r, 2).Value = FormatTime(row.EntradaAM.Value);
            if (row.SalidaAM.HasValue)  ws.Cell(r, 3).Value = FormatTime(row.SalidaAM.Value);
            if (row.EntradaPM.HasValue) ws.Cell(r, 4).Value = FormatTime(row.EntradaPM.Value);
            if (row.SalidaPM.HasValue)  ws.Cell(r, 5).Value = FormatTime(row.SalidaPM.Value);

            ws.Cell(r, 6).Value = $"{totalMin / 60}:{totalMin % 60:00}";
            ws.Cell(r, 7).Value = dec;
            ws.Cell(r, 7).Style.NumberFormat.Format = "0.00";

            ws.Cell(r, 8).Value  = row.Cliente;
            ws.Cell(r, 9).Value  = row.Proyecto;
            ws.Cell(r, 10).Value = row.Modalidad;
            ws.Cell(r, 11).Value = row.Recurso;
            ws.Cell(r, 12).Value = row.Descripcion ?? string.Empty;
            ws.Cell(r, 12).Style.Alignment.WrapText = true;

            // Estilo de la fila
            var dataRange = ws.Range(r, 1, r, 12);
            dataRange.Style.Fill.BackgroundColor   = rowBg;
            dataRange.Style.Border.InsideBorder    = XLBorderStyleValues.Hair;
            dataRange.Style.Border.InsideBorderColor  = XLColor.Gray;
            dataRange.Style.Border.OutsideBorder   = XLBorderStyleValues.Thin;
            dataRange.Style.Border.OutsideBorderColor = XLColor.Gray;

            // Alineación centrada para columnas numéricas/hora
            for (int col = 1; col <= 7; col++)
                ws.Cell(r, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Row(r).Height = 18;
            r++;
        }

        // ── Fila de totales ───────────────────────────────────────────
        r++;
        var labelCell = ws.Range(r, 6, r, 7).Merge();
        labelCell.Value = "Total de horas trabajadas:";
        labelCell.Style.Font.Bold            = true;
        labelCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        labelCell.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;

        var valCell = ws.Cell(r, 8);
        valCell.Value = totalDecimal;
        valCell.Style.Font.Bold            = true;
        valCell.Style.NumberFormat.Format  = "0.00";
        valCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        valCell.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;

        // Congelar la fila de encabezados (fila 5 = r=5 antes de los datos)
        ws.SheetView.Freeze(6, 0);

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    private static string FormatTime(TimeSpan t)
    {
        var h = t.Hours % 12;
        if (h == 0) h = 12;
        var ampm = t.Hours < 12 ? "AM" : "PM";
        return $"{h}:{t.Minutes:00} {ampm}";
    }

    private sealed record RawRow(
        DateTime  FechaRegistro,
        TimeSpan? EntradaAM,
        TimeSpan? SalidaAM,
        TimeSpan? EntradaPM,
        TimeSpan? SalidaPM,
        string    Cliente,
        string    Proyecto,
        string    Modalidad,
        string    Recurso,
        string?   Descripcion);
}
