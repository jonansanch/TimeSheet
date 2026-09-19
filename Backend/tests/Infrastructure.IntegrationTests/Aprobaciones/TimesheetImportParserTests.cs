using ClosedXML.Excel;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Infrastructure.Reportes;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.Aprobaciones;

/// <summary>
/// El formato real no es una tabla limpia: consultor y mes en la cabecera, la fecha es
/// solo el numero de dia, y la banda de datos trae filas vacias con formulas. Estos tests
/// reconstruyen esa plantilla para fijar el comportamiento.
/// </summary>
public class TimesheetImportParserTests
{
    [Fact]
    public void Parse_ConLaPlantillaReal_ShouldLeerCabeceraYFilas()
    {
        using var archivo = ConstruirPlantilla();

        var resultado = new TimesheetImportParser().Parse(archivo);

        resultado.NombreConsultor.Should().Be("Jonathan Andres Sanchez Silva");
        resultado.Mes.Should().Be(5);
        resultado.Anio.Should().Be(2026);
        resultado.Filas.Should().HaveCount(2);
        resultado.Errores.Should().BeEmpty();
    }

    [Fact]
    public void Parse_ShouldArmarLaFechaConElDiaYLaCabecera()
    {
        // En el archivo la fecha es solo "11": el mes y el ano vienen de la cabecera.
        using var archivo = ConstruirPlantilla();

        var fila = new TimesheetImportParser().Parse(archivo).Filas.First();

        fila.Fecha.Should().Be(new DateOnly(2026, 5, 11));
    }

    [Fact]
    public void Parse_ShouldLeerLosDosHorariosYDejarElTerceroVacio()
    {
        using var archivo = ConstruirPlantilla();

        var fila = new TimesheetImportParser().Parse(archivo).Filas.First();

        fila.HoraEntrada1.Should().Be(new TimeOnly(8, 0));
        fila.HoraSalida1.Should().Be(new TimeOnly(13, 0));
        fila.HoraEntrada2.Should().Be(new TimeOnly(14, 30));
        fila.HoraSalida2.Should().Be(new TimeOnly(18, 0));
        fila.HoraEntrada3.Should().BeNull();
        fila.HoraSalida3.Should().BeNull();
    }

    [Fact]
    public void Parse_ShouldLeerCatalogosYDescripcion()
    {
        using var archivo = ConstruirPlantilla();

        var fila = new TimesheetImportParser().Parse(archivo).Filas.First();

        fila.Cliente.Should().Be("ACUDEN");
        fila.Proyecto.Should().Be("Desarrollo ACUDEN Digital CIMA");
        fila.Modalidad.Should().Be("Remoto");
        fila.Recurso.Should().Be("Desarrolladores");
        fila.Lugar.Should().Be("Remoto");
        fila.Descripcion.Should().StartWith("Creacion job");
    }

    [Fact]
    public void Parse_ShouldIgnorarLasFilasVaciasDeLaPlantilla()
    {
        // La plantilla deja ranuras con formulas pero sin datos: no son errores.
        using var archivo = ConstruirPlantilla();

        var resultado = new TimesheetImportParser().Parse(archivo);

        resultado.Filas.Should().HaveCount(2);
        resultado.Errores.Should().BeEmpty();
    }

    [Fact]
    public void Parse_ShouldDetenerseEnLaFilaDeTotales()
    {
        using var archivo = ConstruirPlantilla();

        var resultado = new TimesheetImportParser().Parse(archivo);

        resultado.Filas.Should().NotContain(f => f.Cliente.Contains("Total"));
    }

    [Fact]
    public void Parse_ConUnDiaQueNoExisteEnElMes_ShouldReportarloComoError()
    {
        using var archivo = ConstruirPlantilla(diaExtra: 31, mes: "Febrero");

        var resultado = new TimesheetImportParser().Parse(archivo);

        resultado.Errores.Should().Contain(e => e.Motivo.Contains("no existe en el mes"));
    }

    [Fact]
    public void Parse_ConUnaFilaSinHorarioCompleto_ShouldReportarlaComoError()
    {
        using var archivo = ConstruirPlantilla(filaSinHoras: true);

        var resultado = new TimesheetImportParser().Parse(archivo);

        resultado.Errores.Should().Contain(e => e.Motivo.Contains("ningun horario completo"));
    }

    [Fact]
    public void Parse_ConMesEnNumero_ShouldEntenderlo()
    {
        using var archivo = ConstruirPlantilla(mes: "5");

        var resultado = new TimesheetImportParser().Parse(archivo);

        resultado.Mes.Should().Be(5);
    }

    [Fact]
    public void Parse_ConTresBloquesHorarios_ShouldLeerLosTres()
    {
        // La plantilla del cliente tiene dos, pero el sistema admite tres.
        using var archivo = ConstruirPlantilla(tresBloques: true);

        var fila = new TimesheetImportParser().Parse(archivo).Filas.First();

        fila.HoraEntrada3.Should().Be(new TimeOnly(19, 0));
        fila.HoraSalida3.Should().Be(new TimeOnly(21, 0));
    }

    [Fact]
    public void Parse_ConUnArchivoQueNoEsTimesheet_ShouldLanzarFormatException()
    {
        using var wb = new XLWorkbook();
        wb.Worksheets.Add("Otra cosa").Cell(1, 1).Value = "nada que ver";
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;

        var act = () => new TimesheetImportParser().Parse(ms);

        act.Should().Throw<TimesheetImportFormatException>();
    }

    /// <summary>
    /// Reproduce la plantilla real: cabecera con consultor y mes, encabezados en la fila 7,
    /// una ranura vacia antes de los datos y la fila de totales al final.
    /// </summary>
    private static MemoryStream ConstruirPlantilla(
        string mes = "Mayo",
        int? diaExtra = null,
        bool filaSinHoras = false,
        bool tresBloques = false)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("ConsultorKPG");

        ws.Cell(5, 2).Value = "Timesheet KPG";
        ws.Cell(6, 2).Value = "Consultor";
        ws.Cell(6, 4).Value = "Jonathan Andres Sanchez Silva";
        ws.Cell(6, 7).Value = "Mes";
        ws.Cell(6, 9).Value = mes;
        ws.Cell(6, 10).Value = 2026;

        var col = 2;
        ws.Cell(7, col++).Value = "Fecha";
        ws.Cell(7, col++).Value = "Entrada";
        ws.Cell(7, col++).Value = "Salida";
        ws.Cell(7, col++).Value = "Entrada";
        ws.Cell(7, col++).Value = "Salida";
        if (tresBloques)
        {
            ws.Cell(7, col++).Value = "Entrada";
            ws.Cell(7, col++).Value = "Salida";
        }
        ws.Cell(7, col++).Value = "Total Diario";
        ws.Cell(7, col++).Value = "Cliente";
        ws.Cell(7, col++).Value = "Proyecto";
        ws.Cell(7, col++).Value = "Modalidad";
        ws.Cell(7, col++).Value = "Recurso";
        ws.Cell(7, col++).Value = "Lugar";
        ws.Cell(7, col).Value   = "Descripción Tarea Diaria";

        // Fila 8: ranura vacia de la plantilla.
        var fila = 9;
        EscribirFila(ws, fila++, 11, "Creacion job procesos automaticos", tresBloques);
        EscribirFila(ws, fila++, 8,  "Desarrollo en validacion de identidad", tresBloques);

        if (diaExtra is { } dia)
            EscribirFila(ws, fila++, dia, "Dia fuera de rango", tresBloques);

        if (filaSinHoras)
        {
            var c = 2;
            ws.Cell(fila, c).Value = 15;
            c = tresBloques ? 10 : 8;   // salta los bloques horarios vacios y el total
            ws.Cell(fila, c++).Value = "ACUDEN";
            ws.Cell(fila, c++).Value = "Desarrollo ACUDEN Digital CIMA";
            ws.Cell(fila, c++).Value = "Remoto";
            ws.Cell(fila, c++).Value = "Desarrolladores";
            ws.Cell(fila, c++).Value = "Remoto";
            ws.Cell(fila, c).Value   = "Sin horas";
            fila++;
        }

        ws.Cell(fila + 3, 6).Value = "Total de horas trabajadas:";

        var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;
        return ms;
    }

    private static void EscribirFila(IXLWorksheet ws, int fila, int dia, string descripcion, bool tresBloques)
    {
        var c = 2;
        ws.Cell(fila, c++).Value = dia;
        ws.Cell(fila, c++).Value = new TimeSpan(8, 0, 0);
        ws.Cell(fila, c++).Value = new TimeSpan(13, 0, 0);
        ws.Cell(fila, c++).Value = new TimeSpan(14, 30, 0);
        ws.Cell(fila, c++).Value = new TimeSpan(18, 0, 0);
        if (tresBloques)
        {
            ws.Cell(fila, c++).Value = new TimeSpan(19, 0, 0);
            ws.Cell(fila, c++).Value = new TimeSpan(21, 0, 0);
        }
        ws.Cell(fila, c++).Value = 8.5;                     // total
        ws.Cell(fila, c++).Value = "ACUDEN";
        ws.Cell(fila, c++).Value = "Desarrollo ACUDEN Digital CIMA";
        ws.Cell(fila, c++).Value = "Remoto";
        ws.Cell(fila, c++).Value = "Desarrolladores";
        ws.Cell(fila, c++).Value = "Remoto";
        ws.Cell(fila, c).Value   = descripcion;
    }
}
