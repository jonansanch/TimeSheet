using System.Data;
using Dapper;
using KPG.Timesheet.Application.Features.Reportes.Queries.ExportarReporteHoras;
using MediatR;
using MiniExcelLibs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace KPG.Timesheet.Infrastructure.Reportes;

public class ExportarReporteHorasQueryHandler(IDbConnection db)
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
               r.Cliente,
               r.Proyecto,
               r.Modalidad,
               r.Lugar,
               r.Descripcion
        FROM   RegistrosHoras r
        JOIN   AspNetUsers u ON r.UserId = u.Id
        WHERE  r.FechaRegistro BETWEEN @Desde AND @Hasta
          AND  (@UserId  IS NULL OR r.UserId  = @UserId)
          AND  (@ClientePattern IS NULL OR r.Cliente LIKE @ClientePattern ESCAPE '\')
          AND  (@ProyectoPattern IS NULL OR r.Proyecto LIKE @ProyectoPattern ESCAPE '\')
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
            ProyectoPattern = BuildPrefixLikePattern(request.Proyecto)
        })).ToList();

        return request.Formato == ExportFormato.Excel
            ? GenerarExcel(rows, request)
            : GenerarPdf(rows, request);
    }

    private static ExportarReporteHorasResult GenerarExcel(List<ExportRow> rows, ExportarReporteHorasQuery req)
    {
        using var ms = new MemoryStream();
        ms.SaveAs(rows);
        var fileName = $"reporte-horas-{req.Desde:yyyyMMdd}-{req.Hasta:yyyyMMdd}.xlsx";
        return new ExportarReporteHorasResult(
            ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
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

    private static ExportarReporteHorasResult GenerarPdf(List<ExportRow> rows, ExportarReporteHorasQuery req)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1.5f, QuestPDF.Infrastructure.Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(col =>
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
