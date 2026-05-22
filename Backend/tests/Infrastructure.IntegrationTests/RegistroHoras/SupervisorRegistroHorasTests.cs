using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.RegistroHoras.Commands.CreateRegistroHoras;
using KPG.Timesheet.Application.Features.RegistroHoras.Commands.DeleteRegistroHoras;
using KPG.Timesheet.Application.Features.RegistroHoras.Queries.GetMisRegistros;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using RegistroHorasEntity = KPG.Timesheet.Domain.Entities.RegistroHoras;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.RegistroHoras;

public class SupervisorRegistroHorasTests
{
    private static readonly DateOnly TestToday = new(2026, 5, 14);

    [Fact]
    public async Task Handle_WhenSupervisorCreatesRegistro_ShouldPersistWithSupervisorUserId()
    {
        await using var context = CreateContextWithVentana(3);
        var handler = new CreateRegistroHorasCommandHandler(context, new TestUser("supervisor-1"), new TestClock(TestToday), Substitute.For<IBitacoraService>());

        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        result.Id.Should().BeGreaterThan(0);
        result.UserId.Should().Be("supervisor-1");
        context.RegistrosHoras.Should().ContainSingle(r =>
            r.UserId == "supervisor-1" &&
            r.HoraEntradaAM == new TimeOnly(8, 0));
    }

    [Fact]
    public async Task Handle_WhenSupervisorQueriesHistorial_ShouldReturnOnlyOwnRecords()
    {
        await using var context = CreateContextWithVentana(3);
        context.RegistrosHoras.Add(MakeRegistro("supervisor-1", new DateOnly(2026, 5, 10)));
        context.RegistrosHoras.Add(MakeRegistro("empleado-1", new DateOnly(2026, 5, 11)));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetMisRegistrosQueryHandler(context, new TestUser("supervisor-1"));
        var result = await handler.Handle(new GetMisRegistrosQuery(null, null), CancellationToken.None);

        result.Should().HaveCount(1);
        result.Single().Cliente.Should().Be("KPG");
    }

    [Fact]
    public async Task Handle_WhenSupervisorDeletesOwnRegistro_ShouldRemoveIt()
    {
        await using var context = CreateContextWithVentana(3);
        var registro = MakeRegistro("supervisor-1", new DateOnly(2026, 5, 10));
        context.RegistrosHoras.Add(registro);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteRegistroHorasCommandHandler(context, Substitute.For<IBitacoraService>(), new TestUser("supervisor-1"));
        await handler.Handle(new DeleteRegistroHorasCommand(registro.Id), CancellationToken.None);

        context.RegistrosHoras.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenSupervisorDeletesOtherUserRegistro_ShouldSucceed()
    {
        await using var context = CreateContextWithVentana(3);
        var registro = MakeRegistro("empleado-1", new DateOnly(2026, 5, 10));
        context.RegistrosHoras.Add(registro);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteRegistroHorasCommandHandler(context, Substitute.For<IBitacoraService>(), new TestUser("supervisor-1"));
        await handler.Handle(new DeleteRegistroHorasCommand(registro.Id), CancellationToken.None);

        context.RegistrosHoras.Should().BeEmpty();
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

    private static RegistroHorasEntity MakeRegistro(string userId, DateOnly fecha) =>
        new(userId, fecha,
            new TimeOnly(8, 0), new TimeOnly(13, 0),
            null, null,
            "KPG", "Timesheet", "Remoto", "Consultor", "Desarrollo", "Bogota");

    private static CreateRegistroHorasCommand ValidCommand() =>
        new(new DateOnly(2026, 5, 14),
            new TimeOnly(8, 0), new TimeOnly(13, 0),
            null, null,
            "KPG", "Timesheet", "Remoto", "Consultor", "Desarrollo", "Bogota");

    private sealed class TestUser : IUser
    {
        public TestUser(string id) => Id = id;
        public string? Id { get; }
        public string? Email => null;
        public List<string>? Roles => [KPG.Timesheet.Domain.Constants.Roles.Supervisor];
    }

    private sealed class TestClock : IClock
    {
        public TestClock(DateOnly today) => Today = today;
        public DateOnly Today { get; }
        public DateTimeOffset UtcNow => Today.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
    }
}
