using System.Data;
using ClosedXML.Excel;
using Dapper;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Reportes.Queries.ExportarReporteHoras;
using MediatR;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ParametrosSistemaKeys = KPG.Timesheet.Domain.Constants.ParametrosSistema;

namespace KPG.Timesheet.Infrastructure.Reportes;

public class ExportarReporteHorasQueryHandler(IDbConnection db, IParametrosSistemaService parametros)
    : IRequestHandler<ExportarReporteHorasQuery, ExportarReporteHorasResult>
{
    private const string Sql = """
        SELECT ISNULL(u.NombreCompleto, u.Email)                          AS Empleado,
               u.Email,
               CONVERT(varchar(10), r.FechaRegistro, 103)                 AS Fecha,
               ISNULL(CONVERT(varchar(5), r.HoraEntrada1, 108), '')       AS Entrada1,
               ISNULL(CONVERT(varchar(5), r.HoraSalida1,  108), '')       AS Salida1,
               ISNULL(CONVERT(varchar(5), r.HoraEntrada2, 108), '')       AS Entrada2,
               ISNULL(CONVERT(varchar(5), r.HoraSalida2,  108), '')       AS Salida2,
               ISNULL(CONVERT(varchar(5), r.HoraEntrada3, 108), '')       AS Entrada3,
               ISNULL(CONVERT(varchar(5), r.HoraSalida3,  108), '')       AS Salida3,
               ROUND((
                   ISNULL(DATEDIFF(MINUTE, r.HoraEntrada1, r.HoraSalida1), 0) +
                   ISNULL(DATEDIFF(MINUTE, r.HoraEntrada2, r.HoraSalida2), 0) +
                   ISNULL(DATEDIFF(MINUTE, r.HoraEntrada3, r.HoraSalida3), 0)
               ) / 60.0, 2) AS Horas,
               r.ClienteNombre  AS Cliente,
               r.ProyectoNombre AS Proyecto,
               r.Modalidad,
               r.Lugar,
               r.Descripcion
        FROM   RegistrosHoras r
        JOIN   AspNetUsers u ON r.UserId = u.Id
        WHERE  r.FechaRegistro BETWEEN @Desde AND @Hasta
          AND  (@UserId  IS NULL OR r.UserId  = @UserId)
          AND  (@ClientePattern IS NULL OR r.ClienteNombre LIKE @ClientePattern ESCAPE '\')
          AND  (@ProyectoPattern IS NULL OR r.ProyectoNombre LIKE @ProyectoPattern ESCAPE '\')
          AND  (@RecursoPattern IS NULL OR r.Recurso LIKE @RecursoPattern ESCAPE '\')
        ORDER  BY r.FechaRegistro DESC, Empleado
        OFFSET 0 ROWS FETCH NEXT 1000 ROWS ONLY
        """;

    public async Task<ExportarReporteHorasResult> Handle(
        ExportarReporteHorasQuery request,
        CancellationToken cancellationToken)
    {
        var rows = (await db.QueryAsync<ExportRow>(Sql, new
        {
            Desde    = request.Desde,
            Hasta    = request.Hasta,
            UserId   = string.IsNullOrWhiteSpace(request.UserId)   ? null : request.UserId,
            ClientePattern  = BuildPrefixLikePattern(request.Cliente),
            ProyectoPattern = BuildPrefixLikePattern(request.Proyecto),
            RecursoPattern  = BuildPrefixLikePattern(request.Recurso)
        })).ToList();

        var logo = await parametros.GetTextoAsync(ParametrosSistemaKeys.LogoReportes, string.Empty, cancellationToken);

        return request.Formato == ExportFormato.Excel
            ? GenerarExcel(rows, request, logo)
            : GenerarPdf(rows, request, logo);
    }

    private static readonly string[] Encabezados =
    [
        "Empleado", "Email", "Fecha", "Entrada 1", "Salida 1", "Entrada 2", "Salida 2",
        "Entrada 3", "Salida 3", "Horas", "Cliente", "Proyecto", "Modalidad", "Lugar", "Descripción"
    ];

    private static ExportarReporteHorasResult GenerarExcel(
        List<ExportRow> rows, ExportarReporteHorasQuery req, string? logoDataUri)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Reporte de Horas");
        ws.ShowGridLines = false;

        AgregarLogoSiHay(ws, logoDataUri);

        var titulo = ws.Range(1, 1, 1, Encabezados.Length).Merge();
        titulo.Value = "Reporte de Horas KPG Timesheet";
        titulo.Style.Font.Bold = true;
        titulo.Style.Font.FontSize = 16;
        ws.Row(1).Height = 24;

        ws.Cell(2, 1).Value = $"Período: {req.Desde:dd/MM/yyyy} - {req.Hasta:dd/MM/yyyy}";
        ws.Cell(2, 1).Style.Font.FontColor = XLColor.Gray;

        const int filaEncabezado = 4;
        for (var i = 0; i < Encabezados.Length; i++)
        {
            var celda = ws.Cell(filaEncabezado, i + 1);
            celda.Value = Encabezados[i];
            celda.Style.Font.Bold = true;
            celda.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        var fila = filaEncabezado + 1;
        foreach (var r in rows)
        {
            ws.Cell(fila, 1).Value  = r.Empleado;
            ws.Cell(fila, 2).Value  = r.Email;
            ws.Cell(fila, 3).Value  = r.Fecha;
            ws.Cell(fila, 4).Value  = r.Entrada1;
            ws.Cell(fila, 5).Value  = r.Salida1;
            ws.Cell(fila, 6).Value  = r.Entrada2;
            ws.Cell(fila, 7).Value  = r.Salida2;
            ws.Cell(fila, 8).Value  = r.Entrada3;
            ws.Cell(fila, 9).Value  = r.Salida3;
            ws.Cell(fila, 10).Value = r.Horas;
            ws.Cell(fila, 11).Value = r.Cliente;
            ws.Cell(fila, 12).Value = r.Proyecto;
            ws.Cell(fila, 13).Value = r.Modalidad;
            ws.Cell(fila, 14).Value = r.Lugar;
            ws.Cell(fila, 15).Value = r.Descripcion;

            ws.Range(fila, 1, fila, Encabezados.Length).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            fila++;
        }

        ws.Columns().AdjustToContents();
        ws.SheetView.Freeze(filaEncabezado, 0);

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        var fileName = $"reporte-horas-{req.Desde:yyyyMMdd}-{req.Hasta:yyyyMMdd}.xlsx";
        return new ExportarReporteHorasResult(
            ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    private static void AgregarLogoSiHay(IXLWorksheet ws, string? logoDataUri)
    {
        var bytes  = LogoDataUri.DecodeBytes(logoDataUri);
        var format = LogoDataUri.DecodeFormat(logoDataUri);
        if (bytes is null || format is null) return;

        using var stream = new MemoryStream(bytes);
        ws.AddPicture(stream, format.Value)
            .MoveTo(ws.Cell(1, Encabezados.Length))
            .WithSize(110, 36);
    }

    private static string? BuildPrefixLikePattern(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return EscapeLikePattern(value.Trim()) + "%";
    }

    private static string EscapeLikePattern(string value)
        => value
            .Replace(@"\", @"\\")
            .Replace("%", @"\%")
            .Replace("_", @"\_")
            .Replace("[", @"\[");

    private static ExportarReporteHorasResult GenerarPdf(
        List<ExportRow> rows, ExportarReporteHorasQuery req, string? logoDataUri)
    {
        var logo = LogoDataUri.DecodeBytes(logoDataUri);

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1.5f, QuestPDF.Infrastructure.Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Row(header =>
                {
                    header.RelativeItem().Column(col =>
                    {
                        col.Item().Text($"Reporte de Horas KPG Timesheet")
                            .FontSize(14).Bold();
                        col.Item().Text($"Período: {req.Desde:dd/MM/yyyy} – {req.Hasta:dd/MM/yyyy}")
                            .FontSize(10).FontColor(Colors.Grey.Medium);
                        if (!string.IsNullOrWhiteSpace(req.Cliente))
                            col.Item().Text($"Cliente: {req.Cliente}").FontSize(9).FontColor(Colors.Grey.Medium);
                        if (!string.IsNullOrWhiteSpace(req.Proyecto))
                            col.Item().Text($"Proyecto: {req.Proyecto}").FontSize(9).FontColor(Colors.Grey.Medium);
                        col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });

                    if (logo is not null)
                        header.ConstantItem(70).AlignRight().Image(logo).FitArea();
                });

                page.Content().PaddingTop(8).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(2.5f); // Empleado
                        cols.RelativeColumn(1.2f); // Fecha
                        cols.ConstantColumn(34);   // Entrada horario 1
                        cols.ConstantColumn(34);   // Salida  horario 1
                        cols.ConstantColumn(34);   // Entrada horario 2
                        cols.ConstantColumn(34);   // Salida  horario 2
                        cols.ConstantColumn(34);   // Entrada horario 3
                        cols.ConstantColumn(34);   // Salida  horario 3
                        cols.ConstantColumn(32);   // Horas
                        cols.RelativeColumn(2f);   // Cliente
                        cols.RelativeColumn(2f);   // Proyecto
                        cols.RelativeColumn(4f);   // Descripción
                    });

                    static IContainer HeaderCell(IContainer c) =>
                        c.Background(Colors.Blue.Darken3)
                         .Padding(4)
                         .DefaultTextStyle(x => x.FontColor(Colors.White).Bold().FontSize(8));

                    table.Header(header =>
                    {
                        foreach (var h in new[] { "Empleado", "Fecha", "Entrada 1", "Salida 1", "Entrada 2", "Salida 2", "Entrada 3", "Salida 3", "Horas", "Cliente", "Proyecto", "Descripción" })
                            header.Cell().Element(HeaderCell).Text(h);
                    });

                    var alternateRow = false;
                    foreach (var r in rows)
                    {
                        alternateRow = !alternateRow;
                        var bg = alternateRow ? Colors.White : Colors.Grey.Lighten5;

                        static IContainer DataCell(IContainer c, string bg) =>
                            c.Background(bg).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(3);

                        foreach (var val in new[] { r.Empleado, r.Fecha, r.Entrada1, r.Salida1, r.Entrada2, r.Salida2, r.Entrada3, r.Salida3, r.Horas.ToString("F2"), r.Cliente, r.Proyecto, r.Descripcion })
                            table.Cell().Element(c => DataCell(c, bg)).Text(val ?? string.Empty);
                    }
                });

                page.Footer().AlignRight().Text(text =>
                {
                    text.Span($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}  |  Página ").FontSize(8).FontColor(Colors.Grey.Medium);
                    text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                    text.Span(" de ").FontSize(8).FontColor(Colors.Grey.Medium);
                    text.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        });

        var fileName = $"reporte-horas-{req.Desde:yyyyMMdd}-{req.Hasta:yyyyMMdd}.pdf";
        return new ExportarReporteHorasResult(doc.GeneratePdf(), "application/pdf", fileName);
    }

    private sealed record ExportRow(
        string  Empleado,
        string  Email,
        string  Fecha,
        string  Entrada1,
        string  Salida1,
        string  Entrada2,
        string  Salida2,
        string  Entrada3,
        string  Salida3,
        decimal Horas,
        string  Cliente,
        string  Proyecto,
        string  Modalidad,
        string  Lugar,
        string  Descripcion);
}
