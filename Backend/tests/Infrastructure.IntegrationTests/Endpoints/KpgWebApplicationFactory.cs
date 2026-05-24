using System.Data;
using System.Threading.RateLimiting;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Infrastructure.Data;
using KPG.Timesheet.Infrastructure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NSubstitute;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.Endpoints;

public class KpgWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string AdminEmail    = "admin@test.com";
    public const string EmpleadoEmail = "empleado@test.com";
    public const string TestPassword  = "Test1234!";

    // La conexión SQLite se mantiene abierta durante toda la vida de la factory
    // para que los datos persistan entre los scopes de cada request
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public KpgWebApplicationFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            // Eliminar toda la configuración EF de SQL Server
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();
            services.RemoveAll<IApplicationDbContext>();

            var optionConfigs = services
                .Where(s => s.ServiceType == typeof(Microsoft.EntityFrameworkCore.Infrastructure.IDbContextOptionsConfiguration<ApplicationDbContext>))
                .ToList();
            foreach (var d in optionConfigs) services.Remove(d);

            // SQLite in-memory: soporta ExecuteDeleteAsync / ExecuteUpdateAsync
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(_connection));

            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

            // Stub IDbConnection (Dapper no se usa en los endpoints testeados)
            services.RemoveAll<IDbConnection>();
            services.AddScoped<IDbConnection>(_ => Substitute.For<IDbConnection>());

            // Deshabilitar rate limiting en tests: eliminar toda la configuración previa
            // y registrar una nueva sin límites para la política "login"
            services.RemoveAll<Microsoft.Extensions.Options.IConfigureOptions<
                Microsoft.AspNetCore.RateLimiting.RateLimiterOptions>>();
            services.Configure<Microsoft.AspNetCore.RateLimiting.RateLimiterOptions>(options =>
                options.AddPolicy("login", _ =>
                    RateLimitPartition.GetNoLimiter<string>(string.Empty)));

            // Reemplazar ApplicationDbContextInitialiser con versión que solo hace EnsureCreated
            services.RemoveAll<ApplicationDbContextInitialiser>();
            services.AddScoped<ApplicationDbContextInitialiser>(sp =>
            {
                var logger      = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ApplicationDbContextInitialiser>>();
                var ctx         = sp.GetRequiredService<ApplicationDbContext>();
                var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
                return new ApplicationDbContextInitialiser(logger, ctx, userManager, roleManager);
            });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        SeedDatabase(host.Services).GetAwaiter().GetResult();
        return host;
    }

    private static async Task SeedDatabase(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp          = scope.ServiceProvider;
        var db          = sp.GetRequiredService<ApplicationDbContext>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();

        await db.Database.EnsureCreatedAsync();

        foreach (var role in new[] { Roles.Admin, Roles.Gerente, Roles.Supervisor, Roles.Empleado })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        await CreateUserAsync(userManager, AdminEmail,    TestPassword, Roles.Admin);
        await CreateUserAsync(userManager, EmpleadoEmail, TestPassword, Roles.Empleado);
    }

    private static async Task CreateUserAsync(UserManager<ApplicationUser> mgr, string email, string password, string role)
    {
        if (await mgr.FindByEmailAsync(email) is not null) return;

        var user = new ApplicationUser { UserName = email, Email = email, IsActive = true, Created = DateTimeOffset.UtcNow };
        var result = await mgr.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Seed failed for {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        await mgr.AddToRoleAsync(user, role);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _connection.Dispose();
    }
}
