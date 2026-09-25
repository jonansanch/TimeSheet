using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Services;
using KPG.Timesheet.Application.Features.Aprobaciones;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Domain.Enums;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.Aprobaciones;

/// <summary>
/// La importacion de Excel es carga historica hecha por un Admin: una descripcion floja
/// nunca debe rechazar la fila, solo aparecer como advertencia (ver
/// Docs/plan-calidad-descripciones.md).
/// </summary>
public class ImportarTimesheetDescripcionTests
{
    [Fact]
    public async Task Handle_ConDescripcionQueMarcaHallazgos_ImportaLaFilaYLaReportaComoAdvertencia()
    {
        await using var context = await CrearContextoAsync();
        var proyecto = await SeedClienteYProyectoAsync(context);

        context.TerminosDescripcion.Add(new TerminoDescripcion(
            "varios", TipoTermino.Palabra, ReglaTermino.ProhibidoSiempre,
            SeveridadTermino.Bloquear, "No es trazable, agrupa tareas distintas."));
        await context.SaveChangesAsync(CancellationToken.None);

        var parser = Substitute.For<ITimesheetImportParser>();
        parser.Parse(Arg.Any<Stream>()).Returns(new TimesheetImportado(
            "Juan Perez", 5, 2026,
            [new FilaTimesheet(
                2, new DateOnly(2026, 5, 14),
                new TimeOnly(8, 0), new TimeOnly(13, 0), null, null, null, null,
                "Banco Nacional", "Core Bancario", "Remoto", "Consultor", "Bogota", "varios")],
            []));

        var handler = new ImportarTimesheetCommandHandler(
            context, parser, Substitute.For<IBitacoraService>(),
            new ValidadorDescripcionService(context, new ParametrosSistemaService(context)),
            ActorFalso());

        var resultado = await handler.Handle(
            new ImportarTimesheetCommand([1], "user-1"), CancellationToken.None);

        // La severidad del termino es Bloquear, pero en importacion nunca bloquea la fila:
        // solo se registra como advertencia.
        resultado.Importadas.Should().Be(1);
        resultado.Errores.Should().BeEmpty();
        resultado.Advertencias.Should().ContainSingle(a => a.NumeroFila == 2);
        context.RegistrosHoras.Should().ContainSingle(r => r.Descripcion == "varios");
    }

    [Fact]
    public async Task Handle_ConDescripcionDeCalidad_NoGeneraAdvertencias()
    {
        await using var context = await CrearContextoAsync();
        await SeedClienteYProyectoAsync(context);

        var parser = Substitute.For<ITimesheetImportParser>();
        parser.Parse(Arg.Any<Stream>()).Returns(new TimesheetImportado(
            "Juan Perez", 5, 2026,
            [new FilaTimesheet(
                2, new DateOnly(2026, 5, 14),
                new TimeOnly(8, 0), new TimeOnly(13, 0), null, null, null, null,
                "Banco Nacional", "Core Bancario", "Remoto", "Consultor", "Bogota",
                "Ajuste en modulo de evidencias para cancelaciones.")],
            []));

        var handler = new ImportarTimesheetCommandHandler(
            context, parser, Substitute.For<IBitacoraService>(),
            new ValidadorDescripcionService(context, new ParametrosSistemaService(context)),
            ActorFalso());

        var resultado = await handler.Handle(
            new ImportarTimesheetCommand([1], "user-1"), CancellationToken.None);

        resultado.Importadas.Should().Be(1);
        resultado.Advertencias.Should().BeEmpty();
    }

    private static IUser ActorFalso()
    {
        var user = Substitute.For<IUser>();
        user.Id.Returns("admin-1");
        return user;
    }

    private static async Task<Proyecto> SeedClienteYProyectoAsync(ApplicationDbContext context)
    {
        var cliente = new Cliente("Banco Nacional");
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync(CancellationToken.None);
        var proyecto = new Proyecto(cliente.Id, "Core Bancario");
        context.Proyectos.Add(proyecto);
        await context.SaveChangesAsync(CancellationToken.None);
        return proyecto;
    }

    private static async Task<ApplicationDbContext> CrearContextoAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();
        return context;
    }
}
