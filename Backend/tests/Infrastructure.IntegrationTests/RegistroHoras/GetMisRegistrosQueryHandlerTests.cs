using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.RegistroHoras.Queries.GetMisRegistros;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using RegistroHorasEntity = KPG.Timesheet.Domain.Entities.RegistroHoras;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.RegistroHoras;

public class GetMisRegistrosQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserHasNoRegistros_ShouldReturnEmpty()
    {
        await using var context = CreateContext();
        var handler = new GetMisRegistrosQueryHandler(context, new TestUser("user-sin-registros"));

        var result = await handler.Handle(new GetMisRegistrosQuery(null, null), CancellationToken.None);

        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenUserHasRegistros_ShouldReturnOnlyTheirOwnRecords()
    {
        await using var context = CreateContext();
        context.RegistrosHoras.Add(MakeRegistro("user-1", 1, new DateOnly(2026, 5, 10)));
        context.RegistrosHoras.Add(MakeRegistro("user-2", 2, new DateOnly(2026, 5, 10)));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetMisRegistrosQueryHandler(context, new TestUser("user-1"));
        var result = (await handler.Handle(new GetMisRegistrosQuery(null, null), CancellationToken.None)).Items.ToList();

        result.Should().HaveCount(1);
        result[0].Cliente.Should().Be("Cliente 1");
    }

    [Fact]
    public async Task Handle_WhenUserHasMultipleRegistros_ShouldReturnOrderedByFechaDesc()
    {
        await using var context = CreateContext();
        context.RegistrosHoras.Add(MakeRegistro("user-1", 3, new DateOnly(2026, 5, 10)));
        context.RegistrosHoras.Add(MakeRegistro("user-1", 4, new DateOnly(2026, 5, 12)));
        context.RegistrosHoras.Add(MakeRegistro("user-1", 5, new DateOnly(2026, 5, 11)));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetMisRegistrosQueryHandler(context, new TestUser("user-1"));
        var result = (await handler.Handle(new GetMisRegistrosQuery(null, null), CancellationToken.None)).Items.ToList();

        result.Should().HaveCount(3);
        result[0].FechaRegistro.Should().Be(new DateOnly(2026, 5, 12));
        result[1].FechaRegistro.Should().Be(new DateOnly(2026, 5, 11));
        result[2].FechaRegistro.Should().Be(new DateOnly(2026, 5, 10));
    }

    [Fact]
    public async Task Handle_WhenRegistroExists_ShouldMapAllRequiredFields()
    {
        await using var context = CreateContext();
        context.RegistrosHoras.Add(MakeRegistro("user-1", 1, new DateOnly(2026, 5, 10)));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetMisRegistrosQueryHandler(context, new TestUser("user-1"));
        var result = (await handler.Handle(new GetMisRegistrosQuery(null, null), CancellationToken.None)).Items.ToList();

        var item = result[0];
        item.FechaRegistro.Should().Be(new DateOnly(2026, 5, 10));
        item.HoraEntrada1.Should().Be(new TimeOnly(8, 0));
        item.HoraSalida1.Should().Be(new TimeOnly(13, 0));
        item.HoraEntrada2.Should().BeNull();
        item.HoraSalida2.Should().BeNull();
        item.ProyectoId.Should().Be(1);
        item.Cliente.Should().Be("Cliente 1");
        item.Proyecto.Should().Be("Proyecto 1");
        item.Modalidad.Should().Be("Remoto");
        item.Descripcion.Should().Be("Desarrollo");
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);
        CatalogoDePrueba.SembrarAsync(context).GetAwaiter().GetResult();
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
