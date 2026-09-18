using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Infrastructure.Data;
using KPG.Timesheet.Infrastructure.Identity;
using KPG.Timesheet.Infrastructure.Organizacion;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.Organizacion;

public class CadenaAprobacionServiceTests
{
    private const string Empleado      = "user-empleado";
    private const string JefeDirecto   = "user-jefe";
    private const string SupPuesto     = "user-sup-puesto";
    private const string SupPuestoBanco = "user-sup-puesto-banco";
    private const string SupProyecto   = "user-sup-proyecto";

    [Fact]
    public async Task Resolver_ShouldReturnTheThreeLevelsInOrder()
    {
        await using var context = await CrearEscenarioAsync();

        var cadena = await Resolver(context, Empleado, "Banco Nacional", "Core Bancario");

        cadena.SupervisorPuestoUserId.Should().Be(SupPuesto);
        cadena.SupervisorUsuarioUserId.Should().Be(JefeDirecto);
        cadena.SupervisorProyectoUserId.Should().Be(SupProyecto);
        cadena.Niveles.Should().Equal(SupPuesto, JefeDirecto, SupProyecto);
    }

    [Fact]
    public async Task Resolver_WhenClientSpecificRuleExists_ShouldPreferItOverGeneral()
    {
        await using var context = await CrearEscenarioAsync();
        var banco  = await context.Clientes.FirstAsync(c => c.Nombre == "Banco Nacional");
        var puesto = await context.Empleados.FirstAsync(e => e.Nombre == "Consultor");
        context.SupervisoresPuesto.Add(new SupervisorPuesto(puesto.Id, SupPuestoBanco, banco.Id));
        await context.SaveChangesAsync(CancellationToken.None);

        var cadena = await Resolver(context, Empleado, "Banco Nacional", "Core Bancario");

        cadena.SupervisorPuestoUserId.Should().Be(SupPuestoBanco);
    }

    [Fact]
    public async Task Resolver_WhenClientSpecificRuleIsForAnotherClient_ShouldFallBackToGeneral()
    {
        await using var context = await CrearEscenarioAsync();
        var salud  = await context.Clientes.FirstAsync(c => c.Nombre == "Ministerio de Salud");
        var puesto = await context.Empleados.FirstAsync(e => e.Nombre == "Consultor");
        context.SupervisoresPuesto.Add(new SupervisorPuesto(puesto.Id, SupPuestoBanco, salud.Id));
        await context.SaveChangesAsync(CancellationToken.None);

        var cadena = await Resolver(context, Empleado, "Banco Nacional", "Core Bancario");

        cadena.SupervisorPuestoUserId.Should().Be(SupPuesto);
    }

    [Fact]
    public async Task Resolver_WhenRuleIsInactive_ShouldIgnoreIt()
    {
        await using var context = await CrearEscenarioAsync();
        var regla = await context.SupervisoresPuesto.FirstAsync();
        regla.Desactivar();
        await context.SaveChangesAsync(CancellationToken.None);

        var cadena = await Resolver(context, Empleado, "Banco Nacional", "Core Bancario");

        cadena.SupervisorPuestoUserId.Should().BeNull();
    }

    [Fact]
    public async Task Resolver_WhenUserHasNoPuesto_ShouldSkipFirstLevel()
    {
        await using var context = await CrearEscenarioAsync();
        var usuario = await context.Users.FirstAsync(u => u.Id == Empleado);
        usuario.PuestoId = null;
        await context.SaveChangesAsync(CancellationToken.None);

        var cadena = await Resolver(context, Empleado, "Banco Nacional", "Core Bancario");

        cadena.SupervisorPuestoUserId.Should().BeNull();
        cadena.Niveles.Should().Equal(JefeDirecto, SupProyecto);
    }

    [Fact]
    public async Task Resolver_WhenProjectHasNoSupervisor_ShouldSkipThirdLevel()
    {
        await using var context = await CrearEscenarioAsync();
        var proyecto = await context.Proyectos.FirstAsync(p => p.Nombre == "Core Bancario");
        proyecto.AsignarSupervisor(null);
        await context.SaveChangesAsync(CancellationToken.None);

        var cadena = await Resolver(context, Empleado, "Banco Nacional", "Core Bancario");

        cadena.SupervisorProyectoUserId.Should().BeNull();
        cadena.Niveles.Should().Equal(SupPuesto, JefeDirecto);
    }

    [Fact]
    public async Task Resolver_WhenProjectNameExistsUnderAnotherClient_ShouldNotMatchIt()
    {
        // El nombre de proyecto solo es unico por cliente: buscarlo suelto traeria
        // el supervisor equivocado.
        await using var context = await CrearEscenarioAsync();
        var salud = await context.Clientes.FirstAsync(c => c.Nombre == "Ministerio de Salud");
        var homonimo = new Proyecto(salud.Id, "Core Bancario");
        homonimo.AsignarSupervisor("user-otro-supervisor");
        context.Proyectos.Add(homonimo);
        await context.SaveChangesAsync(CancellationToken.None);

        var cadena = await Resolver(context, Empleado, "Banco Nacional", "Core Bancario");

        cadena.SupervisorProyectoUserId.Should().Be(SupProyecto);
    }

    [Fact]
    public async Task Niveles_WhenSamePersonHoldsConsecutiveLevels_ShouldApproveOnce()
    {
        await using var context = await CrearEscenarioAsync();
        var usuario = await context.Users.FirstAsync(u => u.Id == Empleado);
        usuario.SupervisorUserId = SupPuesto;   // jefe directo == supervisor del puesto
        await context.SaveChangesAsync(CancellationToken.None);

        var cadena = await Resolver(context, Empleado, "Banco Nacional", "Core Bancario");

        cadena.Niveles.Should().Equal(SupPuesto, SupProyecto);
    }

    [Fact]
    public async Task Resolver_WhenUserDoesNotExist_ShouldReturnEmptyChain()
    {
        await using var context = await CrearEscenarioAsync();

        var cadena = await Resolver(context, "user-inexistente", "Banco Nacional", "Core Bancario");

        cadena.Niveles.Should().BeEmpty();
    }

    [Fact]
    public async Task Resolver_WhenClienteIsUnknown_ShouldStillResolveUserSupervisor()
    {
        await using var context = await CrearEscenarioAsync();

        var cadena = await Resolver(context, Empleado, "Cliente Inventado", "Core Bancario");

        cadena.SupervisorUsuarioUserId.Should().Be(JefeDirecto);
        cadena.SupervisorProyectoUserId.Should().BeNull();
        // Sin cliente conocido solo aplica la regla general del puesto.
        cadena.SupervisorPuestoUserId.Should().Be(SupPuesto);
    }

    private static Task<Application.Common.Interfaces.CadenaAprobacionDto> Resolver(
        ApplicationDbContext context, string userId, string cliente, string proyecto) =>
        new CadenaAprobacionService(context).ResolverAsync(userId, cliente, proyecto);

    private static async Task<ApplicationDbContext> CrearEscenarioAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);

        var banco = new Cliente("Banco Nacional");
        var salud = new Cliente("Ministerio de Salud");
        context.Clientes.AddRange(banco, salud);

        var consultor = new Empleado("Consultor");
        context.Empleados.Add(consultor);
        await context.SaveChangesAsync(CancellationToken.None);

        var coreBancario = new Proyecto(banco.Id, "Core Bancario");
        coreBancario.AsignarSupervisor(SupProyecto);
        context.Proyectos.Add(coreBancario);

        context.SupervisoresPuesto.Add(new SupervisorPuesto(consultor.Id, SupPuesto));

        context.Users.Add(new ApplicationUser
        {
            Id = Empleado,
            UserName = "empleado@kpg.com",
            Email = "empleado@kpg.com",
            NombreCompleto = "Juan Pérez",
            PuestoId = consultor.Id,
            SupervisorUserId = JefeDirecto
        });

        await context.SaveChangesAsync(CancellationToken.None);
        return context;
    }
}
