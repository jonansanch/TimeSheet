using ClosedXML.Excel;
using KPG.Timesheet.Infrastructure.Reportes;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.Reportes;

/// <summary>
/// La prueba de que el formato exportado es el del cliente no es comparar celdas a mano:
/// es que <see cref="TimesheetImportParser"/> —que ubica todo por rotulo y esta validado
/// contra el archivo real— pueda leer de vuelta lo que exportamos.
/// </summary>
public class TimesheetDocumentBuilderTests
{
    [Fact]
    public void Excel_ShouldPoderLeerseConElParserDelFormatoDelCliente()
    {
        var bytes = TimesheetDocumentBuilder.Excel("Jonathan Andres Sanchez Silva", 5, 2026, DosDias());

        using var ms = new MemoryStream(bytes);
        var leido = new TimesheetImportParser().Parse(ms);

        leido.Errores.Should().BeEmpty();
        leido.NombreConsultor.Should().Be("Jonathan Andres Sanchez Silva");
        leido.Mes.Should().Be(5);
        leido.Anio.Should().Be(2026);
        leido.Filas.Should().HaveCount(2);

        var primera = leido.Filas[0];
        primera.Fecha.Should().Be(new DateOnly(2026, 5, 4));
        primera.HoraEntrada1.Should().Be(new TimeOnly(8, 0));
        primera.HoraSalida1.Should().Be(new TimeOnly(13, 0));
        primera.HoraEntrada2.Should().Be(new TimeOnly(14, 30));
        primera.HoraSalida2.Should().Be(new TimeOnly(17, 45));
        primera.Cliente.Should().Be("ACUDEN");
        primera.Proyecto.Should().Be("Desarrollo ACUDEN Digital CIMA");
    }

    [Fact]
    public void Excel_ShouldIncluirLugar()
    {
        // La plantilla del cliente trae Lugar y la exportacion anterior no lo sacaba.
        var ws = Abrir(TimesheetDocumentBuilder.Excel("Consultor", 5, 2026, DosDias()));

        var encabezados = Encabezados(ws);
        encabezados.Should().Contain("Lugar");

        var columnaLugar = encabezados.IndexOf("Lugar") + PrimeraColumna;
        ws.Cell(FilaPrimerDato, columnaLugar).GetString().Should().Be("Remoto");
    }

    [Fact]
    public void Excel_SinHorario3_ShouldUsarLasDosParejasDeLaPlantilla()
    {
        var ws = Abrir(TimesheetDocumentBuilder.Excel("Consultor", 5, 2026, DosDias()));

        var encabezados = Encabezados(ws);
        encabezados.Count(h => h == "Entrada").Should().Be(2);
        encabezados.Count(h => h == "Salida").Should().Be(2);

        // Con dos parejas, la descripcion cae en la columna N igual que en la plantilla.
        encabezados.IndexOf("Descripción Tarea Diaria").Should().Be(12);   // B=0 ... N=12
    }

    [Fact]
    public void Excel_ConHorario3_ShouldAgregarLaTerceraPareja()
    {
        // Ningun dato se pierde en silencio: si un dia usa el tercer tramo, la hoja crece.
        var filas = DosDias().Append(DiaCon3Horarios()).ToList();

        var ws = Abrir(TimesheetDocumentBuilder.Excel("Consultor", 5, 2026, filas));

        var encabezados = Encabezados(ws);
        encabezados.Count(h => h == "Entrada").Should().Be(3);
        encabezados.Should().Contain("Lugar");
    }

    [Fact]
    public void Excel_ShouldTotalizarLasHorasDelPeriodo()
    {
        var ws = Abrir(TimesheetDocumentBuilder.Excel("Consultor", 5, 2026, DosDias()));

        var celdaEtiqueta = ws.CellsUsed(c => c.GetString().StartsWith("Total de horas")).Single();
        var total = ws.Cell(celdaEtiqueta.Address.RowNumber, celdaEtiqueta.Address.ColumnNumber + 2);

        // 8:15 + 8:15 = 16.50
        total.GetValue<decimal>().Should().Be(16.50m);
    }

    [Fact]
    public void Excel_SinRegistros_ShouldGenerarLaHojaVacia()
    {
        // Un mes sin registros se entrega igual, con la cabecera y el total en cero.
        var ws = Abrir(TimesheetDocumentBuilder.Excel("Consultor", 5, 2026, []));

        Encabezados(ws).Should().Contain("Fecha");
        ws.CellsUsed(c => c.GetString().StartsWith("Total de horas")).Should().ContainSingle();
    }

    [Fact]
    public void Pdf_ShouldGenerarUnDocumentoValido()
    {
        var bytes = TimesheetDocumentBuilder.Pdf("Consultor", 5, 2026, DosDias());

        bytes.Should().NotBeEmpty();
        System.Text.Encoding.ASCII.GetString(bytes, 0, 5).Should().Be("%PDF-");
    }

    [Fact]
    public void Pdf_SinRegistros_ShouldGenerarseIgual()
    {
        var act = () => TimesheetDocumentBuilder.Pdf("Consultor", 5, 2026, []);

        act.Should().NotThrow();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>La plantilla deja la columna A de margen: el contenido arranca en B.</summary>
    private const int PrimeraColumna = 2;

    /// <summary>Encabezados en la fila 7, datos desde la 8.</summary>
    private const int FilaEncabezado = 7;
    private const int FilaPrimerDato = 8;

    private static IXLWorksheet Abrir(byte[] bytes)
    {
        var ms = new MemoryStream(bytes);
        return new XLWorkbook(ms).Worksheet("ConsultorKPG");
    }

    private static List<string> Encabezados(IXLWorksheet ws) =>
        Enumerable.Range(PrimeraColumna, ws.LastColumnUsed()!.ColumnNumber() - PrimeraColumna + 1)
            .Select(c => ws.Cell(FilaEncabezado, c).GetString())
            .ToList();

    private static List<TimesheetFila> DosDias() =>
    [
        new(new DateOnly(2026, 5, 4),
            new TimeOnly(8, 0), new TimeOnly(13, 0),
            new TimeOnly(14, 30), new TimeOnly(17, 45),
            null, null,
            "ACUDEN", "Desarrollo ACUDEN Digital CIMA", "Remoto", "Desarrolladores", "Remoto",
            "Analisis y revision de casos de uso."),
        new(new DateOnly(2026, 5, 5),
            new TimeOnly(8, 0), new TimeOnly(13, 0),
            new TimeOnly(14, 30), new TimeOnly(17, 45),
            null, null,
            "ACUDEN", "Desarrollo ACUDEN Digital CIMA", "Remoto", "Desarrolladores", "Remoto",
            "Desarrollo de la validacion de identidad.")
    ];

    private static TimesheetFila DiaCon3Horarios() =>
        new(new DateOnly(2026, 5, 6),
            new TimeOnly(8, 0), new TimeOnly(12, 0),
            new TimeOnly(14, 0), new TimeOnly(17, 0),
            new TimeOnly(19, 0), new TimeOnly(21, 0),
            "ACUDEN", "Desarrollo ACUDEN Digital CIMA", "Remoto", "Desarrolladores", "Remoto",
            "Soporte de la salida a produccion.");
}
