using KPG.Timesheet.Application.Common.Services;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ParametrosSistemaKeys = KPG.Timesheet.Domain.Constants.ParametrosSistema;
using Roles = KPG.Timesheet.Domain.Constants.Roles;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.Sistema;

public class VentanaRetroactividadServiceTests
{
    private const string Empleado = "user-empleado";

    [Fact]
    public async Task GetDias_WhenNoRules_ShouldReturnGlobalValue()
    {
        await using var context = CreateContext(global: 5);

        var dias = await Resolver(context, Empleado, [Roles.Empleado]);

        dias.Should().Be(5);
    }

    [Fact]
    public async Task GetDias_WhenParameterIsMissing_ShouldFallBackToThree()
    {
        await using var context = CreateContext(global: null);

        var dias = await Resolver(context, Empleado, [Roles.Empleado]);

        dias.Should().Be(3);
    }

    [Fact]
    public async Task GetDias_WithRoleRule_ShouldUseIt()
    {
        await using var context = CreateContext(global: 3);
        context.ReglasVentanaRetroactividad.Add(ReglaVentanaRetroactividad.ParaRol(Roles.Empleado, 10));
        await context.SaveChangesAsync(CancellationToken.None);

        var dias = await Resolver(context, Empleado, [Roles.Empleado]);

        dias.Should().Be(10);
    }

    [Fact]
    public async Task GetDias_WithUserRule_ShouldWinOverRoleRule()
    {
        // Aunque la regla del rol sea mas amplia, la de la persona manda.
        await using var context = CreateContext(global: 3);
        context.ReglasVentanaRetroactividad.Add(ReglaVentanaRetroactividad.ParaRol(Roles.Empleado, 30));
        context.ReglasVentanaRetroactividad.Add(ReglaVentanaRetroactividad.ParaUsuario(Empleado, 7));
        await context.SaveChangesAsync(CancellationToken.None);

        var dias = await Resolver(context, Empleado, [Roles.Empleado]);

        dias.Should().Be(7);
    }

    [Fact]
    public async Task GetDias_WithUserRuleMoreRestrictive_ShouldStillWin()
    {
        await using var context = CreateContext(global: 20);
        context.ReglasVentanaRetroactividad.Add(ReglaVentanaRetroactividad.ParaUsuario(Empleado, 1));
        await context.SaveChangesAsync(CancellationToken.None);

        var dias = await Resolver(context, Empleado, [Roles.Empleado]);

        dias.Should().Be(1);
    }

    [Fact]
    public async Task GetDias_WithSeveralRoleRules_ShouldTakeTheWidest()
    {
        // Acumular roles no debe castigar a nadie.
        await using var context = CreateContext(global: 3);
        context.ReglasVentanaRetroactividad.Add(ReglaVentanaRetroactividad.ParaRol(Roles.Empleado, 5));
        context.ReglasVentanaRetroactividad.Add(ReglaVentanaRetroactividad.ParaRol(Roles.Supervisor, 15));
        await context.SaveChangesAsync(CancellationToken.None);

        var dias = await Resolver(context, Empleado, [Roles.Empleado, Roles.Supervisor]);

        dias.Should().Be(15);
    }

    [Fact]
    public async Task GetDias_WhenRuleIsInactive_ShouldIgnoreIt()
    {
        await using var context = CreateContext(global: 3);
        var regla = ReglaVentanaRetroactividad.ParaUsuario(Empleado, 30);
        regla.Desactivar();
        context.ReglasVentanaRetroactividad.Add(regla);
        await context.SaveChangesAsync(CancellationToken.None);

        var dias = await Resolver(context, Empleado, [Roles.Empleado]);

        dias.Should().Be(3);
    }

    [Fact]
    public async Task GetDias_WhenRuleBelongsToAnotherRole_ShouldNotApply()
    {
        await using var context = CreateContext(global: 3);
        context.ReglasVentanaRetroactividad.Add(ReglaVentanaRetroactividad.ParaRol(Roles.Gerente, 30));
        await context.SaveChangesAsync(CancellationToken.None);

        var dias = await Resolver(context, Empleado, [Roles.Empleado]);

        dias.Should().Be(3);
    }

    [Fact]
    public async Task GetDias_WhenThereIsNoAuthenticatedUser_ShouldReturnGlobalValue()
    {
        await using var context = CreateContext(global: 5);
        context.ReglasVentanaRetroactividad.Add(ReglaVentanaRetroactividad.ParaRol(Roles.Empleado, 30));
        await context.SaveChangesAsync(CancellationToken.None);

        var dias = await Resolver(context, null, [Roles.Empleado]);

        dias.Should().Be(5);
    }

    [Fact]
    public async Task GetDiasGlobal_ShouldIgnoreAnyRule()
    {
        // La pantalla de parametros edita el valor base, no la ventana de nadie en concreto.
        await using var context = CreateContext(global: 5);
        context.ReglasVentanaRetroactividad.Add(ReglaVentanaRetroactividad.ParaUsuario(Empleado, 30));
        await context.SaveChangesAsync(CancellationToken.None);

        var dias = await Servicio(context).GetDiasGlobalAsync();

        dias.Should().Be(5);
    }

    [Fact]
    public async Task GetDias_WithZeroDays_ShouldAllowOnlyToday()
    {
        // 0 es un valor legitimo: registrar solo el dia en curso.
        await using var context = CreateContext(global: 3);
        context.ReglasVentanaRetroactividad.Add(ReglaVentanaRetroactividad.ParaUsuario(Empleado, 0));
        await context.SaveChangesAsync(CancellationToken.None);

        var dias = await Resolver(context, Empleado, [Roles.Empleado]);

        dias.Should().Be(0);
    }

    private static Task<int> Resolver(ApplicationDbContext context, string? userId, string[] roles) =>
        Servicio(context).GetDiasAsync(userId, roles);

    private static VentanaRetroactividadService Servicio(ApplicationDbContext context) =>
        new(context, new ParametrosSistemaService(context));

    private static ApplicationDbContext CreateContext(int? global)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);

        if (global.HasValue)
        {
            context.ParametrosSistema.Add(new ParametroSistema
            {
                Clave = ParametrosSistemaKeys.VentanaRetroactividad,
                Valor = global.Value.ToString()
            });
            context.SaveChanges();
        }

        return context;
    }
}
