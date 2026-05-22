using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Domain.Enums;
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

    public ApplicationDbContextInitialiser(
        ILogger<ApplicationDbContextInitialiser> logger,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
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
        try { await TrySeedAsync(); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // ── Roles ────────────────────────────────────────────────────────────
        string[] kpgRoles = [Roles.Admin, Roles.Gerente, Roles.Supervisor, Roles.Empleado];
        foreach (var roleName in kpgRoles)
        {
            if (_roleManager.Roles.All(r => r.Name != roleName))
                await _roleManager.CreateAsync(new IdentityRole(roleName));
        }

        // ── Usuarios ─────────────────────────────────────────────────────────
        var admin = await EnsureUserAsync("admin@kpg.com", "Admin1234!", "Administrador KPG", Roles.Admin);
        var gerente = await EnsureUserAsync("gerente@kpg.com", "Gerente1234!", "Laura Martínez", Roles.Gerente);
        var supervisor = await EnsureUserAsync("supervisor@kpg.com", "Supervisor1234!", "Miguel Torres", Roles.Supervisor);
        var emp1 = await EnsureUserAsync("empleado@kpg.com", "Empleado1234!", "Juan Pérez", Roles.Empleado);
        var emp2 = await EnsureUserAsync("ana.garcia@kpg.com", "Empleado1234!", "Ana García", Roles.Empleado);
        var emp3 = await EnsureUserAsync("carlos.ruiz@kpg.com", "Empleado1234!", "Carlos Ruiz", Roles.Empleado);

        // ── Parámetros del sistema ────────────────────────────────────────────
        await EnsureParametroAsync(Domain.Constants.ParametrosSistema.VentanaRetroactividad, "3");
        await EnsureParametroAsync(Domain.Constants.ParametrosSistema.DiasUmbralNotificacion, "3");

        // ── Catálogos ────────────────────────────────────────────────────────
        await SeedCatalogosAsync();

        // ── Registros de horas (histórico 2 meses) ───────────────────────────
        if (!_context.RegistrosHoras.Any())
            await SeedRegistrosAsync(admin.Id, gerente.Id, supervisor.Id, emp1.Id, emp2.Id, emp3.Id);

        // ── Solicitudes de excepción ──────────────────────────────────────────
        if (!_context.SolicitudesExcepcion.Any())
            await SeedSolicitudesAsync(emp1.Id, emp2.Id, emp3.Id);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<ApplicationUser> EnsureUserAsync(
        string email, string password, string nombreCompleto, string role)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                NombreCompleto = nombreCompleto,
                IsActive = true,
                Created = DateTimeOffset.UtcNow
            };
            await _userManager.CreateAsync(user, password);
            await _userManager.AddToRoleAsync(user, role);
        }
        else if (string.IsNullOrWhiteSpace(user.NombreCompleto))
        {
            user.NombreCompleto = nombreCompleto;
            await _userManager.UpdateAsync(user);
        }
        return user;
    }

    private async Task EnsureParametroAsync(string clave, string valor)
    {
        if (!_context.ParametrosSistema.Any(p => p.Clave == clave))
        {
            _context.ParametrosSistema.Add(new ParametroSistema { Clave = clave, Valor = valor });
            await _context.SaveChangesAsync(CancellationToken.None);
        }
    }

    private async Task SeedCatalogosAsync()
    {
        if (!_context.Clientes.Any())
        {
            var seedData = new[]
            {
                ("Banco Nacional",      new[] { "Core Bancario", "Banca Digital" }),
                ("Ministerio de Salud", new[] { "Sistema RIPS", "Portal Ciudadano" }),
                ("Retail SA",           new[] { "E-Commerce", "POS Cloud" }),
                ("Petrocol",            new[] { "SAP FI", "SAP CO" }),
                ("Constructora XYZ",    new[] { "Portal Clientes", "ERP Obra" }),
                ("KPG Interno",         new[] { "Timesheet", "Gestión de Equipo", "Administración" }),
            };

            foreach (var (nombreCliente, proyectos) in seedData)
            {
                var cliente = new Cliente(nombreCliente);
                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync(CancellationToken.None);
                foreach (var p in proyectos)
                    _context.Proyectos.Add(new Proyecto(cliente.Id, p));
            }
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        if (!_context.Empleados.Any())
        {
            foreach (var nombre in new[] { "Consultor", "Analista", "Desarrollador", "Lider tecnico", "Consultor SAP" })
                _context.Empleados.Add(new Empleado(nombre));
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        if (!_context.Modalidades.Any())
        {
            foreach (var nombre in new[] { "Presencial", "Remoto", "Hibrido" })
                _context.Modalidades.Add(new Modalidad(nombre));
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        if (!_context.LugaresTrabajo.Any())
        {
            foreach (var nombre in new[] { "Presencial Oficina", "Presencial Viaje", "Presencial Cliente", "Remoto" })
                _context.LugaresTrabajo.Add(new LugarTrabajo(nombre));
            await _context.SaveChangesAsync(CancellationToken.None);
        }
    }

    private async Task SeedRegistrosAsync(
        string adminId, string gerenteId, string supervisorId,
        string emp1Id, string emp2Id, string emp3Id)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var registros = new List<RegistroHoras>();

        // Días hábiles de las últimas 9 semanas (Mon-Fri)
        var diasHabiles = Enumerable.Range(1, 63)
            .Select(i => hoy.AddDays(-i))
            .Where(d => d.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
            .ToList();

        // ── Juan Pérez: registros completos los últimos 2 meses ─────────────
        foreach (var dia in diasHabiles.Take(43))
        {
            registros.Add(Reg(emp1Id, dia, TurnoRegistro.AM,
                new(8, 0), new(12, 0),
                "Banco Nacional", "Core Bancario", "Presencial", "Desarrollador",
                "Análisis y desarrollo de módulo de pagos.", "Presencial Oficina"));
            registros.Add(Reg(emp1Id, dia, TurnoRegistro.PM,
                new(13, 0), new(17, 0),
                "Retail SA", "E-Commerce", "Remoto", "Consultor",
                "Implementación de API de catálogo de productos.", "Remoto",
                esRetroactivo: dia < hoy));
        }
        // Esta semana
        registros.Add(Reg(emp1Id, hoy, TurnoRegistro.AM,
            new(8, 0), new(12, 0),
            "Banco Nacional", "Banca Digital", "Presencial", "Desarrollador",
            "Diseño de flujo de autenticación biométrica.", "Presencial Oficina"));

        // ── Ana García: registros con algunos días sin PM ────────────────────
        int anaIdx = 0;
        foreach (var dia in diasHabiles.Take(43))
        {
            registros.Add(Reg(emp2Id, dia, TurnoRegistro.AM,
                new(8, 30), new(12, 30),
                "Ministerio de Salud", "Sistema RIPS", "Presencial", "Analista",
                "Validación de reglas de facturación electrónica.", "Presencial Cliente",
                esRetroactivo: dia < hoy));

            // PM solo 3 de cada 4 semanas
            if (anaIdx % 4 != 2)
                registros.Add(Reg(emp2Id, dia, TurnoRegistro.PM,
                    new(14, 0), new(18, 0),
                    "Constructora XYZ", "Portal Clientes", "Remoto", "Analista",
                    "Levantamiento de requerimientos módulo de cotizaciones.", "Remoto",
                    esRetroactivo: dia < hoy));
            anaIdx++;
        }
        registros.Add(Reg(emp2Id, hoy, TurnoRegistro.AM,
            new(8, 30), new(12, 30),
            "Ministerio de Salud", "Portal Ciudadano", "Presencial", "Analista",
            "Capacitación usuarios clave módulo de citas.", "Presencial Cliente"));

        // ── Carlos Ruiz: último registro hace 2 semanas (para probar notificación) ──
        foreach (var dia in diasHabiles.Where(d => d <= hoy.AddDays(-14)).Take(30))
        {
            registros.Add(Reg(emp3Id, dia, TurnoRegistro.AM,
                new(7, 0), new(11, 0),
                "Petrocol", "SAP FI", "Presencial", "Consultor SAP",
                "Configuración de centros de costo para proyecto offshore.", "Presencial Cliente",
                esRetroactivo: true));
            registros.Add(Reg(emp3Id, dia, TurnoRegistro.PM,
                new(13, 0), new(17, 0),
                "Petrocol", "SAP CO", "Presencial", "Consultor SAP",
                "Ajuste de variantes de selección en reportes CO.", "Presencial Cliente",
                esRetroactivo: true));
        }

        // ── Supervisor: esta semana y la anterior ────────────────────────────
        foreach (var dia in diasHabiles.Take(10))
        {
            registros.Add(Reg(supervisorId, dia, TurnoRegistro.AM,
                new(8, 0), new(12, 0),
                "KPG Interno", "Gestión de Equipo", "Remoto", "Lider tecnico",
                "Revisión de avances y seguimiento del equipo.", "Remoto",
                esRetroactivo: dia < hoy));
        }
        registros.Add(Reg(supervisorId, hoy, TurnoRegistro.AM,
            new(8, 0), new(12, 0),
            "KPG Interno", "Gestión de Equipo", "Remoto", "Lider tecnico",
            "Reunión de planificación semanal.", "Remoto"));

        // ── Admin: esta semana ────────────────────────────────────────────────
        foreach (var dia in diasHabiles.Take(5))
        {
            registros.Add(Reg(adminId, dia, TurnoRegistro.AM,
                new(9, 0), new(13, 0),
                "KPG Interno", "Administración", "Remoto", "Consultor",
                "Configuración y administración del sistema.", "Remoto",
                esRetroactivo: dia < hoy));
        }

        _context.RegistrosHoras.AddRange(registros);
        await _context.SaveChangesAsync(CancellationToken.None);
    }

    private async Task SeedSolicitudesAsync(string emp1Id, string emp2Id, string emp3Id)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);

        // Aprobada — emp1 puede registrar en esa fecha retroactiva
        var s1 = new SolicitudExcepcion(emp1Id, hoy.AddDays(-20), "Incapacidad médica certificada.");
        s1.Aprobar();
        _context.SolicitudesExcepcion.Add(s1);

        // Pendiente — esperando revisión del admin
        var s2 = new SolicitudExcepcion(emp2Id, hoy.AddDays(-10), "Viaje de negocios imprevisto al cliente.");
        _context.SolicitudesExcepcion.Add(s2);

        // Pendiente — segunda solicitud
        var s3 = new SolicitudExcepcion(emp1Id, hoy.AddDays(-15), "Falla de conectividad en zona remota.");
        _context.SolicitudesExcepcion.Add(s3);

        // Rechazada
        var s4 = new SolicitudExcepcion(emp3Id, hoy.AddDays(-30), "Olvido de registro.");
        s4.Rechazar();
        _context.SolicitudesExcepcion.Add(s4);

        await _context.SaveChangesAsync(CancellationToken.None);
    }

    private static RegistroHoras Reg(
        string userId, DateOnly fecha, TurnoRegistro turno,
        TimeOnly entrada, TimeOnly salida,
        string cliente, string proyecto, string modalidad, string recurso,
        string descripcion, string lugar,
        bool esRetroactivo = false) =>
        new(userId, fecha, turno, entrada, salida,
            cliente, proyecto, modalidad, recurso, descripcion, lugar, esRetroactivo);

    // ── EnsureTimesheetTablesAsync (DDL idempotente) ──────────────────────────

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

        await _context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[dbo].[NotificacionesEnviadas]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[NotificacionesEnviadas] (
                    [Id] int NOT NULL IDENTITY,
                    [UserId] nvarchar(450) NOT NULL,
                    [Email] nvarchar(256) NOT NULL,
                    [FechaReferencia] date NOT NULL,
                    [DiasAcumulados] int NOT NULL,
                    [Exitoso] bit NOT NULL,
                    [ErrorDetalle] nvarchar(2000) NULL,
                    [Created] datetimeoffset NOT NULL,
                    [CreatedBy] nvarchar(max) NULL,
                    [LastModified] datetimeoffset NOT NULL,
                    [LastModifiedBy] nvarchar(max) NULL,
                    CONSTRAINT [PK_NotificacionesEnviadas] PRIMARY KEY ([Id])
                );
                CREATE INDEX [IX_NotificacionesEnviadas_UserId_Created]
                    ON [dbo].[NotificacionesEnviadas] ([UserId], [Created]);
            END
            """);

        await _context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[dbo].[BitacoraAuditoria]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[BitacoraAuditoria] (
                    [Id] int NOT NULL IDENTITY,
                    [TipoEvento] nvarchar(100) NOT NULL,
                    [ActorId] nvarchar(450) NOT NULL,
                    [ActorEmail] nvarchar(256) NULL,
                    [EntidadAfectada] nvarchar(100) NOT NULL,
                    [EntidadId] nvarchar(450) NULL,
                    [Timestamp] datetimeoffset NOT NULL,
                    [MetadataJson] nvarchar(4000) NULL,
                    CONSTRAINT [PK_BitacoraAuditoria] PRIMARY KEY ([Id])
                );
                CREATE INDEX [IX_BitacoraAuditoria_ActorId]   ON [dbo].[BitacoraAuditoria] ([ActorId]);
                CREATE INDEX [IX_BitacoraAuditoria_Timestamp]  ON [dbo].[BitacoraAuditoria] ([Timestamp]);
                CREATE INDEX [IX_BitacoraAuditoria_TipoEvento] ON [dbo].[BitacoraAuditoria] ([TipoEvento]);
            END
            """);
    }
}
