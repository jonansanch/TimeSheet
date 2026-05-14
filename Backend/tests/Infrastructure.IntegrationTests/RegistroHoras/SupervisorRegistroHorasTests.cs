using KPG.Timesheet.Application.Common.Exceptions;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.RegistroHoras.Commands.CreateRegistroHoras;
using KPG.Timesheet.Application.Features.RegistroHoras.Commands.DeleteRegistroHoras;
using KPG.Timesheet.Application.Features.RegistroHoras.Queries.GetMisRegistros;
using KPG.Timesheet.Domain.Enums;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ValidationException = KPG.Timesheet.Application.Common.Exceptions.ValidationException;
using RegistroHorasEntity = KPG.Timesheet.Domain.Entities.RegistroHoras;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.RegistroHoras;

public class SupervisorRegistroHorasTests
{
    [Fact]
    public async Task Handle_WhenSupervisorCreatesRegistro_ShouldPersistWithSupervisorUserId()
    {
        await using var context = CreateContext();
        var handler = new CreateRegistroHorasCommandHandler(context, new TestUser("supervisor-1"));

        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        result.Id.Should().BeGreaterThan(0);
        result.UserId.Should().Be("supervisor-1");
        context.RegistrosHoras.Should().ContainSingle(r =>
            r.UserId == "supervisor-1" &&
            r.Turno == TurnoRegistro.AM);
    }

    [Fact]
    public async Task Handle_WhenSupervisorHasDuplicateRegistro_ShouldThrowValidationException()
    {
        await using var context = CreateContext();
        var handler = new CreateRegistroHorasCommandHandler(context, new TestUser("supervisor-1"));

        await handler.Handle(ValidCommand(), CancellationToken.None);
        var act = () => handler.Handle(ValidCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenSupervisorQueriesHistorial_ShouldReturnOnlyOwnRecords()
    {
        await using var context = CreateContext();
        context.RegistrosHoras.Add(MakeRegistro("supervisor-1", new DateOnly(2026, 5, 10)));
        context.RegistrosHoras.Add(MakeRegistro("empleado-1", new DateOnly(2026, 5, 11)));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetMisRegistrosQueryHandler(context, new TestUser("supervisor-1"));
        var result = await handler.Handle(new GetMisRegistrosQuery(), CancellationToken.None);

        result.Should().HaveCount(1);
        result.Single().Cliente.Should().Be("KPG");
    }

    [Fact]
    public async Task Handle_WhenSupervisorDeletesOwnRegistro_ShouldRemoveIt()
    {
        await using var context = CreateContext();
        var registro = MakeRegistro("supervisor-1", new DateOnly(2026, 5, 10));
        context.RegistrosHoras.Add(registro);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteRegistroHorasCommandHandler(context, new TestUser("supervisor-1"));
        await handler.Handle(new DeleteRegistroHorasCommand(registro.Id), CancellationToken.None);

        context.RegistrosHoras.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenSupervisorDeletesOtherUserRegistro_ShouldThrowForbiddenAccessException()
    {
        await using var context = CreateContext();
        var registro = MakeRegistro("empleado-1", new DateOnly(2026, 5, 10));
        context.RegistrosHoras.Add(registro);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteRegistroHorasCommandHandler(context, new TestUser("supervisor-1"));
        var act = () => handler.Handle(new DeleteRegistroHorasCommand(registro.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static RegistroHorasEntity MakeRegistro(string userId, DateOnly fecha,
        TurnoRegistro turno = TurnoRegistro.AM) =>
        new(userId, fecha, turno,
            new TimeOnly(8, 0), new TimeOnly(13, 0),
            "KPG", "Timesheet", "Remoto", "Consultor", "Desarrollo", "Bogota");

    private static CreateRegistroHorasCommand ValidCommand() =>
        new(new DateOnly(2026, 5, 14), TurnoRegistro.AM,
            new TimeOnly(8, 0), new TimeOnly(13, 0),
            "KPG", "Timesheet", "Remoto", "Consultor", "Desarrollo", "Bogota");

    private sealed class TestUser : IUser
    {
        public TestUser(string id) => Id = id;
        public string? Id { get; }
        public List<string>? Roles => [KPG.Timesheet.Domain.Constants.Roles.Supervisor];
    }
}
