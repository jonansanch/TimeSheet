using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.RegistroHoras.Queries.GetRegistrosRecientes;
using KPG.Timesheet.Domain.Enums;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using RegistroHorasEntity = KPG.Timesheet.Domain.Entities.RegistroHoras;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.RegistroHoras;

public class GetRegistrosRecientesQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserHasNoRegistros_ShouldReturnEmpty()
    {
        await using var context = CreateContext();
        var handler = new GetRegistrosRecientesQueryHandler(context, new TestUser("user-sin-registros"));

        var result = await handler.Handle(new GetRegistrosRecientesQuery(5), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenUserHasRegistros_ShouldReturnDistinctClienteProyecto()
    {
        await using var context = CreateContext();
        context.RegistrosHoras.Add(MakeRegistro("user-1", "KPG", "Timesheet", new DateOnly(2026, 5, 10)));
        context.RegistrosHoras.Add(MakeRegistro("user-1", "KPG", "Timesheet", new DateOnly(2026, 5, 11), TurnoRegistro.PM));
        context.RegistrosHoras.Add(MakeRegistro("user-1", "Cliente B", "Proyecto X", new DateOnly(2026, 5, 12)));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetRegistrosRecientesQueryHandler(context, new TestUser("user-1"));
        var result = (await handler.Handle(new GetRegistrosRecientesQuery(5), CancellationToken.None)).ToList();

        result.Should().HaveCount(2);
        result[0].Should().Be(new RegistroRecienteDto("Cliente B", "Proyecto X"));
        result[1].Should().Be(new RegistroRecienteDto("KPG", "Timesheet"));
    }

    [Fact]
    public async Task Handle_WhenTopIsSpecified_ShouldLimitResults()
    {
        await using var context = CreateContext();
        for (var i = 1; i <= 6; i++)
            context.RegistrosHoras.Add(MakeRegistro("user-1", $"Cliente {i}", $"Proyecto {i}", new DateOnly(2026, 5, i)));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetRegistrosRecientesQueryHandler(context, new TestUser("user-1"));
        var result = await handler.Handle(new GetRegistrosRecientesQuery(3), CancellationToken.None);

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_ShouldOnlyReturnRegistrosForAuthenticatedUser()
    {
        await using var context = CreateContext();
        context.RegistrosHoras.Add(MakeRegistro("user-1", "KPG", "Timesheet", new DateOnly(2026, 5, 10)));
        context.RegistrosHoras.Add(MakeRegistro("user-2", "Otro", "Otro Proyecto", new DateOnly(2026, 5, 10)));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetRegistrosRecientesQueryHandler(context, new TestUser("user-1"));
        var result = (await handler.Handle(new GetRegistrosRecientesQuery(5), CancellationToken.None)).ToList();

        result.Should().HaveCount(1);
        result[0].Cliente.Should().Be("KPG");
    }

    [Fact]
    public async Task Handle_WhenTopExceedsMaximum_ShouldClampToTen()
    {
        await using var context = CreateContext();
        for (var i = 1; i <= 12; i++)
            context.RegistrosHoras.Add(MakeRegistro("user-1", $"Cliente {i}", $"Proyecto {i}", new DateOnly(2026, 5, 1).AddDays(i)));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetRegistrosRecientesQueryHandler(context, new TestUser("user-1"));
        var result = await handler.Handle(new GetRegistrosRecientesQuery(99), CancellationToken.None);

        result.Should().HaveCount(10);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static RegistroHorasEntity MakeRegistro(
        string userId,
        string cliente,
        string proyecto,
        DateOnly fecha,
        TurnoRegistro turno = TurnoRegistro.AM) =>
        new(userId, fecha, turno,
            new TimeOnly(8, 0), new TimeOnly(13, 0),
            cliente, proyecto, "Remoto", "Consultor", "Desarrollo", "Bogota");

    private sealed class TestUser : IUser
    {
        public TestUser(string id) => Id = id;
        public string? Id { get; }
        public List<string>? Roles => [KPG.Timesheet.Domain.Constants.Roles.Empleado];
    }
}
