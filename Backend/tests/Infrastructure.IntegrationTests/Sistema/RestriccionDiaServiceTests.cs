using KPG.Timesheet.Application.Common.Services;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.Sistema;

public class RestriccionDiaServiceTests
{
    // 2026-05-18 es lunes.
    private static readonly DateOnly Lunes = new(2026, 5, 18);

    [Fact]
    public async Task PuedeRegistrarEnDiaAsync_SinRestricciones_Permite()
    {
        await using var context = CreateContext();

        var (puede, motivo) = await Service(context)
            .PuedeRegistrarEnDiaAsync("user-1", [Roles.Empleado], Lunes, CancellationToken.None);

        puede.Should().BeTrue();
        motivo.Should().BeNull();
    }

    [Fact]
    public async Task PuedeRegistrarEnDiaAsync_ConRestriccionDeUsuario_Bloquea()
    {
        await using var context = CreateContext();
        context.ParametrosRestriccionDia.Add(ParametroRestriccionDia.ParaUsuario(DayOfWeek.Monday, "user-1"));
        await context.SaveChangesAsync(CancellationToken.None);

        var (puede, motivo) = await Service(context)
            .PuedeRegistrarEnDiaAsync("user-1", [Roles.Empleado], Lunes, CancellationToken.None);

        puede.Should().BeFalse();
        motivo.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task PuedeRegistrarEnDiaAsync_ConRestriccionDeOtroUsuario_NoAfecta()
    {
        await using var context = CreateContext();
        context.ParametrosRestriccionDia.Add(ParametroRestriccionDia.ParaUsuario(DayOfWeek.Monday, "otro-usuario"));
        await context.SaveChangesAsync(CancellationToken.None);

        var (puede, _) = await Service(context)
            .PuedeRegistrarEnDiaAsync("user-1", [Roles.Empleado], Lunes, CancellationToken.None);

        puede.Should().BeTrue();
    }

    [Fact]
    public async Task PuedeRegistrarEnDiaAsync_ConRestriccionDeRol_Bloquea()
    {
        await using var context = CreateContext();
        context.ParametrosRestriccionDia.Add(ParametroRestriccionDia.ParaRol(DayOfWeek.Monday, Roles.Empleado));
        await context.SaveChangesAsync(CancellationToken.None);

        var (puede, motivo) = await Service(context)
            .PuedeRegistrarEnDiaAsync("user-1", [Roles.Empleado], Lunes, CancellationToken.None);

        puede.Should().BeFalse();
        motivo.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task PuedeRegistrarEnDiaAsync_ConRestriccionDeOtroRol_NoAfecta()
    {
        await using var context = CreateContext();
        context.ParametrosRestriccionDia.Add(ParametroRestriccionDia.ParaRol(DayOfWeek.Monday, Roles.Gerente));
        await context.SaveChangesAsync(CancellationToken.None);

        var (puede, _) = await Service(context)
            .PuedeRegistrarEnDiaAsync("user-1", [Roles.Empleado], Lunes, CancellationToken.None);

        puede.Should().BeTrue();
    }

    [Fact]
    public async Task PuedeRegistrarEnDiaAsync_ConRestriccionInactiva_NoAfecta()
    {
        await using var context = CreateContext();
        var restriccion = ParametroRestriccionDia.ParaUsuario(DayOfWeek.Monday, "user-1");
        restriccion.Desactivar();
        context.ParametrosRestriccionDia.Add(restriccion);
        await context.SaveChangesAsync(CancellationToken.None);

        var (puede, _) = await Service(context)
            .PuedeRegistrarEnDiaAsync("user-1", [Roles.Empleado], Lunes, CancellationToken.None);

        puede.Should().BeTrue();
    }

    [Fact]
    public async Task PuedeRegistrarEnDiaAsync_ConRestriccionDeOtroDia_NoAfecta()
    {
        await using var context = CreateContext();
        context.ParametrosRestriccionDia.Add(ParametroRestriccionDia.ParaUsuario(DayOfWeek.Friday, "user-1"));
        await context.SaveChangesAsync(CancellationToken.None);

        var (puede, _) = await Service(context)
            .PuedeRegistrarEnDiaAsync("user-1", [Roles.Empleado], Lunes, CancellationToken.None);

        puede.Should().BeTrue();
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static RestriccionDiaService Service(ApplicationDbContext context) => new(context);
}
