using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Aprobaciones;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Domain.Enums;
using KPG.Timesheet.Infrastructure.Data;
using KPG.Timesheet.Infrastructure.Identity;
using KPG.Timesheet.Infrastructure.Organizacion;
using Microsoft.EntityFrameworkCore;
using ForbiddenAccessException = KPG.Timesheet.Application.Common.Exceptions.ForbiddenAccessException;
using ValidationException = KPG.Timesheet.Application.Common.Exceptions.ValidationException;
using RegistroHorasEntity = KPG.Timesheet.Domain.Entities.RegistroHoras;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.Aprobaciones;

/// <summary>
/// Autorizacion de la cadena: cada nivel lo aprueba unicamente su supervisor asignado.
/// </summary>
public class AprobacionCommandsTests
{
    private const string Empleado  = "user-empleado";
    private const string SupPuesto = "user-sup-puesto";   // nivel 1
    private const string Jefe      = "user-jefe";         // nivel 2
    private const string SupProy   = "user-sup-proyecto"; // nivel 3
    private const string Ajeno     = "user-ajeno";

    [Fact]
    public async Task Aprobar_ConElSupervisorDelNivel_ShouldAvanzar()
    {
        await using var context = await EscenarioAsync();
        var id = await RegistroIdAsync(context);

        var dto = await Aprobar(context, id, SupPuesto);

        dto.Estado.Should().Be(EstadoAprobacion.AprobadoNivel1);
        dto.NivelPendiente.Should().Be(2);
    }

    [Fact]
    public async Task Aprobar_ConOtroSupervisor_ShouldForbid()
    {
        // El jefe directo no puede adelantarse al supervisor del puesto.
        await using var context = await EscenarioAsync();
        var id = await RegistroIdAsync(context);

        var act = () => Aprobar(context, id, Jefe);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Aprobar_ConAlguienAjenoALaCadena_ShouldForbid()
    {
        await using var context = await EscenarioAsync();
        var id = await RegistroIdAsync(context);

        var act = () => Aprobar(context, id, Ajeno);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Aprobar_CuandoLaMismaPersonaOcupaDosNiveles_ShouldAvanzarAmbos()
    {
        // Pedirle dos confirmaciones seguidas a la misma persona no aporta nada.
        await using var context = await EscenarioAsync(jefeEsSupervisorDePuesto: true);
        var id = await RegistroIdAsync(context);

        var dto = await Aprobar(context, id, SupPuesto);

        dto.Estado.Should().Be(EstadoAprobacion.AprobadoNivel2);
        dto.NivelPendiente.Should().Be(3);
    }

    [Fact]
    public async Task Aprobar_LaCadenaCompleta_ShouldTerminarAprobado()
    {
        await using var context = await EscenarioAsync();
        var id = await RegistroIdAsync(context);

        await Aprobar(context, id, SupPuesto);
        await Aprobar(context, id, Jefe);
        var dto = await Aprobar(context, id, SupProy);

        dto.Estado.Should().Be(EstadoAprobacion.Aprobado);
        dto.NivelPendiente.Should().BeNull();
    }

    [Fact]
    public async Task Aprobar_UnNivelSinAprobadorAsignado_ShouldForbid()
    {
        // Sin supervisor configurado no lo aprueba nadie: primero hay que asignarlo.
        await using var context = await EscenarioAsync(conSupervisorDePuesto: false);
        var id = await RegistroIdAsync(context);

        var act = () => Aprobar(context, id, SupPuesto);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Rechazar_ShouldGuardarComentarioYDevolverAlEmpleado()
    {
        await using var context = await EscenarioAsync();
        var id = await RegistroIdAsync(context);
        await Aprobar(context, id, SupPuesto);

        var dto = await Rechazar(context, id, Jefe, "Las horas del martes no corresponden.");

        dto.Estado.Should().Be(EstadoAprobacion.Rechazado);
        dto.ComentarioRechazo.Should().Be("Las horas del martes no corresponden.");
    }

    [Fact]
    public async Task Rechazar_ConOtroSupervisor_ShouldForbid()
    {
        await using var context = await EscenarioAsync();
        var id = await RegistroIdAsync(context);

        var act = () => Rechazar(context, id, SupProy, "Motivo.");

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Reenviar_ShouldRecomenzarDesdeElPrimerNivel()
    {
        await using var context = await EscenarioAsync();
        var id = await RegistroIdAsync(context);
        await Aprobar(context, id, SupPuesto);
        await Rechazar(context, id, Jefe, "Corregir.");

        var handler = new ReenviarRegistroCommandHandler(context, new NullBitacora(), new TestUser(Empleado));
        var dto = await handler.Handle(new ReenviarRegistroCommand(id), CancellationToken.None);

        dto.Estado.Should().Be(EstadoAprobacion.Pendiente);
        dto.NivelPendiente.Should().Be(1);
        dto.ComentarioRechazo.Should().BeNull();
    }

    [Fact]
    public async Task Reenviar_PorAlguienQueNoEsElDueno_ShouldForbid()
    {
        await using var context = await EscenarioAsync();
        var id = await RegistroIdAsync(context);
        await Rechazar(context, id, SupPuesto, "Motivo.");

        var handler = new ReenviarRegistroCommandHandler(context, new NullBitacora(), new TestUser(Jefe));
        var act = () => handler.Handle(new ReenviarRegistroCommand(id), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task RevertirAprobacion_PorQuienAprobo_ShouldRetroceder()
    {
        await using var context = await EscenarioAsync();
        var id = await RegistroIdAsync(context);
        await Aprobar(context, id, SupPuesto);

        var handler = new RevertirAprobacionCommandHandler(
            context, new CadenaAprobacionService(context), new NullBitacora(), new TestUser(SupPuesto));
        var dto = await handler.Handle(new RevertirAprobacionCommand(id), CancellationToken.None);

        dto.Estado.Should().Be(EstadoAprobacion.Pendiente);
    }

    [Fact]
    public async Task RevertirAprobacion_PorOtroSupervisor_ShouldForbid()
    {
        await using var context = await EscenarioAsync();
        var id = await RegistroIdAsync(context);
        await Aprobar(context, id, SupPuesto);

        var handler = new RevertirAprobacionCommandHandler(
            context, new CadenaAprobacionService(context), new NullBitacora(), new TestUser(Jefe));
        var act = () => handler.Handle(new RevertirAprobacionCommand(id), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task RevertirRechazo_PorQuienRechazo_ShouldVolverAlPuntoExacto()
    {
        await using var context = await EscenarioAsync();
        var id = await RegistroIdAsync(context);
        await Aprobar(context, id, SupPuesto);
        await Rechazar(context, id, Jefe, "Me equivoque.");

        var handler = new RevertirRechazoCommandHandler(
            context, new CadenaAprobacionService(context), new NullBitacora(), new TestUser(Jefe));
        var dto = await handler.Handle(new RevertirRechazoCommand(id), CancellationToken.None);

        dto.Estado.Should().Be(EstadoAprobacion.AprobadoNivel1);
        dto.NivelPendiente.Should().Be(2);
    }

    [Fact]
    public async Task RevertirRechazo_PorAlguienQueNoRechazo_ShouldForbid()
    {
        await using var context = await EscenarioAsync();
        var id = await RegistroIdAsync(context);
        await Rechazar(context, id, SupPuesto, "Motivo.");

        var handler = new RevertirRechazoCommandHandler(
            context, new CadenaAprobacionService(context), new NullBitacora(), new TestUser(Jefe));
        var act = () => handler.Handle(new RevertirRechazoCommand(id), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Fact]
    public async Task Aprobar_UnRegistroYaAprobado_ShouldFailWithValidationError()
    {
        await using var context = await EscenarioAsync();
        var id = await RegistroIdAsync(context);
        await Aprobar(context, id, SupPuesto);
        await Aprobar(context, id, Jefe);
        await Aprobar(context, id, SupProy);

        var act = () => Aprobar(context, id, SupProy);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Aprobar_ShouldDejarRastroEnElHistorial()
    {
        await using var context = await EscenarioAsync();
        var id = await RegistroIdAsync(context);

        await Aprobar(context, id, SupPuesto);

        var historial = await context.AprobacionesRegistro.Where(a => a.RegistroHorasId == id).ToListAsync();
        historial.Should().ContainSingle();
        historial[0].Accion.Should().Be(AprobacionRegistro.AccionAprobar);
        historial[0].Nivel.Should().Be(1);
        historial[0].ActorUserId.Should().Be(SupPuesto);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static Task<EstadoRegistroDto> Aprobar(ApplicationDbContext context, int id, string actorId) =>
        new AprobarRegistroCommandHandler(
            context, new CadenaAprobacionService(context), new NullBitacora(), new TestUser(actorId))
            .Handle(new AprobarRegistroCommand(id), CancellationToken.None);

    private static Task<EstadoRegistroDto> Rechazar(
        ApplicationDbContext context, int id, string actorId, string comentario) =>
        new RechazarRegistroCommandHandler(
            context, new CadenaAprobacionService(context), new NullBitacora(),
            new NotificadorEspia(), new TestUser(actorId))
            .Handle(new RechazarRegistroCommand(id, comentario), CancellationToken.None);

    private static Task<int> RegistroIdAsync(ApplicationDbContext context) =>
        context.RegistrosHoras.Select(r => r.Id).FirstAsync();

    private static async Task<ApplicationDbContext> EscenarioAsync(
        bool conSupervisorDePuesto = true,
        bool jefeEsSupervisorDePuesto = false)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);

        var cliente = new Cliente("Banco Nacional");
        context.Clientes.Add(cliente);
        var puesto = new Empleado("Consultor");
        context.Empleados.Add(puesto);
        await context.SaveChangesAsync(CancellationToken.None);

        var proyecto = new Proyecto(cliente.Id, "Core Bancario");
        proyecto.AsignarSupervisor(SupProy);
        context.Proyectos.Add(proyecto);

        if (conSupervisorDePuesto)
            context.SupervisoresPuesto.Add(new SupervisorPuesto(puesto.Id, SupPuesto));

        context.Users.Add(new ApplicationUser
        {
            Id = Empleado,
            UserName = "empleado@kpg.com",
            Email = "empleado@kpg.com",
            PuestoId = puesto.Id,
            SupervisorUserId = jefeEsSupervisorDePuesto ? SupPuesto : Jefe
        });
        await context.SaveChangesAsync(CancellationToken.None);

        context.RegistrosHoras.Add(new RegistroHorasEntity(
            Empleado, new DateOnly(2026, 5, 14),
            new TimeOnly(8, 0), new TimeOnly(13, 0),
            null, null, null, null,
            proyecto.Id, "Banco Nacional", "Core Bancario",
            "Remoto", "Consultor", "Desarrollo", "Bogota"));
        await context.SaveChangesAsync(CancellationToken.None);

        return context;
    }

    private sealed class TestUser(string id) : IUser
    {
        public string? Id => id;
        public string? Email => null;
        public List<string>? Roles => [KPG.Timesheet.Domain.Constants.Roles.Supervisor];
    }

    /// <summary>Registra la llamada sin enviar nada: el envio real se prueba aparte.</summary>
    private sealed class NotificadorEspia : INotificadorAprobacion
    {
        public Task NotificarRechazoAsync(string empleadoUserId, DateOnly fechaRegistro,
            string proyecto, string comentario, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class NullBitacora : IBitacoraService
    {
        public Task RegistrarAsync(string tipoEvento, string actorId, string? actorEmail,
            string entidadAfectada, string? entidadId, object? metadata = null,
            CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
