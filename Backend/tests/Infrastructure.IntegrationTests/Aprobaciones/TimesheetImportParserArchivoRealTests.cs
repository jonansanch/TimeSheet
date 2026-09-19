using KPG.Timesheet.Infrastructure.Reportes;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.Aprobaciones;

/// <summary>
/// Prueba el parser contra el archivo real que entrego el cliente, si esta disponible en
/// la maquina. Se omite en CI y en cualquier equipo que no lo tenga: sirve para validar
/// el formato de verdad, no como red de seguridad permanente.
/// </summary>
public class TimesheetImportParserArchivoRealTests
{
    private const string Ruta =
        @"C:\Users\jonat\Downloads\Jonathan Andres Sanchez Silva - Mayo (3) - copia.xlsx";

    [Fact]
    public void Parse_ElArchivoRealDelCliente_ShouldLeerLasSeisFilas()
    {
        // Sin paquete de tests omitibles: si el archivo no esta, el test no aplica.
        if (!File.Exists(Ruta)) return;

        using var fs = File.OpenRead(Ruta);
        var resultado = new TimesheetImportParser().Parse(fs);

        resultado.NombreConsultor.Should().Be("Jonathan Andres Sanchez Silva");
        resultado.Mes.Should().Be(5);
        resultado.Anio.Should().Be(2026);
        resultado.Errores.Should().BeEmpty();
        resultado.Filas.Should().HaveCount(6);

        // Las filas vienen en orden descendente de dia, tal cual el archivo.
        resultado.Filas.Select(f => f.Fecha.Day).Should().Equal(11, 8, 7, 6, 5, 4);

        var primera = resultado.Filas[0];
        primera.Fecha.Should().Be(new DateOnly(2026, 5, 11));
        primera.HoraEntrada1.Should().Be(new TimeOnly(8, 0));
        primera.HoraSalida1.Should().Be(new TimeOnly(13, 0));
        primera.HoraEntrada2.Should().Be(new TimeOnly(14, 30));
        primera.HoraSalida2.Should().Be(new TimeOnly(18, 0));
        primera.HoraEntrada3.Should().BeNull();
        primera.Cliente.Should().Be("ACUDEN");
        primera.Proyecto.Should().Be("Desarrollo ACUDEN Digital CIMA");
        primera.Modalidad.Should().Be("Remoto");
        primera.Recurso.Should().Be("Desarrolladores");
        primera.Lugar.Should().Be("Remoto");
        primera.Descripcion.Should().StartWith("Creacion job");

        // Las descripciones multilinea del archivo deben conservarse enteras.
        resultado.Filas.Should().Contain(f => f.Descripcion.Contains('\n'));
    }
}
