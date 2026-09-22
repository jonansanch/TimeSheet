using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Services;
using KPG.Timesheet.Application.Features.RegistroHoras.Commands.CreateRegistroHoras;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ApplicationValidationException = KPG.Timesheet.Application.Common.Exceptions.ValidationException;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.RegistroHoras;

/// <summary>
/// La restriccion de dia bloquea la creacion, tanto para un registro individual como
/// para un rango: el rango no es una via para saltarsela.
/// </summary>
public class RestriccionDiaHandlerTests
{
    // 2026-05-18 es lunes.
    private static readonly DateOnly Lunes = new(2026, 5, 18);

    [Fact]
    public async Task CreateRegistroHoras_ConDiaRestringidoParaElUsuario_Throws()
    {
        await using var context = await CreateContextAsync();
        context.ParametrosRestriccionDia.Add(ParametroRestriccionDia.ParaUsuario(DayOfWeek.Monday, "user-1"));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = MakeHandler(context, Lunes);

        var act = () => handler.Handle(CommandForDate(Lunes), CancellationToken.None);

        await act.Should().ThrowAsync<ApplicationValidationException>()
            .Where(e => e.Errors.Any(err => err.Value.Any(msg => msg.Contains("restriccion") || msg.Contains("restricción"))));
    }

    [Fact]
    public async Task CreateRegistroHoras_ConDiaRestringidoParaElRol_Throws()
    {
        await using var context = await CreateContextAsync();
        context.ParametrosRestriccionDia.Add(ParametroRestriccionDia.ParaRol(DayOfWeek.Monday, Roles.Empleado));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = MakeHandler(context, Lunes);

        var act = () => handler.Handle(CommandForDate(Lunes), CancellationToken.None);

        await act.Should().ThrowAsync<ApplicationValidationException>();
    }

    [Fact]
    public async Task CreateRegistroHoras_SinRestriccion_Succeeds()
    {
        await using var context = await CreateContextAsync();
        var handler = MakeHandler(context, Lunes);

        var result = await handler.Handle(CommandForDate(Lunes), CancellationToken.None);

        result.Id.Should().BeGreaterThan(0);
    }

    // La carga por rango (CreateRegistrosRango) no consulta restricciones de dia a
    // proposito: ver CreateRegistrosRangoCommandHandlerTests.
    // Handle_ConRestriccionDeDiaActiva_ShouldIgnorarlaYCrearIgual.

    private static async Task<ApplicationDbContext> CreateContextAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);
        await CatalogoDePrueba.SembrarAsync(context);
        context.ParametrosSistema.Add(new ParametroSistema
        {
            Clave = KPG.Timesheet.Domain.Constants.ParametrosSistema.VentanaRetroactividad,
            Valor = "30"
        });
        await context.SaveChangesAsync(CancellationToken.None);
        return context;
    }

    private static CreateRegistroHorasCommandHandler MakeHandler(ApplicationDbContext context, DateOnly today) =>
        new(context, new TestUser("user-1"), new TestClock(today), new NullBitacora(),
            new VentanaRetroactividadService(context, new ParametrosSistemaService(context)),
            new RestriccionDiaService(context));

    private static CreateRegistroHorasCommand CommandForDate(DateOnly fecha) =>
        new(fecha,
            new TimeOnly(8, 0), new TimeOnly(13, 0),
            null, null,
            null, null,
            1, "Remoto", "Consultor", "Desarrollo", "Bogota");

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
