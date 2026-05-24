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
        context.RegistrosHoras.Add(MakeRegistro("user-1", "KPG", "Timesheet", new DateOnly(2026, 5, 10)));
        context.RegistrosHoras.Add(MakeRegistro("user-2", "Otro", "Otro Proyecto", new DateOnly(2026, 5, 10)));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetMisRegistrosQueryHandler(context, new TestUser("user-1"));
        var result = (await handler.Handle(new GetMisRegistrosQuery(null, null), CancellationToken.None)).Items.ToList();

        result.Should().HaveCount(1);
        result[0].Cliente.Should().Be("KPG");
    }

    [Fact]
    public async Task Handle_WhenUserHasMultipleRegistros_ShouldReturnOrderedByFechaDesc()
    {
        await using var context = CreateContext();
        context.RegistrosHoras.Add(MakeRegistro("user-1", "A", "P1", new DateOnly(2026, 5, 10)));
        context.RegistrosHoras.Add(MakeRegistro("user-1", "B", "P2", new DateOnly(2026, 5, 12)));
        context.RegistrosHoras.Add(MakeRegistro("user-1", "C", "P3", new DateOnly(2026, 5, 11)));
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
        context.RegistrosHoras.Add(MakeRegistro("user-1", "KPG", "Timesheet", new DateOnly(2026, 5, 10)));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetMisRegistrosQueryHandler(context, new TestUser("user-1"));
        var result = (await handler.Handle(new GetMisRegistrosQuery(null, null), CancellationToken.None)).Items.ToList();

        var item = result[0];
        item.FechaRegistro.Should().Be(new DateOnly(2026, 5, 10));
        item.HoraEntradaAM.Should().Be(new TimeOnly(8, 0));
        item.HoraSalidaAM.Should().Be(new TimeOnly(13, 0));
        item.HoraEntradaPM.Should().BeNull();
        item.HoraSalidaPM.Should().BeNull();
        item.Cliente.Should().Be("KPG");
        item.Proyecto.Should().Be("Timesheet");
        item.Modalidad.Should().Be("Remoto");
        item.Descripcion.Should().Be("Desarrollo");
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
        DateOnly fecha) =>
        new(userId, fecha,
            new TimeOnly(8, 0), new TimeOnly(13, 0),
            null, null,
            cliente, proyecto, "Remoto", "Consultor", "Desarrollo", "Bogota");

    private sealed class TestUser : IUser
    {
        public TestUser(string id) => Id = id;
        public string? Id { get; }
        public string? Email => null;
        public List<string>? Roles => [KPG.Timesheet.Domain.Constants.Roles.Empleado];
    }
}
