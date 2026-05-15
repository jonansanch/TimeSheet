using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.RegistroHoras.Commands.CreateRegistroHoras;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Domain.Enums;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

// TestClock fija la fecha para que los tests no dependan de DateTime.Today

namespace KPG.Timesheet.Infrastructure.IntegrationTests.RegistroHoras;

public class CreateRegistroHorasCommandHandlerTests
{
    private static readonly DateOnly TestToday = new(2026, 5, 14);

    [Fact]
    public async Task Handle_WhenValid_ShouldPersistRegistroForAuthenticatedUser()
    {
        await using var context = CreateContextWithVentana(3);
        var handler = new CreateRegistroHorasCommandHandler(context, new TestUser("user-1"), new TestClock(TestToday));

        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        result.Id.Should().BeGreaterThan(0);
        result.UserId.Should().Be("user-1");
        context.RegistrosHoras.Should().ContainSingle(r =>
            r.UserId == "user-1" &&
            r.FechaRegistro == new DateOnly(2026, 5, 14) &&
            r.Turno == TurnoRegistro.AM);
    }

    [Fact]
    public async Task Handle_WhenSameDateAndTurno_ShouldAllowMultipleRegistros()
    {
        await using var context = CreateContextWithVentana(3);
        var handler = new CreateRegistroHorasCommandHandler(context, new TestUser("user-1"), new TestClock(TestToday));

        await handler.Handle(ValidCommand(), CancellationToken.None);
        await handler.Handle(ValidCommand(), CancellationToken.None);

        context.RegistrosHoras.Count(r => r.UserId == "user-1" && r.Turno == TurnoRegistro.AM).Should().Be(2);
    }

    [Fact]
    public async Task Handle_WhenFechaFueraVentanaConExcepcionAprobada_ShouldCreateRegistro()
    {
        await using var context = CreateContextWithVentana(3);
        var fechaFuera = TestToday.AddDays(-10); // muy fuera de la ventana de 3 días
        var solicitud = new SolicitudExcepcion("user-1", fechaFuera, "Enfermedad grave");
        solicitud.Aprobar();
        context.SolicitudesExcepcion.Add(solicitud);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateRegistroHorasCommandHandler(context, new TestUser("user-1"), new TestClock(TestToday));
        var command = new CreateRegistroHorasCommand(
            fechaFuera, TurnoRegistro.AM,
            new TimeOnly(8, 0), new TimeOnly(13, 0),
            "KPG", "Timesheet", "Remoto", "Consultor", "Desarrollo", "Bogota");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Id.Should().BeGreaterThan(0);
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

    private static CreateRegistroHorasCommand ValidCommand() =>
        new(
            new DateOnly(2026, 5, 14),
            TurnoRegistro.AM,
            new TimeOnly(8, 0),
            new TimeOnly(13, 0),
            "KPG",
            "Timesheet",
            "Remoto",
            "Consultor",
            "Desarrollo",
            "Bogota");

    private sealed class TestUser : IUser
    {
        public TestUser(string id)
        {
            Id = id;
        }

        public string? Id { get; }
        public List<string>? Roles => [KPG.Timesheet.Domain.Constants.Roles.Empleado];
    }

    private sealed class TestClock : IClock
    {
        public TestClock(DateOnly today) => Today = today;
        public DateOnly Today { get; }
    }
}
