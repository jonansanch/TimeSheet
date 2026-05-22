using FluentValidation.Results;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.RegistroHoras.Commands.CreateRegistroHoras;
using KPG.Timesheet.Domain.Enums;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ApplicationValidationException = KPG.Timesheet.Application.Common.Exceptions.ValidationException;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.RegistroHoras;

public class VentanaRetroactivoTests
{
    // Referencia: hoy = miércoles 2026-05-14
    private static readonly DateOnly Today = new(2026, 5, 14);

    [Fact]
    public async Task Handle_WhenFechaEsHoy_ShouldSaveWithEsRetroactivoFalse()
    {
        await using var context = CreateContextWithVentana(3);
        var handler = MakeHandler(context, Today);

        var result = await handler.Handle(CommandForDate(Today), CancellationToken.None);

        result.EsRetroactivo.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenFechaEsDentroVentana_ShouldSaveWithEsRetroactivoTrue()
    {
        // 2026-05-12 es lunes = 1 día hábil antes del miércoles 2026-05-14 → dentro de ventana 3 días
        var fechaRetroactiva = new DateOnly(2026, 5, 12);
        await using var context = CreateContextWithVentana(3);
        var handler = MakeHandler(context, Today);

        var result = await handler.Handle(CommandForDate(fechaRetroactiva), CancellationToken.None);

        result.EsRetroactivo.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenFechaEsFueraVentana_ShouldThrowValidationException()
    {
        // 2026-05-07 es jueves = 5+ días hábiles antes del miércoles 2026-05-14 → fuera de ventana 3 días
        var fechaFuera = new DateOnly(2026, 5, 7);
        await using var context = CreateContextWithVentana(3);
        var handler = MakeHandler(context, Today);

        var act = () => handler.Handle(CommandForDate(fechaFuera), CancellationToken.None);

        await act.Should().ThrowAsync<ApplicationValidationException>()
            .Where(e => e.Errors.Any(err => err.Value.Any(msg => msg.Contains("ventana"))));
    }

    [Fact]
    public async Task Handle_WhenVentanaConfigurableA5Dias_ShouldPermitir5DiasHabiles()
    {
        // Con ventana=5 y hoy=miércoles 14, 5 días hábiles atrás = jueves 7
        // 2026-05-08 es viernes = 4 días hábiles atrás → dentro de ventana 5 días
        var fechaDentroVentana5 = new DateOnly(2026, 5, 8);
        await using var context = CreateContextWithVentana(5);
        var handler = MakeHandler(context, Today);

        var result = await handler.Handle(CommandForDate(fechaDentroVentana5), CancellationToken.None);

        result.EsRetroactivo.Should().BeTrue();
    }

    private static ApplicationDbContext CreateContextWithVentana(int dias)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);
        context.ParametrosSistema.Add(new KPG.Timesheet.Domain.Entities.ParametroSistema
        {
            Clave = KPG.Timesheet.Domain.Constants.ParametrosSistema.VentanaRetroactividad,
            Valor = dias.ToString()
        });
        context.SaveChanges();
        return context;
    }

    private static CreateRegistroHorasCommandHandler MakeHandler(ApplicationDbContext context, DateOnly today) =>
        new(context, new TestUser("user-1"), new TestClock(today), new NullBitacora());

    private static CreateRegistroHorasCommand CommandForDate(DateOnly fecha) =>
        new(fecha, TurnoRegistro.AM,
            new TimeOnly(8, 0), new TimeOnly(13, 0),
            "KPG", "Timesheet", "Remoto", "Consultor", "Desarrollo", "Bogota");

    private sealed class TestUser : IUser
    {
        public TestUser(string id) => Id = id;
        public string? Id { get; }
        public string? Email => null;
        public List<string>? Roles => [KPG.Timesheet.Domain.Constants.Roles.Empleado];
    }

    private sealed class TestClock : IClock
    {
        public TestClock(DateOnly today) => Today = today;
        public DateOnly Today { get; }
        public DateTimeOffset UtcNow => Today.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
    }

    private sealed class NullBitacora : IBitacoraService
    {
        public Task RegistrarAsync(string tipoEvento, string actorId, string? actorEmail,
            string entidadAfectada, string? entidadId, object? metadata = null,
            CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
