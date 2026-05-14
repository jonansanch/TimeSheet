using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KPG.Timesheet.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            await _context.Database.EnsureCreatedAsync();
            await EnsureTimesheetTablesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Crear los 4 roles KPG
        string[] kpgRoles = [Roles.Admin, Roles.Gerente, Roles.Supervisor, Roles.Empleado];
        foreach (var roleName in kpgRoles)
        {
            if (_roleManager.Roles.All(r => r.Name != roleName))
                await _roleManager.CreateAsync(new IdentityRole(roleName));
        }

        // Usuario administrador
        var admin = new ApplicationUser { UserName = "admin@kpg.com", Email = "admin@kpg.com" };
        if (_userManager.Users.All(u => u.UserName != admin.UserName))
        {
            await _userManager.CreateAsync(admin, "Admin1234!");
            await _userManager.AddToRoleAsync(admin, Roles.Admin);
        }

        // Usuario empleado de prueba
        var empleado = new ApplicationUser { UserName = "empleado@kpg.com", Email = "empleado@kpg.com" };
        if (_userManager.Users.All(u => u.UserName != empleado.UserName))
        {
            await _userManager.CreateAsync(empleado, "Empleado1234!");
            await _userManager.AddToRoleAsync(empleado, Roles.Empleado);
        }

        // Usuario supervisor de prueba
        var supervisor = new ApplicationUser { UserName = "supervisor@kpg.com", Email = "supervisor@kpg.com" };
        if (_userManager.Users.All(u => u.UserName != supervisor.UserName))
        {
            await _userManager.CreateAsync(supervisor, "Supervisor1234!");
            await _userManager.AddToRoleAsync(supervisor, Roles.Supervisor);
        }
    }

    private async Task EnsureTimesheetTablesAsync()
    {
        await _context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[dbo].[RefreshTokens]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[RefreshTokens] (
                    [Id] uniqueidentifier NOT NULL,
                    [UserId] nvarchar(450) NOT NULL,
                    [TokenHash] nvarchar(128) NOT NULL,
                    [ExpiresAt] datetime2 NOT NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    [RevokedAt] datetime2 NULL,
                    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id])
                );

                CREATE UNIQUE INDEX [IX_RefreshTokens_TokenHash] ON [dbo].[RefreshTokens] ([TokenHash]);
                CREATE INDEX [IX_RefreshTokens_UserId] ON [dbo].[RefreshTokens] ([UserId]);
            END
            """);

        await _context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[dbo].[RegistrosHoras]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[RegistrosHoras] (
                    [Id] int NOT NULL IDENTITY,
                    [UserId] nvarchar(450) NOT NULL,
                    [FechaRegistro] date NOT NULL,
                    [Turno] nvarchar(2) NOT NULL,
                    [HoraEntrada] time NOT NULL,
                    [HoraSalida] time NOT NULL,
                    [Cliente] nvarchar(200) NOT NULL,
                    [Proyecto] nvarchar(200) NOT NULL,
                    [Modalidad] nvarchar(100) NOT NULL,
                    [Recurso] nvarchar(100) NOT NULL,
                    [Descripcion] nvarchar(1000) NOT NULL,
                    [Lugar] nvarchar(200) NOT NULL,
                    [Created] datetimeoffset NOT NULL,
                    [CreatedBy] nvarchar(max) NULL,
                    [LastModified] datetimeoffset NOT NULL,
                    [LastModifiedBy] nvarchar(max) NULL,
                    CONSTRAINT [PK_RegistrosHoras] PRIMARY KEY ([Id])
                );

                CREATE UNIQUE INDEX [IX_RegistrosHoras_UserId_FechaRegistro_Turno] ON [dbo].[RegistrosHoras] ([UserId], [FechaRegistro], [Turno]);
                CREATE INDEX [IX_RegistrosHoras_FechaRegistro] ON [dbo].[RegistrosHoras] ([FechaRegistro]);
            END
            """);
    }
}
