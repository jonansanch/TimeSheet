using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.RegistroHoras.Commands.CreateRegistrosRango;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Domain.Enums;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ValidationException = KPG.Timesheet.Application.Common.Exceptions.ValidationException;
using RegistroHorasEntity = KPG.Timesheet.Domain.Entities.RegistroHoras;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.RegistroHoras;

/// <summary>
/// Es una carga masiva a proposito: no pasa por la ventana de retroactividad, ni por
/// restricciones de dia, ni evita duplicados, y queda aprobada en los 3 niveles de una
/// vez — como la importacion de Excel. Solo se saltan los dias no laborables.
/// </summary>
public class CreateRegistrosRangoCommandHandlerTests
{
    // Jueves 14 de mayo de 2026. La semana: L11 M12 X13 J14 V15 S16 D17.
    private static readonly DateOnly Hoy = new(2026, 5, 14);

    [Fact]
    public async Task Handle_UnaSemanaHabil_ShouldCrearSoloLosDiasLaborables()
    {
        await using var context = await EscenarioAsync();

        var resultado = await Handle(context, new DateOnly(2026, 5, 11), new DateOnly(2026, 5, 14));

        resultado.Creados.Should().Be(4);   // lunes a jueves
        var fechas = await context.RegistrosHoras.Select(r => r.FechaRegistro).ToListAsync();
        fechas.Should().OnlyContain(f => f.DayOfWeek != DayOfWeek.Saturday && f.DayOfWeek != DayOfWeek.Sunday);
    }

    [Fact]
    public async Task Handle_ShouldSaltarDomingosSiempre()
    {
        await using var context = await EscenarioAsync();

        await Handle(context, new DateOnly(2026, 5, 8), new DateOnly(2026, 5, 14));

        var fechas = await context.RegistrosHoras.Select(r => r.FechaRegistro).ToListAsync();
        fechas.Should().NotContain(f => f.DayOfWeek == DayOfWeek.Sunday);
    }

    [Fact]
    public async Task Handle_SinIncluirSabados_ShouldOmitirlos()
    {
        await using var context = await EscenarioAsync();

        await Handle(context, new DateOnly(2026, 5, 8), new DateOnly(2026, 5, 14));

        var fechas = await context.RegistrosHoras.Select(r => r.FechaRegistro).ToListAsync();
        fechas.Should().NotContain(f => f.DayOfWeek == DayOfWeek.Saturday);
    }

    [Fact]
    public async Task Handle_IncluyendoSabados_ShouldCrearlos()
    {
        await using var context = await EscenarioAsync();

        await Handle(context, new DateOnly(2026, 5, 8), new DateOnly(2026, 5, 14), incluirSabados: true);

        var fechas = await context.RegistrosHoras.Select(r => r.FechaRegistro).ToListAsync();
        fechas.Should().Contain(new DateOnly(2026, 5, 9));   // sabado
    }

    [Fact]
    public async Task Handle_ConRestriccionDeDiaActiva_ShouldIgnorarlaYCrearIgual()
    {
        // La carga masiva no consulta restricciones de dia: es una carga ya validada.
        await using var context = await EscenarioAsync();
        context.ParametrosRestriccionDia.Add(
            ParametroRestriccionDia.ParaUsuario(DayOfWeek.Monday, "user-1"));
        await context.SaveChangesAsync(CancellationToken.None);

        var resultado = await Handle(context, new DateOnly(2026, 5, 11), new DateOnly(2026, 5, 14));

        resultado.Creados.Should().Be(4);
        var fechas = await context.RegistrosHoras.Select(r => r.FechaRegistro).ToListAsync();
        fechas.Should().Contain(new DateOnly(2026, 5, 11));   // lunes, pese a la restriccion
    }

    [Fact]
    public async Task Handle_ConDiasFueraDeLaVentanaDeRetroactividad_ShouldCrearlosIgual()
    {
        // Sin ventana de retroactividad configurada (valor por defecto muy corto): la carga
        // masiva no la respeta, a diferencia del registro individual.
        await using var context = await EscenarioAsync(ventanaDias: 1);

        var resultado = await Handle(context, new DateOnly(2026, 5, 8), new DateOnly(2026, 5, 14));

        resultado.Creados.Should().Be(5);   // 8, 11, 12, 13, 14 (viernes a jueves, sin fin de semana)
    }

    [Fact]
    public async Task Handle_ConDiaYaRegistrado_ShouldCrearOtroRegistroSinOmitir()
    {
        // La carga masiva no evita duplicados: es responsabilidad de quien la ejecuta.
        await using var context = await EscenarioAsync();
        context.RegistrosHoras.Add(RegistroExistente(new DateOnly(2026, 5, 13)));
        await context.SaveChangesAsync(CancellationToken.None);

        var resultado = await Handle(context, new DateOnly(2026, 5, 11), new DateOnly(2026, 5, 14));

        resultado.Creados.Should().Be(4);
        (await context.RegistrosHoras.CountAsync(r => r.FechaRegistro == new DateOnly(2026, 5, 13)))
            .Should().Be(2);
    }

    [Fact]
    public async Task Handle_ConFechasFuturas_ShouldCrearlasIgual()
    {
        await using var context = await EscenarioAsync();

        var resultado = await Handle(context, new DateOnly(2026, 5, 14), new DateOnly(2026, 5, 20));

        resultado.Creados.Should().Be(5);   // jueves 14, viernes 15, lunes 18, martes 19, miercoles 20
        (await context.RegistrosHoras.CountAsync()).Should().Be(5);
    }

    [Fact]
    public async Task Handle_ShouldQuedarAprobadoEnLosTresNiveles()
    {
        await using var context = await EscenarioAsync();

        await Handle(context, new DateOnly(2026, 5, 11), new DateOnly(2026, 5, 14));

        var registros = await context.RegistrosHoras.ToListAsync();
        registros.Should().OnlyContain(r => r.Estado == EstadoAprobacion.Aprobado);

        var aprobaciones = await context.AprobacionesRegistro.ToListAsync();
        aprobaciones.Should().HaveCount(registros.Count * 3);
    }

    [Fact]
    public async Task Handle_ShouldMarcarComoRetroactivosSoloLosDiasAnteriores()
    {
        await using var context = await EscenarioAsync();

        await Handle(context, new DateOnly(2026, 5, 11), new DateOnly(2026, 5, 14));

        var registros = await context.RegistrosHoras.ToListAsync();
        registros.Single(r => r.FechaRegistro == Hoy).EsRetroactivo.Should().BeFalse();
        registros.Where(r => r.FechaRegistro < Hoy).Should().OnlyContain(r => r.EsRetroactivo);
    }

    [Fact]
    public async Task Handle_ConUnProyectoInactivo_ShouldFallar()
    {
        await using var context = await EscenarioAsync();
        var proyecto = await context.Proyectos.FirstAsync();
        proyecto.Desactivar();
        await context.SaveChangesAsync(CancellationToken.None);

        var act = () => Handle(context, new DateOnly(2026, 5, 11), new DateOnly(2026, 5, 14));

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_ShouldGuardarLaFotoDelNombreEnCadaRegistro()
    {
        await using var context = await EscenarioAsync();

        await Handle(context, new DateOnly(2026, 5, 11), new DateOnly(2026, 5, 14));

        var registro = await context.RegistrosHoras.FirstAsync();
        registro.ClienteNombre.Should().Be("Banco Nacional");
        registro.ProyectoNombre.Should().Be("Core Bancario");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static async Task<RegistrosRangoResultadoDto> Handle(
        ApplicationDbContext context, DateOnly desde, DateOnly hasta, bool incluirSabados = false)
    {
        var proyectoId = await context.Proyectos.Select(p => p.Id).FirstAsync();
        var handler = new CreateRegistrosRangoCommandHandler(
            context, new TestUser("user-1"), new TestClock(Hoy), new NullBitacora());

        return await handler.Handle(new CreateRegistrosRangoCommand(
            desde, hasta,
            new TimeOnly(8, 0), new TimeOnly(13, 0),
            null, null, null, null,
            proyectoId, "Remoto", "Consultor", "Desarrollo", "Bogota", incluirSabados),
            CancellationToken.None);
    }

    private static RegistroHorasEntity RegistroExistente(DateOnly fecha) =>
        new("user-1", fecha,
            new TimeOnly(8, 0), new TimeOnly(13, 0),
            null, null, null, null,
            1, "Banco Nacional", "Core Bancario",
            "Remoto", "Consultor", "Ya existia", "Bogota");

    private static async Task<ApplicationDbContext> EscenarioAsync(int ventanaDias = 30)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);

        context.ParametrosSistema.Add(new ParametroSistema
        {
            Clave = KPG.Timesheet.Domain.Constants.ParametrosSistema.VentanaRetroactividad,
            Valor = ventanaDias.ToString()
        });

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

    private sealed class TestClock(DateOnly today) : IClock
    {
        public DateOnly Today => today;
        public DateTimeOffset UtcNow => today.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
    }

    private sealed class NullBitacora : IBitacoraService
    {
        public Task RegistrarAsync(string tipoEvento, string actorId, string? actorEmail,
            string entidadAfectada, string? entidadId, object? metadata = null,
            CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
