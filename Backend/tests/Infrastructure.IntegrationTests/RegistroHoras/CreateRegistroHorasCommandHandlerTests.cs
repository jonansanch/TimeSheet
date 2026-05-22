using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.RegistroHoras.Commands.CreateRegistroHoras;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.RegistroHoras;

public class CreateRegistroHorasCommandHandlerTests
{
    private static readonly DateOnly TestToday = new(2026, 5, 14);

    [Fact]
    public async Task Handle_WhenValid_ShouldPersistRegistroForAuthenticatedUser()
    {
        await using var context = CreateContextWithVentana(3);
        var handler = new CreateRegistroHorasCommandHandler(context, new TestUser("user-1"), new TestClock(TestToday), new NullBitacora());

        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        result.Id.Should().BeGreaterThan(0);
        result.UserId.Should().Be("user-1");
        context.RegistrosHoras.Should().ContainSingle(r =>
            r.UserId == "user-1" &&
            r.FechaRegistro == new DateOnly(2026, 5, 14) &&
            r.HoraEntradaAM == new TimeOnly(8, 0));
    }

    [Fact]
    public async Task Handle_WhenSameDateSecondCall_ShouldUpsertAddingPMBlock()
    {
        await using var context = CreateContextWithVentana(3);
        var handler = new CreateRegistroHorasCommandHandler(context, new TestUser("user-1"), new TestClock(TestToday), new NullBitacora());

        // First call: AM block only
        await handler.Handle(ValidCommand(), CancellationToken.None);

        // Second call: PM block for the same date
        var pmCommand = new CreateRegistroHorasCommand(
            new DateOnly(2026, 5, 14),
            null, null,
            new TimeOnly(13, 0), new TimeOnly(17, 0),
            "KPG", "Timesheet", "Remoto", "Consultor", "Desarrollo", "Bogota");
        await handler.Handle(pmCommand, CancellationToken.None);

        // Must remain a single record with both blocks
        context.RegistrosHoras.Count(r => r.UserId == "user-1").Should().Be(1);
        var registro = await context.RegistrosHoras.SingleAsync(r => r.UserId == "user-1");
        registro.TieneAM.Should().BeTrue();
        registro.TienePM.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenFechaFueraVentanaConExcepcionAprobada_ShouldCreateRegistro()
    {
        await using var context = CreateContextWithVentana(3);
        var fechaFuera = TestToday.AddDays(-10);
        var solicitud = new SolicitudExcepcion("user-1", fechaFuera, "Enfermedad grave");
        solicitud.Aprobar();
        context.SolicitudesExcepcion.Add(solicitud);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateRegistroHorasCommandHandler(context, new TestUser("user-1"), new TestClock(TestToday), new NullBitacora());
        var command = new CreateRegistroHorasCommand(
            fechaFuera,
            new TimeOnly(8, 0), new TimeOnly(13, 0),
            null, null,
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
            new TimeOnly(8, 0),
            new TimeOnly(13, 0),
            null,
            null,
            "KPG",
            "Timesheet",
            "Remoto",
            "Consultor",
            "Desarrollo",
            "Bogota");

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
