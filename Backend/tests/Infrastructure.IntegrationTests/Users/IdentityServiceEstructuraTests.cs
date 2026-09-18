using Dapper;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Infrastructure.Data;
using KPG.Timesheet.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.Users;

public class IdentityServiceEstructuraTests
{
    [Fact]
    public async Task AsignarEstructura_ShouldSetSupervisorAndPuesto()
    {
        var (identity, context) = CreateServices();
        var jefe     = await CrearUsuarioAsync(identity, "jefe@kpg.com");
        var empleado = await CrearUsuarioAsync(identity, "empleado@kpg.com");
        var puestoId = await CrearPuestoAsync(context, "Consultor");

        var (result, user) = await identity.AsignarEstructuraAsync(empleado, jefe, puestoId);

        result.Succeeded.Should().BeTrue();
        user!.SupervisorUserId.Should().Be(jefe);
        user.PuestoId.Should().Be(puestoId);
    }

    [Fact]
    public async Task AsignarEstructura_WithNulls_ShouldClearBoth()
    {
        var (identity, context) = CreateServices();
        var jefe     = await CrearUsuarioAsync(identity, "jefe@kpg.com");
        var empleado = await CrearUsuarioAsync(identity, "empleado@kpg.com");
        var puestoId = await CrearPuestoAsync(context, "Consultor");
        await identity.AsignarEstructuraAsync(empleado, jefe, puestoId);

        var (result, user) = await identity.AsignarEstructuraAsync(empleado, null, null);

        result.Succeeded.Should().BeTrue();
        user!.SupervisorUserId.Should().BeNull();
        user.PuestoId.Should().BeNull();
    }

    [Fact]
    public async Task AsignarEstructura_WhenSupervisorIsSelf_ShouldFail()
    {
        var (identity, _) = CreateServices();
        var empleado = await CrearUsuarioAsync(identity, "empleado@kpg.com");

        var (result, _) = await identity.AsignarEstructuraAsync(empleado, empleado, null);

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("su propio supervisor"));
    }

    [Fact]
    public async Task AsignarEstructura_WhenItWouldCreateADirectCycle_ShouldFail()
    {
        // A reporta a B; intentar que B reporte a A cierra el ciclo.
        var (identity, _) = CreateServices();
        var a = await CrearUsuarioAsync(identity, "a@kpg.com");
        var b = await CrearUsuarioAsync(identity, "b@kpg.com");
        await identity.AsignarEstructuraAsync(a, b, null);

        var (result, _) = await identity.AsignarEstructuraAsync(b, a, null);

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("ciclo"));
    }

    [Fact]
    public async Task AsignarEstructura_WhenItWouldCreateAnIndirectCycle_ShouldFail()
    {
        // A → B → C; intentar que C reporte a A cierra un ciclo de tres niveles.
        var (identity, _) = CreateServices();
        var a = await CrearUsuarioAsync(identity, "a@kpg.com");
        var b = await CrearUsuarioAsync(identity, "b@kpg.com");
        var c = await CrearUsuarioAsync(identity, "c@kpg.com");
        await identity.AsignarEstructuraAsync(a, b, null);
        await identity.AsignarEstructuraAsync(b, c, null);

        var (result, _) = await identity.AsignarEstructuraAsync(c, a, null);

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("ciclo"));
    }

    [Fact]
    public async Task AsignarEstructura_WhenHierarchyStaysAcyclic_ShouldSucceed()
    {
        // Dos personas bajo el mismo jefe no es ciclo.
        var (identity, _) = CreateServices();
        var jefe = await CrearUsuarioAsync(identity, "jefe@kpg.com");
        var a    = await CrearUsuarioAsync(identity, "a@kpg.com");
        var b    = await CrearUsuarioAsync(identity, "b@kpg.com");
        await identity.AsignarEstructuraAsync(a, jefe, null);

        var (result, _) = await identity.AsignarEstructuraAsync(b, jefe, null);

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task AsignarEstructura_WhenSupervisorIsInactive_ShouldFail()
    {
        var (identity, _) = CreateServices();
        var jefe     = await CrearUsuarioAsync(identity, "jefe@kpg.com");
        var empleado = await CrearUsuarioAsync(identity, "empleado@kpg.com");
        await identity.DeactivateUserAsync(jefe, "admin-id");

        var (result, _) = await identity.AsignarEstructuraAsync(empleado, jefe, null);

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task AsignarEstructura_WhenPuestoDoesNotExist_ShouldFail()
    {
        var (identity, _) = CreateServices();
        var empleado = await CrearUsuarioAsync(identity, "empleado@kpg.com");

        var (result, _) = await identity.AsignarEstructuraAsync(empleado, null, 9999);

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("puesto"));
    }

    [Fact]
    public async Task GetOrganigrama_ShouldReturnActiveUsersWithTheirSupervisor()
    {
        var (identity, context) = CreateServices();
        var jefe     = await CrearUsuarioAsync(identity, "jefe@kpg.com");
        var empleado = await CrearUsuarioAsync(identity, "empleado@kpg.com");
        var puestoId = await CrearPuestoAsync(context, "Consultor");
        await identity.AsignarEstructuraAsync(empleado, jefe, puestoId);

        var nodos = await identity.GetOrganigramaAsync();

        nodos.Should().HaveCount(2);
        nodos.Single(n => n.UserId == empleado).SupervisorUserId.Should().Be(jefe);
        nodos.Single(n => n.UserId == empleado).PuestoNombre.Should().Be("Consultor");
        nodos.Single(n => n.UserId == jefe).SupervisorUserId.Should().BeNull();
    }

    [Fact]
    public async Task GetOrganigrama_ShouldExcludeInactiveUsers()
    {
        var (identity, _) = CreateServices();
        await CrearUsuarioAsync(identity, "activo@kpg.com");
        var inactivo = await CrearUsuarioAsync(identity, "inactivo@kpg.com");
        await identity.DeactivateUserAsync(inactivo, "admin-id");

        var nodos = await identity.GetOrganigramaAsync();

        nodos.Should().ContainSingle().Which.Email.Should().Be("activo@kpg.com");
    }

    private static async Task<string> CrearUsuarioAsync(IdentityService identity, string email)
    {
        var (result, user) = await identity.CreateUserAsync(email, "Empleado1234!", Roles.Empleado, email);
        result.Succeeded.Should().BeTrue();
        return user!.Id;
    }

    private static async Task<int> CrearPuestoAsync(ApplicationDbContext context, string nombre)
    {
        var puesto = new Empleado(nombre);
        context.Empleados.Add(puesto);
        await context.SaveChangesAsync(CancellationToken.None);
        return puesto.Id;
    }

    private static (IdentityService Identity, ApplicationDbContext Context) CreateServices()
    {
        SqlMapper.AddTypeHandler(new DateTimeOffsetTypeHandler());
        SqlMapper.AddTypeHandler(new NullableDateTimeOffsetTypeHandler());

        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization();
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection));
        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddClaimsPrincipalFactory<UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>>();
        services.AddTransient<IdentityService>();
        services.AddSingleton<System.Data.IDbConnection>(connection);

        var provider = services.BuildServiceProvider();
        var context = provider.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();
        foreach (var role in new[] { Roles.Admin, Roles.Gerente, Roles.Supervisor, Roles.Empleado })
            context.Roles.Add(new IdentityRole(role) { NormalizedName = role.ToUpperInvariant() });
        context.SaveChanges();

        return (provider.GetRequiredService<IdentityService>(), context);
    }
}
