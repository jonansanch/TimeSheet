using System.Globalization;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace KPG.Timesheet.Infrastructure.Reportes;

/// <summary>Una fila del timesheet, ya resuelta desde la base.</summary>
public record TimesheetFila(
    DateOnly  Fecha,
    TimeOnly? Entrada1,
    TimeOnly? Salida1,
    TimeOnly? Entrada2,
    TimeOnly? Salida2,
    TimeOnly? Entrada3,
    TimeOnly? Salida3,
    string    Cliente,
    string    Proyecto,
    string    Modalidad,
    string    Recurso,
    string    Lugar,
    string    Descripcion)
{
    public IEnumerable<(TimeOnly? Entrada, TimeOnly? Salida)> Horarios()
    {
        yield return (Entrada1, Salida1);
        yield return (Entrada2, Salida2);
        yield return (Entrada3, Salida3);
    }

    public int TotalMinutos => Horarios().Sum(h =>
        h.Entrada.HasValue && h.Salida.HasValue
            ? (int)(h.Salida.Value - h.Entrada.Value).TotalMinutes
            : 0);
}

/// <summary>
/// Arma el timesheet mensual con el formato de la plantilla que usa el cliente
/// (hoja "ConsultorKPG"): columna A de margen, titulo en la fila 5, consultor y mes en la 6,
/// encabezados en la 7 y datos desde la 8. La fecha es solo el numero de dia.
///
/// <para>
/// La plantilla trae <b>dos</b> bloques horarios y el sistema admite tres. La hoja se arma
/// con dos y solo agrega el tercer par si algun dia del periodo lo usa: asi el archivo
/// habitual sale igual al que el cliente conoce y ningun dato se pierde en silencio.
/// </para>
/// <para>
/// Va aparte del handler para poder verificar el formato sin base de datos: el test lo
/// genera y se lo da a leer a <see cref="TimesheetImportParser"/>, que ubica todo por
/// rotulo. Si el parser del formato del cliente entiende lo que exportamos, el formato
/// coincide de verdad.
/// </para>
/// </summary>
public static class TimesheetDocumentBuilder
{
    public static byte[] Excel(string consultor, int mes, int anio, IReadOnlyList<TimesheetFila> filas)
    {
        var cols = Columnas.Para(filas);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("ConsultorKPG");

        // La plantilla usa Century Gothic 11 en toda la hoja y oculta la cuadricula.
        ws.Style.Font.FontName = "Century Gothic";
        ws.Style.Font.FontSize = 11;
        ws.ShowGridLines = false;

        AplicarAnchos(ws, cols);

        // ── Fila 5: titulo ──────────────────────────────────────────────────
        var titulo = ws.Range(5, cols.Fecha, 5, cols.Ultima).Merge();
        titulo.Value = "Timesheet KPG";
        titulo.Style.Font.Bold            = true;
        titulo.Style.Font.FontSize        = 20;
        titulo.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        titulo.Style.Alignment.Vertical   = XLAlignmentVerticalValues.Center;
        ws.Row(5).Height = 45;

        // ── Fila 6: consultor / mes / anio ──────────────────────────────────
        ws.Cell(6, cols.Fecha).Value = "Consultor";
        ws.Cell(6, cols.Fecha).Style.Font.Bold = true;
        ws.Range(6, cols.Fecha + 1, 6, cols.TotalHm - 1).Merge().Value = consultor;

        ws.Cell(6, cols.TotalHm).Value = "Mes";
        ws.Cell(6, cols.TotalHm).Style.Font.Bold = true;
        ws.Cell(6, cols.Cliente).Value  = NombreDelMes(mes, anio);
        ws.Cell(6, cols.Proyecto).Value = anio;
        ws.Row(6).Height = 29;

        // ── Fila 7: encabezados ─────────────────────────────────────────────
        const int filaEncabezado = 7;
        foreach (var (columna, texto) in cols.Encabezados())
            Encabezado(ws.Cell(filaEncabezado, columna), texto);

        // "Total Diario" cubre las dos columnas de total (hh:mm y decimal), como la plantilla.
        var totalDiario = ws.Range(filaEncabezado, cols.TotalHm, filaEncabezado, cols.TotalDecimal).Merge();
        Encabezado(totalDiario.FirstCell(), "Total Diario");
        totalDiario.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        ws.Row(filaEncabezado).Height = 17;

        // ── Datos ───────────────────────────────────────────────────────────
        var fila = filaEncabezado + 1;
        decimal totalDecimal = 0;

        foreach (var f in filas)
        {
            var minutos = f.TotalMinutos;
            var dec     = Math.Round(minutos / 60.0m, 2);
            totalDecimal += dec;

            // Solo el numero de dia: el mes y el anio ya estan en la cabecera.
            ws.Cell(fila, cols.Fecha).Value = f.Fecha.Day;

            var horarios = f.Horarios().Take(cols.Pares).ToList();
            for (var i = 0; i < horarios.Count; i++)
            {
                var (entrada, salida) = horarios[i];
                if (entrada.HasValue) Hora(ws.Cell(fila, cols.Entrada(i + 1)), entrada.Value);
                if (salida.HasValue)  Hora(ws.Cell(fila, cols.Salida(i + 1)),  salida.Value);
            }

            ws.Cell(fila, cols.TotalHm).Value = $"{minutos / 60}:{minutos % 60:00}";
            ws.Cell(fila, cols.TotalDecimal).Value = dec;
            ws.Cell(fila, cols.TotalDecimal).Style.NumberFormat.Format = "0.00";

            ws.Cell(fila, cols.Cliente).Value     = f.Cliente;
            ws.Cell(fila, cols.Proyecto).Value    = f.Proyecto;
            ws.Cell(fila, cols.Modalidad).Value   = f.Modalidad;
            ws.Cell(fila, cols.Recurso).Value     = f.Recurso;
            ws.Cell(fila, cols.Lugar).Value       = f.Lugar;
            ws.Cell(fila, cols.Descripcion).Value = f.Descripcion;

            var rango = ws.Range(fila, cols.Fecha, fila, cols.Ultima);
            rango.Style.Border.InsideBorder  = XLBorderStyleValues.Thin;
            rango.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            rango.Style.Alignment.Vertical   = XLAlignmentVerticalValues.Center;
            rango.Style.Alignment.WrapText   = true;

            for (var col = cols.Fecha; col <= cols.TotalDecimal; col++)
                ws.Cell(fila, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            fila++;
        }

        // ── Total, dos filas mas abajo, como la plantilla ───────────────────
        fila += 2;
        var etiqueta = ws.Range(fila, cols.TotalDecimal - 2, fila, cols.TotalDecimal - 1).Merge();
        etiqueta.Value = "Total de horas trabajadas:";
        etiqueta.Style.Font.Bold            = true;
        etiqueta.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        etiqueta.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

        var valor = ws.Cell(fila, cols.TotalDecimal);
        valor.Value = totalDecimal;
        valor.Style.Font.Bold            = true;
        valor.Style.NumberFormat.Format  = "0.00";
        valor.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        valor.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

        ws.PageSetup.PageOrientation = XLPageOrientation.Landscape;
        ws.PageSetup.FitToPages(1, 0);
        ws.SheetView.Freeze(filaEncabezado, 0);

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public static byte[] Pdf(string consultor, int mes, int anio, IReadOnlyList<TimesheetFila> filas)
    {
        var pares = Columnas.ParesNecesarios(filas);
        var total = Math.Round(filas.Sum(f => f.TotalMinutos) / 60.0m, 2);
        var periodo = $"{NombreDelMes(mes, anio)} {anio}";

        return Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1.2f, QuestPDF.Infrastructure.Unit.Centimetre);
                page.DefaultTextStyle(t => t.FontSize(8));

                page.Header().Column(col =>
                {
                    col.Item().AlignCenter().Text("Timesheet KPG").FontSize(16).Bold();
                    col.Item().PaddingTop(6).Row(r =>
                    {
                        r.RelativeItem().Text(t => { t.Span("Consultor: ").Bold(); t.Span(consultor); });
                        r.ConstantItem(160).AlignRight()
                            .Text(t => { t.Span("Mes: ").Bold(); t.Span(periodo); });
                    });
                    col.Item().PaddingBottom(6);
                });

                page.Content().Table(tabla =>
                {
                    tabla.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(28);                      // Fecha
                        for (var i = 0; i < pares * 2; i++)
                            c.ConstantColumn(42);                  // Entrada / Salida
                        c.ConstantColumn(38);                      // Total hh:mm
                        c.ConstantColumn(34);                      // Total decimal
                        c.RelativeColumn(2.0f);                    // Cliente
                        c.RelativeColumn(2.6f);                    // Proyecto
                        c.RelativeColumn(1.3f);                    // Modalidad
                        c.RelativeColumn(1.8f);                    // Recurso
                        c.RelativeColumn(1.2f);                    // Lugar
                        c.RelativeColumn(4.0f);                    // Descripcion
                    });

                    tabla.Header(h =>
                    {
                        foreach (var texto in Columnas.TextosEncabezado(pares))
                            h.Cell().Element(CeldaEncabezado).Text(texto);
                    });

                    foreach (var f in filas)
                    {
                        var minutos = f.TotalMinutos;

                        tabla.Cell().Element(Celda).AlignCenter().Text(f.Fecha.Day.ToString());

                        foreach (var (entrada, salida) in f.Horarios().Take(pares))
                        {
                            tabla.Cell().Element(Celda).AlignCenter().Text(FormatoHora(entrada));
                            tabla.Cell().Element(Celda).AlignCenter().Text(FormatoHora(salida));
                        }

                        tabla.Cell().Element(Celda).AlignCenter().Text($"{minutos / 60}:{minutos % 60:00}");
                        tabla.Cell().Element(Celda).AlignCenter()
                            .Text(Math.Round(minutos / 60.0m, 2).ToString("0.00", CultureInfo.InvariantCulture));
                        tabla.Cell().Element(Celda).Text(f.Cliente);
                        tabla.Cell().Element(Celda).Text(f.Proyecto);
                        tabla.Cell().Element(Celda).Text(f.Modalidad);
                        tabla.Cell().Element(Celda).Text(f.Recurso);
                        tabla.Cell().Element(Celda).Text(f.Lugar);
                        tabla.Cell().Element(Celda).Text(f.Descripcion);
                    }
                });

                page.Footer().PaddingTop(8).Row(r =>
                {
                    r.RelativeItem().Text(t =>
                    {
                        t.Span("Total de horas trabajadas: ").Bold();
                        t.Span(total.ToString("0.00", CultureInfo.InvariantCulture)).Bold();
                    });
                    r.ConstantItem(120).AlignRight().Text(t =>
                    {
                        t.CurrentPageNumber();
                        t.Span(" / ");
                        t.TotalPages();
                    });
                });
            });
        }).GeneratePdf();

        static IContainer CeldaEncabezado(IContainer c) =>
            c.Border(0.5f).BorderColor(Colors.Grey.Darken1)
             .Background(Colors.Grey.Lighten3).Padding(3).DefaultTextStyle(t => t.Bold());

        static IContainer Celda(IContainer c) =>
            c.Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(3);
    }

    public static string NombreDelMes(int mes, int anio)
    {
        var texto = new DateTime(anio, mes, 1).ToString("MMMM", new CultureInfo("es-ES"));
        return char.ToUpper(texto[0]) + texto[1..];
    }

    private static void Encabezado(IXLCell celda, string texto)
    {
        celda.Value = texto;
        celda.Style.Font.Bold            = true;
        celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        celda.Style.Alignment.Vertical   = XLAlignmentVerticalValues.Center;
        celda.Style.Alignment.WrapText   = true;
        celda.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    }

    /// <summary>
    /// Guarda solo la hora del dia, como la plantilla. Con un DateTime completo y un formato
    /// de hora, Excel conserva la parte de fecha y al releer el archivo la celda vuelve como
    /// un TimeSpan de miles de horas, ilegible para el parser.
    /// </summary>
    private static void Hora(IXLCell celda, TimeOnly valor)
    {
        celda.Value = valor.ToTimeSpan();
        celda.Style.NumberFormat.Format  = "[$-409]h:mm AM/PM;@";
        celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    }

    private static string FormatoHora(TimeOnly? valor) =>
        valor.HasValue
            ? DateTime.Today.Add(valor.Value.ToTimeSpan()).ToString("h:mm tt", CultureInfo.InvariantCulture)
            : string.Empty;

    private static void AplicarAnchos(IXLWorksheet ws, Columnas cols)
    {
        ws.Column(Columnas.Margen).Width = 5;
        ws.Column(cols.Fecha).Width      = 14;

        for (var par = 1; par <= cols.Pares; par++)
        {
            ws.Column(cols.Entrada(par)).Width = 11;
            ws.Column(cols.Salida(par)).Width  = 11;
        }

        ws.Column(cols.TotalHm).Width      = 11;
        ws.Column(cols.TotalDecimal).Width = 9;
        ws.Column(cols.Cliente).Width      = 27;
        ws.Column(cols.Proyecto).Width     = 37;
        ws.Column(cols.Modalidad).Width    = 16;
        ws.Column(cols.Recurso).Width      = 31;
        ws.Column(cols.Lugar).Width        = 14;
        ws.Column(cols.Descripcion).Width  = 53;
    }

    /// <summary>
    /// Posiciones de cada columna. Se calculan en vez de ser constantes porque el tercer par
    /// de horarios solo aparece si algun dia lo usa, y todo lo que va despues se corre.
    /// </summary>
    private sealed record Columnas(int Pares)
    {
        /// <summary>La plantilla deja la columna A como margen y empieza el contenido en B.</summary>
        public const int Margen = 1;

        public static int ParesNecesarios(IReadOnlyList<TimesheetFila> filas) =>
            filas.Any(f => f.Entrada3.HasValue || f.Salida3.HasValue) ? 3 : 2;

        public static Columnas Para(IReadOnlyList<TimesheetFila> filas) => new(ParesNecesarios(filas));

        public int Fecha            => Margen + 1;
        public int Entrada(int par) => Fecha + (par - 1) * 2 + 1;
        public int Salida(int par)  => Entrada(par) + 1;
        public int TotalHm          => Fecha + Pares * 2 + 1;
        public int TotalDecimal     => TotalHm + 1;
        public int Cliente          => TotalDecimal + 1;
        public int Proyecto         => Cliente + 1;
        public int Modalidad        => Proyecto + 1;
        public int Recurso          => Modalidad + 1;
        public int Lugar            => Recurso + 1;
        public int Descripcion      => Lugar + 1;
        public int Ultima           => Descripcion;

        /// <summary>Encabezados salvo "Total Diario", que se combina aparte sobre dos columnas.</summary>
        public IEnumerable<(int Columna, string Texto)> Encabezados()
        {
            yield return (Fecha, "Fecha");
            for (var par = 1; par <= Pares; par++)
            {
                yield return (Entrada(par), "Entrada");
                yield return (Salida(par),  "Salida");
            }
            yield return (Cliente,     "Cliente");
            yield return (Proyecto,    "Proyecto");
            yield return (Modalidad,   "Modalidad");
            yield return (Recurso,     "Recurso");
            yield return (Lugar,       "Lugar");
            yield return (Descripcion, "Descripción Tarea Diaria");
        }

        public static IEnumerable<string> TextosEncabezado(int pares)
        {
            yield return "Fecha";
            for (var par = 1; par <= pares; par++)
            {
                yield return "Entrada";
                yield return "Salida";
            }
            yield return "Total";
            yield return "Horas";
            yield return "Cliente";
            yield return "Proyecto";
            yield return "Modalidad";
            yield return "Recurso";
            yield return "Lugar";
            yield return "Descripción Tarea Diaria";
        }
    }
}
