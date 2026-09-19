using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.SolicitudesExcepcion.Commands.AprobarSolicitudExcepcion;
using KPG.Timesheet.Application.Features.SolicitudesExcepcion.Commands.CreateSolicitudExcepcion;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Domain.Enums;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ValidationException = KPG.Timesheet.Application.Common.Exceptions.ValidationException;
using RegistroHorasEntity = KPG.Timesheet.Domain.Entities.RegistroHoras;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.SolicitudesExcepcion;

/// <summary>
/// El empleado llena el formulario y la justificacion de una vez; al aprobarse la
/// solicitud el registro se crea solo, sin que tenga que volver a teclear nada.
/// </summary>
public class SolicitudConRegistroTests
{
    private static readonly DateOnly Fecha = new(2026, 4, 10);

    [Fact]
    public async Task Crear_ConRegistroAdjunto_ShouldGuardarloEnLaSolicitud()
    {
        await using var context = await EscenarioAsync();

        await Crear(context, conRegistro: true);

        var solicitud = await context.SolicitudesExcepcion.FirstAsync();
        solicitud.TieneRegistro.Should().BeTrue();
        solicitud.ProyectoNombre.Should().Be("Core Bancario");
        solicitud.HoraEntrada1.Should().Be(new TimeOnly(8, 0));
    }

    [Fact]
    public async Task Crear_SinRegistroAdjunto_ShouldSeguirFuncionando()
    {
        // Compatibilidad: la solicitud de solo justificacion sigue siendo valida.
        await using var context = await EscenarioAsync();

        await Crear(context, conRegistro: false);

        var solicitud = await context.SolicitudesExcepcion.FirstAsync();
        solicitud.TieneRegistro.Should().BeFalse();
        solicitud.Justificacion.Should().Be("Estuve incapacitado.");
    }

    [Fact]
    public async Task Crear_ConRegistroIncompleto_ShouldFallar()
    {
        // "Debe dejar llenar todo el registro": si lo adjunta, tiene que venir completo.
        await using var context = await EscenarioAsync();
        var proyectoId = await context.Proyectos.Select(p => p.Id).FirstAsync();

        var act = () => Handler(context).Handle(
            new CreateSolicitudExcepcionCommand(
                Fecha, "Justificacion", proyectoId,
                new TimeOnly(8, 0), new TimeOnly(13, 0),
                null, null, null, null,
                Modalidad: "Remoto", Recurso: null, Lugar: "Bogota", Descripcion: "Trabajo"),
            CancellationToken.None);

        // El recurso falta: el dominio lo rechaza antes de guardar nada.
        await act.Should().ThrowAsync<Domain.Exceptions.DomainRuleException>();
    }

    [Fact]
    public async Task Crear_ConUnProyectoInactivo_ShouldFallar()
    {
        await using var context = await EscenarioAsync();
        var proyecto = await context.Proyectos.FirstAsync();
        proyecto.Desactivar();
        await context.SaveChangesAsync(CancellationToken.None);

        var act = () => Crear(context, conRegistro: true);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Aprobar_ConRegistroAdjunto_ShouldCrearElRegistro()
    {
        await using var context = await EscenarioAsync();
        await Crear(context, conRegistro: true);
        var id = await context.SolicitudesExcepcion.Select(s => s.Id).FirstAsync();

        await Aprobar(context, id);

        var registro = await context.RegistrosHoras.SingleAsync();
        registro.FechaRegistro.Should().Be(Fecha);
        registro.ProyectoNombre.Should().Be("Core Bancario");
        registro.Descripcion.Should().Be("Trabajo del dia");
        registro.EsRetroactivo.Should().BeTrue();
    }

    [Fact]
    public async Task Aprobar_SinRegistroAdjunto_ShouldAprobarSinCrearNada()
    {
        await using var context = await EscenarioAsync();
        await Crear(context, conRegistro: false);
        var id = await context.SolicitudesExcepcion.Select(s => s.Id).FirstAsync();

        await Aprobar(context, id);

        (await context.SolicitudesExcepcion.FirstAsync()).Estado.Should().Be(EstadoSolicitud.Aprobada);
        (await context.RegistrosHoras.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Aprobar_CuandoElEmpleadoYaRegistroEseDia_ShouldNoDuplicar()
    {
        // Pudo registrarlo por otra via mientras la solicitud esperaba.
        await using var context = await EscenarioAsync();
        await Crear(context, conRegistro: true);
        var proyectoId = await context.Proyectos.Select(p => p.Id).FirstAsync();
        context.RegistrosHoras.Add(new RegistroHorasEntity(
            "user-1", Fecha,
            new TimeOnly(9, 0), new TimeOnly(12, 0),
            null, null, null, null,
            proyectoId, "Banco Nacional", "Core Bancario",
            "Remoto", "Consultor", "Registrado por otra via", "Bogota"));
        await context.SaveChangesAsync(CancellationToken.None);

        var id = await context.SolicitudesExcepcion.Select(s => s.Id).FirstAsync();
        await Aprobar(context, id);

        var registros = await context.RegistrosHoras.ToListAsync();
        registros.Should().ContainSingle();
        registros[0].Descripcion.Should().Be("Registrado por otra via");
    }

    [Fact]
    public async Task AdjuntarRegistro_AUnaSolicitudYaAprobada_ShouldFallar()
    {
        var solicitud = new SolicitudExcepcion("user-1", Fecha, "Justificacion");
        solicitud.Aprobar();

        var act = () => solicitud.AdjuntarRegistro(
            1, "Cliente", "Proyecto",
            new TimeOnly(8, 0), new TimeOnly(13, 0),
            null, null, null, null,
            "Remoto", "Consultor", "Bogota", "Trabajo");

        act.Should().Throw<Domain.Exceptions.DomainRuleException>()
            .WithMessage("*pendiente*");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static async Task Crear(ApplicationDbContext context, bool conRegistro)
    {
        var proyectoId = await context.Proyectos.Select(p => p.Id).FirstAsync();

        var command = conRegistro
            ? new CreateSolicitudExcepcionCommand(
                Fecha, "Estuve incapacitado.", proyectoId,
                new TimeOnly(8, 0), new TimeOnly(13, 0),
                null, null, null, null,
                "Remoto", "Consultor", "Bogota", "Trabajo del dia")
            : new CreateSolicitudExcepcionCommand(Fecha, "Estuve incapacitado.");

        await Handler(context).Handle(command, CancellationToken.None);
    }

    private static Task Aprobar(ApplicationDbContext context, int id) =>
        new AprobarSolicitudExcepcionCommandHandler(context, new NullBitacora(), new TestUser("admin-1"))
            .Handle(new AprobarSolicitudExcepcionCommand(id), CancellationToken.None);

    private static CreateSolicitudExcepcionCommandHandler Handler(ApplicationDbContext context) =>
        new(context, new TestUser("user-1"), new NullBitacora());

    private static async Task<ApplicationDbContext> EscenarioAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);

        var cliente = new Cliente("Banco Nacional");
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync(CancellationToken.None);

        context.Proyectos.Add(new Proyecto(cliente.Id, "Core Bancario"));
        await context.SaveChangesAsync(CancellationToken.None);

        return context;
    }

    private sealed class TestUser(string id) : IUser
    {
        public string? Id => id;
        public string? Email => null;
        public List<string>? Roles => [KPG.Timesheet.Domain.Constants.Roles.Empleado];
    }

    private sealed class NullBitacora : IBitacoraService
    {
        public Task RegistrarAsync(string tipoEvento, string actorId, string? actorEmail,
            string entidadAfectada, string? entidadId, object? metadata = null,
            CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
