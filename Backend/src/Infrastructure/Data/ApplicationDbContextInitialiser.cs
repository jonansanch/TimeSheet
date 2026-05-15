using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Domain.Entities;
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

        // Usuario supervisor de prueba
        var supervisor = new ApplicationUser { UserName = "supervisor@kpg.com", Email = "supervisor@kpg.com" };
        if (_userManager.Users.All(u => u.UserName != supervisor.UserName))
        {
            await _userManager.CreateAsync(supervisor, "Supervisor1234!");
            await _userManager.AddToRoleAsync(supervisor, Roles.Supervisor);
        }

        // Empleados de prueba
        var empleado1 = new ApplicationUser { UserName = "empleado@kpg.com", Email = "empleado@kpg.com" };
        if (_userManager.Users.All(u => u.UserName != empleado1.UserName))
        {
            await _userManager.CreateAsync(empleado1, "Empleado1234!");
            await _userManager.AddToRoleAsync(empleado1, Roles.Empleado);
        }

        var empleado2 = new ApplicationUser { UserName = "ana.garcia@kpg.com", Email = "ana.garcia@kpg.com" };
        if (_userManager.Users.All(u => u.UserName != empleado2.UserName))
        {
            await _userManager.CreateAsync(empleado2, "Empleado1234!");
            await _userManager.AddToRoleAsync(empleado2, Roles.Empleado);
        }

        var empleado3 = new ApplicationUser { UserName = "carlos.ruiz@kpg.com", Email = "carlos.ruiz@kpg.com" };
        if (_userManager.Users.All(u => u.UserName != empleado3.UserName))
        {
            await _userManager.CreateAsync(empleado3, "Empleado1234!");
            await _userManager.AddToRoleAsync(empleado3, Roles.Empleado);
        }

        // Parámetro de ventana de retroactividad (3 días hábiles por defecto)
        if (!_context.ParametrosSistema.Any(p => p.Clave == Domain.Constants.ParametrosSistema.VentanaRetroactividad))
        {
            _context.ParametrosSistema.Add(new Domain.Entities.ParametroSistema
            {
                Clave = Domain.Constants.ParametrosSistema.VentanaRetroactividad,
                Valor = "3"
            });
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        // Clientes y proyectos del catálogo (valores que estaban hardcodeados en el formulario)
        if (!_context.Clientes.Any())
        {
            var seedData = new[]
            {
                ("KPG", "Timesheet"),
                ("Cliente interno", "Soporte operativo"),
                ("Cliente externo", "Consultoria")
            };

            foreach (var (nombreCliente, nombreProyecto) in seedData)
            {
                var cliente = new Cliente(nombreCliente);
                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync(CancellationToken.None);
                _context.Proyectos.Add(new Proyecto(cliente.Id, nombreProyecto));
            }
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        // Empleados del catálogo (recursos hardcodeados en el formulario de registro)
        if (!_context.Empleados.Any())
        {
            string[] recursosIniciales = ["Consultor", "Analista", "Lider tecnico"];
            foreach (var nombre in recursosIniciales)
                _context.Empleados.Add(new Empleado(nombre));
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        // Modalidades del catálogo
        if (!_context.Modalidades.Any())
        {
            string[] modalidadesIniciales = ["Presencial", "Remoto", "Hibrido"];
            foreach (var nombre in modalidadesIniciales)
                _context.Modalidades.Add(new Modalidad(nombre));
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        // Lugares de trabajo del catálogo
        if (!_context.LugaresTrabajo.Any())
        {
            string[] lugaresIniciales = ["Presencial Oficina", "Presencial Viaje", "Presencial Cliente", "Remoto"];
            foreach (var nombre in lugaresIniciales)
                _context.LugaresTrabajo.Add(new LugarTrabajo(nombre));
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        // Parámetro de umbral de notificaciones (3 días por defecto)
        if (!_context.ParametrosSistema.Any(p => p.Clave == Domain.Constants.ParametrosSistema.DiasUmbralNotificacion))
        {
            _context.ParametrosSistema.Add(new Domain.Entities.ParametroSistema
            {
                Clave = Domain.Constants.ParametrosSistema.DiasUmbralNotificacion,
                Valor = "3"
            });
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        // Registros de horas de prueba (solo si la tabla está vacía)
        if (!_context.RegistrosHoras.Any())
        {
            // Recargar usuarios para obtener sus IDs generados
            var user1 = await _userManager.FindByEmailAsync("empleado@kpg.com");
            var user2 = await _userManager.FindByEmailAsync("ana.garcia@kpg.com");
            var user3 = await _userManager.FindByEmailAsync("carlos.ruiz@kpg.com");
            var userSup = await _userManager.FindByEmailAsync("supervisor@kpg.com");
            var userAdmin = await _userManager.FindByEmailAsync("admin@kpg.com");

            var hoy = DateOnly.FromDateTime(DateTime.Today);

            var registros = new List<Domain.Entities.RegistroHoras>
            {
                // Empleado 1 — esta semana
                new(user1!.Id, hoy, Domain.Enums.TurnoRegistro.AM,
                    new TimeOnly(8, 0), new TimeOnly(12, 0),
                    "Banco Nacional", "Core Bancario", "Presencial", "Desarrollador",
                    "Análisis de requerimientos de módulo de pagos.", "Bogotá"),
                new(user1!.Id, hoy, Domain.Enums.TurnoRegistro.PM,
                    new TimeOnly(13, 0), new TimeOnly(17, 0),
                    "Banco Nacional", "Core Bancario", "Presencial", "Desarrollador",
                    "Implementación de API de transacciones.", "Bogotá"),
                new(user1!.Id, hoy.AddDays(-1), Domain.Enums.TurnoRegistro.AM,
                    new TimeOnly(8, 0), new TimeOnly(12, 0),
                    "Retail SA", "E-Commerce", "Remoto", "Consultor",
                    "Revisión de arquitectura del carrito de compras.", "Remoto"),

                // Empleado 2 (Ana García)
                new(user2!.Id, hoy, Domain.Enums.TurnoRegistro.AM,
                    new TimeOnly(8, 30), new TimeOnly(12, 30),
                    "Ministerio de Salud", "Sistema RIPS", "Presencial", "Analista",
                    "Capacitación en módulo de facturación electrónica.", "Bogotá"),
                new(user2!.Id, hoy, Domain.Enums.TurnoRegistro.PM,
                    new TimeOnly(14, 0), new TimeOnly(18, 0),
                    "Ministerio de Salud", "Sistema RIPS", "Presencial", "Analista",
                    "Pruebas de integración con ADRES.", "Bogotá"),
                new(user2!.Id, hoy.AddDays(-2), Domain.Enums.TurnoRegistro.AM,
                    new TimeOnly(9, 0), new TimeOnly(13, 0),
                    "Constructora XYZ", "Portal Clientes", "Remoto", "Desarrollador",
                    "Desarrollo de módulo de cotizaciones en línea.", "Medellín"),

                // Empleado 3 (Carlos Ruiz)
                new(user3!.Id, hoy, Domain.Enums.TurnoRegistro.AM,
                    new TimeOnly(7, 0), new TimeOnly(11, 0),
                    "Petrocol", "SAP FI", "Presencial", "Consultor SAP",
                    "Configuración de centro de costo para proyecto offshore.", "Cartagena"),
                new(user3!.Id, hoy.AddDays(-1), Domain.Enums.TurnoRegistro.PM,
                    new TimeOnly(13, 0), new TimeOnly(17, 0),
                    "Petrocol", "SAP FI", "Presencial", "Consultor SAP",
                    "Ajuste de variantes de selección en reportes financieros.", "Cartagena"),

                // Supervisor — también tiene registros propios
                new(userSup!.Id, hoy, Domain.Enums.TurnoRegistro.AM,
                    new TimeOnly(8, 0), new TimeOnly(12, 0),
                    "KPG Interno", "Gestión de Equipo", "Remoto", "Supervisor",
                    "Revisión de avances semanales del equipo.", "Bogotá"),

                // Admin — tiene registros propios
                new(userAdmin!.Id, hoy.AddDays(-1), Domain.Enums.TurnoRegistro.AM,
                    new TimeOnly(9, 0), new TimeOnly(13, 0),
                    "KPG Interno", "Administración", "Remoto", "Admin",
                    "Configuración de parámetros del sistema.", "Bogotá"),
            };

            _context.RegistrosHoras.AddRange(registros);
            await _context.SaveChangesAsync(CancellationToken.None);
        }
    }

    private async Task EnsureTimesheetTablesAsync()
    {
        await _context.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'[dbo].[AspNetUsers]', N'IsActive') IS NULL
            BEGIN
                ALTER TABLE [dbo].[AspNetUsers] ADD [IsActive] bit NOT NULL CONSTRAINT [DF_AspNetUsers_IsActive] DEFAULT(1);
            END

            IF COL_LENGTH(N'[dbo].[AspNetUsers]', N'Created') IS NULL
            BEGIN
                ALTER TABLE [dbo].[AspNetUsers] ADD [Created] datetimeoffset NOT NULL CONSTRAINT [DF_AspNetUsers_Created] DEFAULT(SYSDATETIMEOFFSET());
            END

            IF COL_LENGTH(N'[dbo].[AspNetUsers]', N'DeactivatedAt') IS NULL
            BEGIN
                ALTER TABLE [dbo].[AspNetUsers] ADD [DeactivatedAt] datetimeoffset NULL;
            END

            IF COL_LENGTH(N'[dbo].[AspNetUsers]', N'DeactivatedBy') IS NULL
            BEGIN
                ALTER TABLE [dbo].[AspNetUsers] ADD [DeactivatedBy] nvarchar(450) NULL;
            END
            """);

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
                    [EsRetroactivo] bit NOT NULL DEFAULT 0,
                    [Created] datetimeoffset NOT NULL,
                    [CreatedBy] nvarchar(max) NULL,
                    [LastModified] datetimeoffset NOT NULL,
                    [LastModifiedBy] nvarchar(max) NULL,
                    CONSTRAINT [PK_RegistrosHoras] PRIMARY KEY ([Id])
                );

                CREATE INDEX [IX_RegistrosHoras_FechaRegistro] ON [dbo].[RegistrosHoras] ([FechaRegistro]);
            END
            ELSE
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'[dbo].[RegistrosHoras]')
                      AND name = N'EsRetroactivo'
                )
                BEGIN
                    ALTER TABLE [dbo].[RegistrosHoras]
                        ADD [EsRetroactivo] bit NOT NULL DEFAULT 0;
                END

                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE object_id = OBJECT_ID(N'[dbo].[RegistrosHoras]')
                      AND name = N'IX_RegistrosHoras_UserId_FechaRegistro_Turno'
                )
                BEGIN
                    DROP INDEX [IX_RegistrosHoras_UserId_FechaRegistro_Turno] ON [dbo].[RegistrosHoras];
                END
            END
            """);

        await _context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[dbo].[ParametrosSistema]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[ParametrosSistema] (
                    [Id] int NOT NULL IDENTITY,
                    [Clave] nvarchar(100) NOT NULL,
                    [Valor] nvarchar(500) NOT NULL,
                    [Created] datetimeoffset NOT NULL,
                    [CreatedBy] nvarchar(max) NULL,
                    [LastModified] datetimeoffset NOT NULL,
                    [LastModifiedBy] nvarchar(max) NULL,
                    CONSTRAINT [PK_ParametrosSistema] PRIMARY KEY ([Id])
                );
                CREATE UNIQUE INDEX [IX_ParametrosSistema_Clave] ON [dbo].[ParametrosSistema] ([Clave]);
            END
            """);

        await _context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[dbo].[Empleados]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[Empleados] (
                    [Id] int NOT NULL IDENTITY,
                    [Nombre] nvarchar(200) NOT NULL,
                    [Activo] bit NOT NULL DEFAULT 1,
                    [Created] datetimeoffset NOT NULL,
                    [CreatedBy] nvarchar(max) NULL,
                    [LastModified] datetimeoffset NOT NULL,
                    [LastModifiedBy] nvarchar(max) NULL,
                    CONSTRAINT [PK_Empleados] PRIMARY KEY ([Id])
                );
                CREATE UNIQUE INDEX [IX_Empleados_Nombre] ON [dbo].[Empleados] ([Nombre]);
            END
            """);

        await _context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[dbo].[Clientes]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[Clientes] (
                    [Id] int NOT NULL IDENTITY,
                    [Nombre] nvarchar(200) NOT NULL,
                    [Activo] bit NOT NULL DEFAULT 1,
                    [Created] datetimeoffset NOT NULL,
                    [CreatedBy] nvarchar(max) NULL,
                    [LastModified] datetimeoffset NOT NULL,
                    [LastModifiedBy] nvarchar(max) NULL,
                    CONSTRAINT [PK_Clientes] PRIMARY KEY ([Id])
                );
                CREATE UNIQUE INDEX [IX_Clientes_Nombre] ON [dbo].[Clientes] ([Nombre]);
            END
            """);

        await _context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[dbo].[Proyectos]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[Proyectos] (
                    [Id] int NOT NULL IDENTITY,
                    [Nombre] nvarchar(200) NOT NULL,
                    [ClienteId] int NOT NULL,
                    [Activo] bit NOT NULL DEFAULT 1,
                    [Created] datetimeoffset NOT NULL,
                    [CreatedBy] nvarchar(max) NULL,
                    [LastModified] datetimeoffset NOT NULL,
                    [LastModifiedBy] nvarchar(max) NULL,
                    CONSTRAINT [PK_Proyectos] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_Proyectos_Clientes] FOREIGN KEY ([ClienteId]) REFERENCES [dbo].[Clientes]([Id])
                );
                CREATE UNIQUE INDEX [IX_Proyectos_ClienteId_Nombre] ON [dbo].[Proyectos] ([ClienteId], [Nombre]);
            END
            """);

        await _context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[dbo].[Modalidades]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[Modalidades] (
                    [Id] int NOT NULL IDENTITY,
                    [Nombre] nvarchar(100) NOT NULL,
                    [Activo] bit NOT NULL DEFAULT 1,
                    [Created] datetimeoffset NOT NULL,
                    [CreatedBy] nvarchar(max) NULL,
                    [LastModified] datetimeoffset NOT NULL,
                    [LastModifiedBy] nvarchar(max) NULL,
                    CONSTRAINT [PK_Modalidades] PRIMARY KEY ([Id])
                );
                CREATE UNIQUE INDEX [IX_Modalidades_Nombre] ON [dbo].[Modalidades] ([Nombre]);
            END
            """);

        await _context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[dbo].[LugaresTrabajo]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[LugaresTrabajo] (
                    [Id] int NOT NULL IDENTITY,
                    [Nombre] nvarchar(200) NOT NULL,
                    [Activo] bit NOT NULL DEFAULT 1,
                    [Created] datetimeoffset NOT NULL,
                    [CreatedBy] nvarchar(max) NULL,
                    [LastModified] datetimeoffset NOT NULL,
                    [LastModifiedBy] nvarchar(max) NULL,
                    CONSTRAINT [PK_LugaresTrabajo] PRIMARY KEY ([Id])
                );
                CREATE UNIQUE INDEX [IX_LugaresTrabajo_Nombre] ON [dbo].[LugaresTrabajo] ([Nombre]);
            END
            """);

        await _context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[dbo].[SolicitudesExcepcion]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[SolicitudesExcepcion] (
                    [Id] int NOT NULL IDENTITY,
                    [UserId] nvarchar(450) NOT NULL,
                    [FechaRegistro] date NOT NULL,
                    [Justificacion] nvarchar(1000) NOT NULL,
                    [Estado] nvarchar(20) NOT NULL,
                    [Created] datetimeoffset NOT NULL,
                    [CreatedBy] nvarchar(max) NULL,
                    [LastModified] datetimeoffset NOT NULL,
                    [LastModifiedBy] nvarchar(max) NULL,
                    CONSTRAINT [PK_SolicitudesExcepcion] PRIMARY KEY ([Id])
                );
                CREATE INDEX [IX_SolicitudesExcepcion_UserId_FechaRegistro]
                    ON [dbo].[SolicitudesExcepcion] ([UserId], [FechaRegistro]);
            END
            """);
    }
}
