using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.RegistroHoras.Queries.GetRegistrosRecientes;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using RegistroHorasEntity = KPG.Timesheet.Domain.Entities.RegistroHoras;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.RegistroHoras;

/// <summary>
/// Desde la migracion a ProyectoId el handler resuelve los nombres por join, asi que
/// estos tests necesitan un catalogo real de clientes y proyectos.
/// </summary>
public class GetRegistrosRecientesQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserHasNoRegistros_ShouldReturnEmpty()
    {
        await using var context = await CreateContextAsync(proyectos: 1);
        var handler = new GetRegistrosRecientesQueryHandler(context, new TestUser("user-sin-registros"));

        var result = await handler.Handle(new GetRegistrosRecientesQuery(5), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenSameProjectOnSeveralDates_ShouldReturnItOnce()
    {
        await using var context = await CreateContextAsync(proyectos: 2);
        context.RegistrosHoras.Add(MakeRegistro("user-1", 1, new DateOnly(2026, 5, 10)));
        context.RegistrosHoras.Add(MakeRegistro("user-1", 1, new DateOnly(2026, 5, 11)));
        context.RegistrosHoras.Add(MakeRegistro("user-1", 2, new DateOnly(2026, 5, 12)));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetRegistrosRecientesQueryHandler(context, new TestUser("user-1"));
        var result = (await handler.Handle(new GetRegistrosRecientesQuery(5), CancellationToken.None)).ToList();

        result.Should().HaveCount(2);
        result[0].ProyectoId.Should().Be(2);   // el más reciente primero
        result[1].ProyectoId.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldResolveClienteAndProyectoNames()
    {
        await using var context = await CreateContextAsync(proyectos: 1);
        context.RegistrosHoras.Add(MakeRegistro("user-1", 1, new DateOnly(2026, 5, 10)));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetRegistrosRecientesQueryHandler(context, new TestUser("user-1"));
        var result = (await handler.Handle(new GetRegistrosRecientesQuery(5), CancellationToken.None)).ToList();

        result.Should().ContainSingle();
        result[0].Cliente.Should().Be("Cliente 1");
        result[0].Proyecto.Should().Be("Proyecto 1");
    }

    [Fact]
    public async Task Handle_WhenTopIsSpecified_ShouldLimitResults()
    {
        await using var context = await CreateContextAsync(proyectos: 6);
        for (var i = 1; i <= 6; i++)
            context.RegistrosHoras.Add(MakeRegistro("user-1", i, new DateOnly(2026, 5, i)));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetRegistrosRecientesQueryHandler(context, new TestUser("user-1"));
        var result = await handler.Handle(new GetRegistrosRecientesQuery(3), CancellationToken.None);

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_ShouldOnlyReturnRegistrosForAuthenticatedUser()
    {
        await using var context = await CreateContextAsync(proyectos: 2);
        context.RegistrosHoras.Add(MakeRegistro("user-1", 1, new DateOnly(2026, 5, 10)));
        context.RegistrosHoras.Add(MakeRegistro("user-2", 2, new DateOnly(2026, 5, 10)));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetRegistrosRecientesQueryHandler(context, new TestUser("user-1"));
        var result = (await handler.Handle(new GetRegistrosRecientesQuery(5), CancellationToken.None)).ToList();

        result.Should().ContainSingle();
        result[0].ProyectoId.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WhenTopExceedsMaximum_ShouldClampToTen()
    {
        await using var context = await CreateContextAsync(proyectos: 12);
        for (var i = 1; i <= 12; i++)
            context.RegistrosHoras.Add(MakeRegistro("user-1", i, new DateOnly(2026, 5, 1).AddDays(i)));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetRegistrosRecientesQueryHandler(context, new TestUser("user-1"));
        var result = await handler.Handle(new GetRegistrosRecientesQuery(99), CancellationToken.None);

        result.Should().HaveCount(10);
    }

    /// <summary>Crea N clientes con un proyecto cada uno, de modo que ProyectoId == N.</summary>
    private static async Task<ApplicationDbContext> CreateContextAsync(int proyectos)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);

        for (var i = 1; i <= proyectos; i++)
        {
            var cliente = new Cliente($"Cliente {i}");
            context.Clientes.Add(cliente);
            await context.SaveChangesAsync(CancellationToken.None);
            context.Proyectos.Add(new Proyecto(cliente.Id, $"Proyecto {i}"));
            await context.SaveChangesAsync(CancellationToken.None);
        }

        return context;
    }

    private static RegistroHorasEntity MakeRegistro(
        string userId,
        int proyectoId,
        DateOnly fecha) =>
        new(userId, fecha,
            new TimeOnly(8, 0), new TimeOnly(13, 0),
            null, null,
            null, null,
            proyectoId, $"Cliente {proyectoId}", $"Proyecto {proyectoId}", "Remoto", "Consultor", "Desarrollo", "Bogota");

    private sealed class TestUser : IUser
    {
        public TestUser(string id) => Id = id;
        public string? Id { get; }
        public string? Email => null;
        public List<string>? Roles => [KPG.Timesheet.Domain.Constants.Roles.Empleado];
    }
}
