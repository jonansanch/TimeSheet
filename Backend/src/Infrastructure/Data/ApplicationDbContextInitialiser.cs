using System.Text;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
    private readonly IHostEnvironment _environment;

    public ApplicationDbContextInitialiser(
        ILogger<ApplicationDbContextInitialiser> logger,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IHostEnvironment environment)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _environment = environment;
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
        var admin      = await EnsureUserAsync("admin@kpg.com",          "Admin1234!",      "Administrador KPG",  Roles.Admin);
        var gerente    = await EnsureUserAsync("gerente@kpg.com",      "Gerente1234!",    "Laura Martínez",     Roles.Gerente);
        var supervisor = await EnsureUserAsync("supervisor@kpg.com",   "Supervisor1234!", "Miguel Torres",      Roles.Supervisor);
        var emp1       = await EnsureUserAsync("empleado@kpg.com",     "Empleado1234!",   "Juan Pérez",         Roles.Empleado);
        var emp2       = await EnsureUserAsync("ana.garcia@kpg.com",   "Empleado1234!",   "Ana García",         Roles.Empleado);
        var emp3       = await EnsureUserAsync("carlos.ruiz@kpg.com",  "Empleado1234!",   "Carlos Ruiz",        Roles.Empleado);

        // Equipo ampliado para poder simular el flujo con varios puestos y dos ramas del
        // organigrama, en vez de una sola linea de tres personas.
        var sup2       = await EnsureUserAsync("sofia.rojas@kpg.com",  "Supervisor1234!", "Sofía Rojas",        Roles.Supervisor);
        var emp4       = await EnsureUserAsync("diego.mora@kpg.com",   "Empleado1234!",   "Diego Mora",         Roles.Empleado);
        var emp5       = await EnsureUserAsync("valeria.leon@kpg.com", "Empleado1234!",   "Valeria León",       Roles.Empleado);
        var emp6       = await EnsureUserAsync("andres.gil@kpg.com",   "Empleado1234!",   "Andrés Gil",         Roles.Empleado);
        var emp7       = await EnsureUserAsync("paula.nino@kpg.com",   "Empleado1234!",   "Paula Niño",         Roles.Empleado);

        // ── Parámetros del sistema ────────────────────────────────────────────
        await EnsureParametroAsync(Domain.Constants.ParametrosSistema.VentanaRetroactividad, "3");
        await EnsureParametroAsync(Domain.Constants.ParametrosSistema.DiasUmbralNotificacion, "3");
        await EnsureParametroAsync(Domain.Constants.ParametrosSistema.HorasDiaCompleto, "8");
        await EnsureParametroAsync(Domain.Constants.ParametrosSistema.PeriodoAprobacion, "Semanal");

        // ── Catálogos ────────────────────────────────────────────────────────
        await SeedCatalogosAsync();

        // ── Estructura organizacional ────────────────────────────────────────
        // Sin esto la cadena de aprobacion resuelve vacia y ningun registro se puede
        // aprobar: el flujo completo no se puede ni demostrar ni probar.
        await SeedEstructuraAsync(
            admin, gerente, supervisor, emp1, emp2, emp3,
            sup2, emp4, emp5, emp6, emp7);

        // Dataset amplio para validar el organigrama y las bandejas de aprobacion.
        // Tiene un guard adicional porque RunDatabaseInitialiser tambien puede habilitarse
        // fuera de Development para tareas operativas y estas cuentas son solo de QA.
        if (_environment.IsDevelopment())
            await SeedOrganigramaQaAsync(gerente);

        // ── Registros de horas (histórico 2 meses) ───────────────────────────
        if (!_context.RegistrosHoras.Any())
        {
            await SeedRegistrosAsync(admin.Id, gerente.Id, supervisor.Id, emp1.Id, emp2.Id, emp3.Id);
            await SeedAprobacionesAsync(supervisor.Id, gerente.Id, admin.Id);
        }

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
                UserName       = email,
                Email          = email,
                NombreCompleto = nombreCompleto,
                IsActive       = true,
                Created        = DateTimeOffset.UtcNow
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
            foreach (var nombre in new[]
            {
                "Consultor", "Analista", "Desarrollador", "Lider tecnico", "Consultor SAP",
                "Arquitecto", "QA", "Disenador UX", "Soporte", "Gerente de Proyecto"
            })
                _context.Empleados.Add(new Empleado(nombre));
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        if (!_context.Modalidades.Any())
        {
            foreach (var nombre in new[] { "Cliente", "Remoto" })
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

    /// <summary>
    /// Deja armada la estructura que necesita la cadena de aprobacion: puesto y jefe de
    /// cada usuario, supervisores por puesto (nivel 1) y responsable de cada proyecto
    /// (nivel 3). Sin esto los tres niveles resuelven vacios y **ningun registro se puede
    /// aprobar**, porque un nivel sin aprobador asignado no lo aprueba nadie.
    ///
    /// <para>
    /// Es idempotente y no pisa lo configurado a mano: solo completa lo que esta vacio.
    /// </para>
    /// </summary>
    private async Task SeedEstructuraAsync(
        ApplicationUser admin, ApplicationUser gerente, ApplicationUser supervisor,
        ApplicationUser emp1, ApplicationUser emp2, ApplicationUser emp3,
        ApplicationUser sup2, ApplicationUser emp4, ApplicationUser emp5,
        ApplicationUser emp6, ApplicationUser emp7)
    {
        // Se indexa por nombre normalizado (sin tildes, minusculas): el catalogo real
        // dice "Lider Tecnico" con tildes y mayusculas, y una comparacion exacta dejaba
        // a esa gente sin puesto — y por lo tanto sin quien les apruebe el nivel 1.
        var puestos = (await _context.Empleados.ToListAsync(CancellationToken.None))
            .GroupBy(e => NormalizarNombre(e.Nombre))
            .ToDictionary(g => g.Key, g => g.First().Id);

        // ── Puesto y jefe directo (nivel 2 = organigrama) ────────────────────
        // Todos tienen jefe. La cabeza es su propio jefe: asi su nivel 2 tambien
        // resuelve y sus registros no quedan trabados para siempre.
        await AsignarEstructuraAsync(gerente,    puestos, "Lider tecnico", jefeId: gerente.Id);
        await AsignarEstructuraAsync(admin,      puestos, "Consultor",     jefeId: gerente.Id);
        await AsignarEstructuraAsync(supervisor, puestos, "Lider tecnico", jefeId: gerente.Id);
        await AsignarEstructuraAsync(emp1,       puestos, "Desarrollador", jefeId: supervisor.Id);
        await AsignarEstructuraAsync(emp2,       puestos, "Analista",      jefeId: supervisor.Id);
        await AsignarEstructuraAsync(emp3,       puestos, "Consultor SAP", jefeId: supervisor.Id);

        // Segunda rama: Sofia cuelga de Laura y tiene su propio equipo. Asi la pantalla
        // de aprobaciones se puede probar con dos revisores distintos a la vez.
        await AsignarEstructuraAsync(sup2,        puestos, "Gerente de Proyecto", jefeId: gerente.Id);
        await AsignarEstructuraAsync(emp4,        puestos, "Arquitecto",          jefeId: sup2.Id);
        await AsignarEstructuraAsync(emp5,        puestos, "QA",                  jefeId: sup2.Id);
        await AsignarEstructuraAsync(emp6,        puestos, "Disenador UX",        jefeId: sup2.Id);
        await AsignarEstructuraAsync(emp7,        puestos, "Soporte",             jefeId: sup2.Id);

        // ── Supervisores por puesto (nivel 1) ────────────────────────────────
        // Se evalua REGLA POR REGLA, no "si la tabla esta vacia": con el guard grueso, un
        // puesto agregado despues se quedaba sin supervisor para siempre y los registros
        // de esa gente no los podia aprobar nadie.
        var existentes = await _context.SupervisoresPuesto
            .Select(sp => new { sp.PuestoId, sp.ClienteId })
            .ToListAsync(CancellationToken.None);

        bool YaHayRegla(int puestoId, int? clienteId) =>
            existentes.Any(e => e.PuestoId == puestoId && e.ClienteId == clienteId);

        var nuevasReglas = new List<SupervisorPuesto>();

        // Reglas generales: valen para todos los clientes.
        foreach (var (puesto, responsable) in new[]
        {
            ("Desarrollador", supervisor.Id),
            ("Analista",      supervisor.Id),
            ("Consultor SAP", supervisor.Id),
            ("Consultor",     supervisor.Id),
            ("Arquitecto",          sup2.Id),
            ("QA",                  sup2.Id),
            ("Disenador UX",        sup2.Id),
            ("Soporte",             sup2.Id),
            ("Gerente de Proyecto", gerente.Id),
            // Al admin y no a la gerente: la gerente ES Lider tecnico, y apuntarle la regla
            // a ella misma la dejaria aprobando su propio nivel 1.
            ("Lider tecnico", admin.Id),
        })
        {
            if (BuscarPuesto(puestos, puesto) is { } puestoId && !YaHayRegla(puestoId, null))
                nuevasReglas.Add(new SupervisorPuesto(puestoId, responsable));
        }

        // Regla especifica de un cliente: gana sobre la general del mismo puesto.
        // Sirve de ejemplo vivo de que la excepcion por cliente funciona.
        var petrocol = await _context.Clientes
            .Where(c => c.Nombre == "Petrocol")
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync(CancellationToken.None);

        if (petrocol is not null
            && BuscarPuesto(puestos, "Consultor SAP") is { } sapId
            && !YaHayRegla(sapId, petrocol))
            nuevasReglas.Add(new SupervisorPuesto(sapId, gerente.Id, petrocol));

        if (nuevasReglas.Count > 0)
        {
            _context.SupervisoresPuesto.AddRange(nuevasReglas);
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        // ── Responsable de cada proyecto (nivel 3) ───────────────────────────
        var proyectosSinSupervisor = await _context.Proyectos
            .Where(p => p.SupervisorUserId == null)
            .Join(_context.Clientes, p => p.ClienteId, c => c.Id, (p, c) => new { Proyecto = p, Cliente = c.Nombre })
            .ToListAsync(CancellationToken.None);

        foreach (var item in proyectosSinSupervisor)
        {
            // Lo interno lo responde el admin; lo de clientes, la gerente.
            item.Proyecto.AsignarSupervisor(item.Cliente == "KPG Interno" ? admin.Id : gerente.Id);
        }

        if (proyectosSinSupervisor.Count > 0)
            await _context.SaveChangesAsync(CancellationToken.None);
    }

    private static int? BuscarPuesto(Dictionary<string, int> puestos, string nombre) =>
        puestos.TryGetValue(NormalizarNombre(nombre), out var id) ? id : null;

    /// <summary>
    /// Completa el dataset de desarrollo hasta 40 usuarios activos: 1 Admin, 3 Gerentes,
    /// 8 Supervisores y 28 Empleados. Las cuentas tienen correos deterministas y la
    /// asignacion es idempotente, por lo que reiniciar la API no crea duplicados.
    /// </summary>
    private async Task SeedOrganigramaQaAsync(ApplicationUser gerenteRaiz)
    {
        var gerente2 = await EnsureUserAsync(
            "camila.vargas@kpg.com", "Gerente1234!", "Camila Vargas", Roles.Gerente);
        var gerente3 = await EnsureUserAsync(
            "ricardo.mendoza@kpg.com", "Gerente1234!", "Ricardo Mendoza", Roles.Gerente);

        var supervisores = new[]
        {
            await EnsureUserAsync("natalia.gomez@kpg.com", "Supervisor1234!", "Natalia Gómez", Roles.Supervisor),
            await EnsureUserAsync("felipe.castro@kpg.com", "Supervisor1234!", "Felipe Castro", Roles.Supervisor),
            await EnsureUserAsync("daniela.ortiz@kpg.com", "Supervisor1234!", "Daniela Ortiz", Roles.Supervisor),
            await EnsureUserAsync("jorge.herrera@kpg.com", "Supervisor1234!", "Jorge Herrera", Roles.Supervisor),
            await EnsureUserAsync("mariana.silva@kpg.com", "Supervisor1234!", "Mariana Silva", Roles.Supervisor),
            await EnsureUserAsync("sebastian.lopez@kpg.com", "Supervisor1234!", "Sebastián López", Roles.Supervisor)
        };

        var empleados = new[]
        {
            await EnsureUserAsync("lucia.ramirez@kpg.com", "Empleado1234!", "Lucía Ramírez", Roles.Empleado),
            await EnsureUserAsync("mateo.sanchez@kpg.com", "Empleado1234!", "Mateo Sánchez", Roles.Empleado),
            await EnsureUserAsync("isabella.torres@kpg.com", "Empleado1234!", "Isabella Torres", Roles.Empleado),
            await EnsureUserAsync("santiago.moreno@kpg.com", "Empleado1234!", "Santiago Moreno", Roles.Empleado),
            await EnsureUserAsync("valentina.castro@kpg.com", "Empleado1234!", "Valentina Castro", Roles.Empleado),
            await EnsureUserAsync("emiliano.rojas@kpg.com", "Empleado1234!", "Emiliano Rojas", Roles.Empleado),
            await EnsureUserAsync("mariana.gutierrez@kpg.com", "Empleado1234!", "Mariana Gutiérrez", Roles.Empleado),
            await EnsureUserAsync("samuel.diaz@kpg.com", "Empleado1234!", "Samuel Díaz", Roles.Empleado),
            await EnsureUserAsync("gabriela.martinez@kpg.com", "Empleado1234!", "Gabriela Martínez", Roles.Empleado),
            await EnsureUserAsync("nicolas.vargas@kpg.com", "Empleado1234!", "Nicolás Vargas", Roles.Empleado),
            await EnsureUserAsync("martina.herrera@kpg.com", "Empleado1234!", "Martina Herrera", Roles.Empleado),
            await EnsureUserAsync("alejandro.ruiz@kpg.com", "Empleado1234!", "Alejandro Ruiz", Roles.Empleado),
            await EnsureUserAsync("paulina.ortiz@kpg.com", "Empleado1234!", "Paulina Ortiz", Roles.Empleado),
            await EnsureUserAsync("tomas.gomez@kpg.com", "Empleado1234!", "Tomás Gómez", Roles.Empleado),
            await EnsureUserAsync("renata.silva@kpg.com", "Empleado1234!", "Renata Silva", Roles.Empleado),
            await EnsureUserAsync("maximiliano.lopez@kpg.com", "Empleado1234!", "Maximiliano López", Roles.Empleado),
            await EnsureUserAsync("julieta.mendoza@kpg.com", "Empleado1234!", "Julieta Mendoza", Roles.Empleado),
            await EnsureUserAsync("thiago.cardenas@kpg.com", "Empleado1234!", "Thiago Cárdenas", Roles.Empleado),
            await EnsureUserAsync("emma.navarro@kpg.com", "Empleado1234!", "Emma Navarro", Roles.Empleado),
            await EnsureUserAsync("benjamin.reyes@kpg.com", "Empleado1234!", "Benjamín Reyes", Roles.Empleado),
            await EnsureUserAsync("antonella.romero@kpg.com", "Empleado1234!", "Antonella Romero", Roles.Empleado)
        };

        var puestos = (await _context.Empleados.ToListAsync(CancellationToken.None))
            .GroupBy(e => NormalizarNombre(e.Nombre))
            .ToDictionary(g => g.Key, g => g.First().Id);

        await AsignarEstructuraAsync(gerente2, puestos, "Gerente de Proyecto", gerenteRaiz.Id);
        await AsignarEstructuraAsync(gerente3, puestos, "Gerente de Proyecto", gerenteRaiz.Id);

        var jefesSupervisores = new[]
        {
            gerente2.Id, gerente2.Id,
            gerente3.Id, gerente3.Id,
            gerenteRaiz.Id, gerenteRaiz.Id
        };

        for (var i = 0; i < supervisores.Length; i++)
            await AsignarEstructuraAsync(
                supervisores[i], puestos, "Lider tecnico", jefesSupervisores[i]);

        var puestosEmpleados = new[]
        {
            "Desarrollador", "Analista", "Consultor SAP", "QA",
            "Arquitecto", "Disenador UX", "Soporte", "Consultor"
        };
        var tamanosEquipo = new[] { 4, 4, 4, 3, 3, 3 };
        var empleadoIndex = 0;

        for (var supervisorIndex = 0; supervisorIndex < supervisores.Length; supervisorIndex++)
        {
            for (var integrante = 0; integrante < tamanosEquipo[supervisorIndex]; integrante++)
            {
                var empleado = empleados[empleadoIndex];
                var puesto = puestosEmpleados[empleadoIndex % puestosEmpleados.Length];
                await AsignarEstructuraAsync(
                    empleado, puestos, puesto, supervisores[supervisorIndex].Id);
                empleadoIndex++;
            }
        }
    }

    /// <summary>
    /// Minusculas y sin tildes, para que "Lider tecnico" encuentre a "Lider Tecnico".
    /// El catalogo lo escribe gente, y el seed no puede depender de como lo tecleo.
    /// </summary>
    private static string NormalizarNombre(string nombre)
    {
        var descompuesto = nombre.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sinTildes = new string(descompuesto
            .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                        != System.Globalization.UnicodeCategory.NonSpacingMark)
            .ToArray());
        return sinTildes.Normalize(NormalizationForm.FormC);
    }

    /// <summary>Completa puesto y jefe solo si estan vacios, para no pisar lo configurado.</summary>
    private async Task AsignarEstructuraAsync(
        ApplicationUser usuario, Dictionary<string, int> puestos, string puesto, string? jefeId)
    {
        var cambio = false;

        if (usuario.PuestoId is null && BuscarPuesto(puestos, puesto) is { } puestoId)
        {
            usuario.PuestoId = puestoId;
            cambio = true;
        }

        if (usuario.SupervisorUserId is null && jefeId is not null)
        {
            usuario.SupervisorUserId = jefeId;
            cambio = true;
        }

        if (cambio) await _userManager.UpdateAsync(usuario);
    }

    /// <summary>
    /// Deja los registros del seed en estados de aprobacion realistas: lo de semanas
    /// anteriores ya aprobado, lo de la ultima semana esperando revision. Asi la pantalla
    /// de aprobaciones tiene algo que mostrar apenas se levanta el ambiente, en vez de
    /// pedir que alguien registre y apruebe a mano para poder verla.
    /// </summary>
    private async Task SeedAprobacionesAsync(string supervisorId, string gerenteId, string adminId)
    {
        var corte = DateOnly.FromDateTime(DateTime.Today).AddDays(-7);

        var registros = await _context.RegistrosHoras
            .Where(r => r.FechaRegistro < corte)
            .ToListAsync(CancellationToken.None);

        foreach (var registro in registros)
        {
            // Los tres niveles de la cadena del seed, en orden.
            foreach (var (nivel, actor) in new[] { (1, supervisorId), (2, supervisorId), (3, gerenteId) })
            {
                if (registro.NivelPendiente != nivel) break;

                registro.Aprobar(nivel);
                _context.AprobacionesRegistro.Add(
                    new AprobacionRegistro(registro.Id, AprobacionRegistro.AccionAprobar, nivel, actor));
            }
        }

        // Un dia rechazado, para que el estado de rechazo tambien se pueda ver y probar.
        var paraRechazar = await _context.RegistrosHoras
            .Where(r => r.FechaRegistro >= corte && r.Estado == Domain.Enums.EstadoAprobacion.Pendiente)
            .OrderBy(r => r.FechaRegistro)
            .FirstOrDefaultAsync(CancellationToken.None);

        if (paraRechazar is not null)
        {
            const string motivo = "La descripcion no detalla en que se fueron las horas de la tarde.";
            paraRechazar.Rechazar(motivo);
            _context.AprobacionesRegistro.Add(new AprobacionRegistro(
                paraRechazar.Id, AprobacionRegistro.AccionRechazar, 1, supervisorId, motivo));
        }

        await _context.SaveChangesAsync(CancellationToken.None);
    }

    private async Task SeedRegistrosAsync(
        string adminId, string gerenteId, string supervisorId,
        string emp1Id, string emp2Id, string emp3Id)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var registros = new List<RegistroHoras>();

        // El registro guarda ProyectoId: el seed resuelve los nombres una sola vez.
        var proyectosPorNombre = await (
            from p in _context.Proyectos
            join c in _context.Clientes on p.ClienteId equals c.Id
            select new { Clave = c.Nombre + "|" + p.Nombre, p.Id })
            .ToDictionaryAsync(x => x.Clave, x => x.Id, CancellationToken.None);

        // Días hábiles de las últimas 9 semanas (Mon-Fri)
        var diasHabiles = Enumerable.Range(1, 63)
            .Select(i => hoy.AddDays(-i))
            .Where(d => d.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
            .ToList();

        // ── Juan Pérez: jornada completa AM+PM ──────────────────────────────
        foreach (var dia in diasHabiles.Take(43))
        {
            registros.Add(Reg(emp1Id, dia,
                new(8, 0), new(12, 0), new(13, 0), new(17, 0),
                ProyectoId(proyectosPorNombre, "Banco Nacional", "Core Bancario"), "Cliente", "Desarrollador",
                "Análisis y desarrollo de módulo de pagos.", "Presencial Oficina",
                esRetroactivo: dia < hoy));
        }
        registros.Add(Reg(emp1Id, hoy,
            new(8, 0), new(12, 0), null, null,
            ProyectoId(proyectosPorNombre, "Banco Nacional", "Banca Digital"), "Cliente", "Desarrollador",
            "Diseño de flujo de autenticación biométrica.", "Presencial Oficina"));

        // ── Ana García: AM siempre, PM en 3 de cada 4 semanas ───────────────
        int anaIdx = 0;
        foreach (var dia in diasHabiles.Take(43))
        {
            var tienePM = anaIdx % 4 != 2;
            registros.Add(Reg(emp2Id, dia,
                new(8, 30), new(12, 30),
                tienePM ? new TimeOnly(14, 0) : null,
                tienePM ? new TimeOnly(18, 0) : null,
                ProyectoId(proyectosPorNombre, "Ministerio de Salud", "Sistema RIPS"), "Cliente", "Analista",
                "Validación de reglas de facturación electrónica.", "Presencial Cliente",
                esRetroactivo: dia < hoy));
            anaIdx++;
        }
        registros.Add(Reg(emp2Id, hoy,
            new(8, 30), new(12, 30), null, null,
            ProyectoId(proyectosPorNombre, "Ministerio de Salud", "Portal Ciudadano"), "Cliente", "Analista",
            "Capacitación usuarios clave módulo de citas.", "Presencial Cliente"));

        // ── Carlos Ruiz: último registro hace 2 semanas (para notificación) ──
        foreach (var dia in diasHabiles.Where(d => d <= hoy.AddDays(-14)).Take(30))
        {
            registros.Add(Reg(emp3Id, dia,
                new(7, 0), new(11, 0), new(13, 0), new(17, 0),
                ProyectoId(proyectosPorNombre, "Petrocol", "SAP FI"), "Cliente", "Consultor SAP",
                "Configuración de centros de costo proyecto offshore.", "Presencial Cliente",
                esRetroactivo: true));
        }

        // ── Supervisor: esta semana y la anterior ────────────────────────────
        foreach (var dia in diasHabiles.Take(10))
        {
            registros.Add(Reg(supervisorId, dia,
                new(8, 0), new(12, 0), null, null,
                ProyectoId(proyectosPorNombre, "KPG Interno", "Gestión de Equipo"), "Remoto", "Lider tecnico",
                "Revisión de avances y seguimiento del equipo.", "Remoto",
                esRetroactivo: dia < hoy));
        }
        registros.Add(Reg(supervisorId, hoy,
            new(8, 0), new(12, 0), null, null,
            ProyectoId(proyectosPorNombre, "KPG Interno", "Gestión de Equipo"), "Remoto", "Lider tecnico",
            "Reunión de planificación semanal.", "Remoto"));

        // ── Admin: esta semana ────────────────────────────────────────────────
        foreach (var dia in diasHabiles.Take(5))
        {
            registros.Add(Reg(adminId, dia,
                new(9, 0), new(13, 0), null, null,
                ProyectoId(proyectosPorNombre, "KPG Interno", "Administración"), "Remoto", "Consultor",
                "Configuración y administración del sistema.", "Remoto",
                esRetroactivo: dia < hoy));
        }

        _context.RegistrosHoras.AddRange(registros);
        await _context.SaveChangesAsync(CancellationToken.None);
    }

    private async Task SeedSolicitudesAsync(string emp1Id, string emp2Id, string emp3Id)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);

        var s1 = new SolicitudExcepcion(emp1Id, hoy.AddDays(-20), "Incapacidad médica certificada.");
        s1.Aprobar();
        _context.SolicitudesExcepcion.Add(s1);

        var s2 = new SolicitudExcepcion(emp2Id, hoy.AddDays(-10), "Viaje de negocios imprevisto al cliente.");
        _context.SolicitudesExcepcion.Add(s2);

        var s3 = new SolicitudExcepcion(emp1Id, hoy.AddDays(-15), "Falla de conectividad en zona remota.");
        _context.SolicitudesExcepcion.Add(s3);

        var s4 = new SolicitudExcepcion(emp3Id, hoy.AddDays(-30), "Olvido de registro.");
        s4.Rechazar();
        _context.SolicitudesExcepcion.Add(s4);

        await _context.SaveChangesAsync(CancellationToken.None);
    }

    /// <summary>Resuelve el proyecto del seed por (cliente, proyecto); falla claro si falta en el catalogo.</summary>
    private static (int Id, string Cliente, string Proyecto) ProyectoId(
        Dictionary<string, int> porNombre, string cliente, string proyecto) =>
        porNombre.TryGetValue(cliente + "|" + proyecto, out var id)
            ? (id, cliente, proyecto)
            : throw new InvalidOperationException(
                $"El seed referencia el proyecto '{proyecto}' del cliente '{cliente}', que no existe en el catalogo.");

    private static RegistroHoras Reg(
        string userId, DateOnly fecha,
        TimeOnly? entrada1, TimeOnly? salida1,
        TimeOnly? entrada2, TimeOnly? salida2,
        (int Id, string Cliente, string Proyecto) proyecto, string modalidad, string recurso,
        string descripcion, string lugar,
        bool esRetroactivo = false,
        TimeOnly? entrada3 = null, TimeOnly? salida3 = null) =>
        new(userId, fecha,
            entrada1, salida1, entrada2, salida2, entrada3, salida3,
            proyecto.Id, proyecto.Cliente, proyecto.Proyecto,
            modalidad, recurso, descripcion, lugar, esRetroactivo);

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

        // Crear tabla si no existe (esquema nuevo con horarios 1/2/3)
        await _context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[dbo].[RegistrosHoras]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[RegistrosHoras] (
                    [Id]           int NOT NULL IDENTITY,
                    [UserId]       nvarchar(450) NOT NULL,
                    [FechaRegistro] date NOT NULL,
                    [HoraEntrada1] time NULL,
                    [HoraSalida1]  time NULL,
                    [HoraEntrada2] time NULL,
                    [HoraSalida2]  time NULL,
                    [HoraEntrada3] time NULL,
                    [HoraSalida3]  time NULL,
                    [ProyectoId]   int NOT NULL,
                    [ClienteNombre]  nvarchar(200) NOT NULL,
                    [ProyectoNombre] nvarchar(200) NOT NULL,
                    [Modalidad]    nvarchar(100) NOT NULL,
                    [Recurso]      nvarchar(100) NOT NULL,
                    [Descripcion]  nvarchar(1000) NOT NULL,
                    [Lugar]        nvarchar(200) NOT NULL,
                    [EsRetroactivo] bit NOT NULL DEFAULT 0,
                    [Estado] int NOT NULL DEFAULT 0,
                    [EstadoPrevioAlRechazo] int NULL,
                    [ComentarioRechazo] nvarchar(1000) NULL,
                    [Created]      datetimeoffset NOT NULL,
                    [CreatedBy]    nvarchar(max) NULL,
                    [LastModified]  datetimeoffset NOT NULL,
                    [LastModifiedBy] nvarchar(max) NULL,
                    CONSTRAINT [PK_RegistrosHoras] PRIMARY KEY ([Id])
                );

                CREATE UNIQUE INDEX [IX_RegistrosHoras_UserId_FechaRegistro_ProyectoId]
                    ON [dbo].[RegistrosHoras] ([UserId], [FechaRegistro], [ProyectoId]);
            END
            """);

        // Migracion anterior: EsRetroactivo
        await _context.Database.ExecuteSqlRawAsync("""
            IF NOT EXISTS (
                SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID(N'[dbo].[RegistrosHoras]')
                  AND name = N'EsRetroactivo'
            )
            BEGIN
                ALTER TABLE [dbo].[RegistrosHoras]
                    ADD [EsRetroactivo] bit NOT NULL DEFAULT 0;
            END
            """);

        // Migracion registro unico diario — paso 1: agregar columnas AM/PM.
        // Solo aplica a una BD anterior a la fusion diaria: la condicion sobre HoraEntrada1
        // evita que, una vez migrada a horarios 1/2/3, este paso vuelva a crear las columnas
        // AM/PM en cada arranque.
        await _context.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'[dbo].[RegistrosHoras]', N'HoraEntradaAM') IS NULL
               AND COL_LENGTH(N'[dbo].[RegistrosHoras]', N'HoraEntrada1') IS NULL
            BEGIN
                ALTER TABLE [dbo].[RegistrosHoras] ADD [HoraEntradaAM] time NULL;
                ALTER TABLE [dbo].[RegistrosHoras] ADD [HoraSalidaAM]  time NULL;
                ALTER TABLE [dbo].[RegistrosHoras] ADD [HoraEntradaPM] time NULL;
                ALTER TABLE [dbo].[RegistrosHoras] ADD [HoraSalidaPM]  time NULL;
            END
            """);

        // Migracion registro unico diario — paso 2: copiar datos de Turno viejo a columnas AM/PM
        // Cada UPDATE es un batch separado para evitar error de columna inexistente en compile-time
        await _context.Database.ExecuteSqlRawAsync("""
            IF EXISTS (
                SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID(N'[dbo].[RegistrosHoras]')
                  AND name = N'Turno'
            )
            BEGIN
                EXEC('UPDATE [dbo].[RegistrosHoras] SET HoraEntradaAM = HoraEntrada, HoraSalidaAM = HoraSalida WHERE Turno = ''AM''');
                EXEC('UPDATE [dbo].[RegistrosHoras] SET HoraEntradaPM = HoraEntrada, HoraSalidaPM = HoraSalida WHERE Turno = ''PM''');
            END
            """);

        // Migracion registro unico diario — paso 3: fusionar PM en AM y borrar duplicados
        await _context.Database.ExecuteSqlRawAsync("""
            IF EXISTS (
                SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID(N'[dbo].[RegistrosHoras]')
                  AND name = N'Turno'
            )
            BEGIN
                EXEC('
                    UPDATE am_rec
                    SET am_rec.HoraEntradaPM = pm_rec.HoraEntradaPM,
                        am_rec.HoraSalidaPM  = pm_rec.HoraSalidaPM
                    FROM [dbo].[RegistrosHoras] am_rec
                    INNER JOIN [dbo].[RegistrosHoras] pm_rec
                        ON am_rec.UserId = pm_rec.UserId
                       AND am_rec.FechaRegistro = pm_rec.FechaRegistro
                       AND am_rec.Turno = ''AM''
                       AND pm_rec.Turno = ''PM''
                ');
                EXEC('
                    DELETE FROM [dbo].[RegistrosHoras]
                    WHERE Turno = ''PM''
                      AND EXISTS (
                          SELECT 1 FROM [dbo].[RegistrosHoras] am
                          WHERE am.UserId = [dbo].[RegistrosHoras].UserId
                            AND am.FechaRegistro = [dbo].[RegistrosHoras].FechaRegistro
                            AND am.Turno = ''AM''
                      )
                ');
            END
            """);

        // Migracion registro unico diario — paso 4: eliminar columnas e indice viejos
        await _context.Database.ExecuteSqlRawAsync("""
            IF EXISTS (
                SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID(N'[dbo].[RegistrosHoras]')
                  AND name = N'Turno'
            )
            BEGIN
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RegistrosHoras_FechaRegistro' AND object_id = OBJECT_ID(N'[dbo].[RegistrosHoras]'))
                    DROP INDEX [IX_RegistrosHoras_FechaRegistro] ON [dbo].[RegistrosHoras];

                ALTER TABLE [dbo].[RegistrosHoras] DROP COLUMN [Turno];
                ALTER TABLE [dbo].[RegistrosHoras] DROP COLUMN [HoraEntrada];
                ALTER TABLE [dbo].[RegistrosHoras] DROP COLUMN [HoraSalida];
            END
            """);

        // Migracion registro unico diario — paso 5: fusionar y limpiar duplicados restantes
        // Para cada par (UserId, FechaRegistro) duplicado: copia AM/PM del segundo al primero y lo borra
        // Guardado por existencia de las columnas AM/PM: tras la migracion a horarios 1/2/3
        // estas columnas ya no existen y el batch fallaria al compilar.
        await _context.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'[dbo].[RegistrosHoras]', N'HoraEntradaAM') IS NOT NULL
            BEGIN
                EXEC('
                    UPDATE r1
                    SET r1.HoraEntradaAM = COALESCE(r1.HoraEntradaAM, r2.HoraEntradaAM),
                        r1.HoraSalidaAM  = COALESCE(r1.HoraSalidaAM,  r2.HoraSalidaAM),
                        r1.HoraEntradaPM = COALESCE(r1.HoraEntradaPM, r2.HoraEntradaPM),
                        r1.HoraSalidaPM  = COALESCE(r1.HoraSalidaPM,  r2.HoraSalidaPM)
                    FROM [dbo].[RegistrosHoras] r1
                    INNER JOIN [dbo].[RegistrosHoras] r2
                        ON r1.UserId = r2.UserId
                       AND r1.FechaRegistro = r2.FechaRegistro
                       AND r1.Id < r2.Id
                ');
            END
            """);

        // Borra los sobrantes que el UPDATE de arriba acaba de fusionar. Lleva EL MISMO guard
        // que aquel porque es su continuacion: sin guard corria en cada arranque y, con el
        // modelo actual (un registro por proyecto y dia), habria borrado todos los registros
        // de un dia menos el primero — datos legitimos, no duplicados.
        await _context.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'[dbo].[RegistrosHoras]', N'HoraEntradaAM') IS NOT NULL
            BEGIN
                DELETE r
                FROM [dbo].[RegistrosHoras] r
                WHERE EXISTS (
                    SELECT 1 FROM [dbo].[RegistrosHoras] r2
                    WHERE r2.UserId = r.UserId
                      AND r2.FechaRegistro = r.FechaRegistro
                      AND r2.Id < r.Id
                )
            END
            """);

        // Migracion registro unico diario — paso 6: crear indice unico por usuario/dia/proyecto
        // Si existe el indice viejo (solo UserId+FechaRegistro), lo reemplaza por el correcto
        await _context.Database.ExecuteSqlRawAsync("""
            IF EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE name = N'IX_RegistrosHoras_UserId_FechaRegistro'
                  AND object_id = OBJECT_ID(N'[dbo].[RegistrosHoras]')
            )
            BEGIN
                DROP INDEX [IX_RegistrosHoras_UserId_FechaRegistro] ON [dbo].[RegistrosHoras];
            END
            """);

        // Red de seguridad: normalmente el indice ya lo crearon la creacion de la tabla o la
        // migracion a ProyectoId. Solo actua si falta de verdad.
        // El guard pregunta por el MISMO nombre que crea: antes preguntaba por el nombre
        // viejo (_Cliente_Proyecto) y creaba el nuevo, asi que en una BD ya migrada intentaba
        // crearlo por segunda vez y el arranque reventaba.
        // Tambien exige que exista ProyectoId: en una BD aun sin migrar la columna no esta.
        await _context.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'[dbo].[RegistrosHoras]', N'ProyectoId') IS NOT NULL
               AND NOT EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE name = N'IX_RegistrosHoras_UserId_FechaRegistro_ProyectoId'
                  AND object_id = OBJECT_ID(N'[dbo].[RegistrosHoras]')
            )
            BEGIN
                CREATE UNIQUE INDEX [IX_RegistrosHoras_UserId_FechaRegistro_ProyectoId]
                    ON [dbo].[RegistrosHoras] ([UserId], [FechaRegistro], [ProyectoId]);
            END
            """);

        // ── Migracion horarios 1/2/3 ─────────────────────────────────────────
        // Renombra AM->1 y PM->2 preservando los datos, y agrega el horario 3.
        // sp_rename conserva el contenido: no se pierde ningun registro existente.
        await _context.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'[dbo].[RegistrosHoras]', N'HoraEntradaAM') IS NOT NULL
               AND COL_LENGTH(N'[dbo].[RegistrosHoras]', N'HoraEntrada1') IS NULL
            BEGIN
                EXEC sp_rename N'[dbo].[RegistrosHoras].[HoraEntradaAM]', N'HoraEntrada1', 'COLUMN';
                EXEC sp_rename N'[dbo].[RegistrosHoras].[HoraSalidaAM]',  N'HoraSalida1',  'COLUMN';
                EXEC sp_rename N'[dbo].[RegistrosHoras].[HoraEntradaPM]', N'HoraEntrada2', 'COLUMN';
                EXEC sp_rename N'[dbo].[RegistrosHoras].[HoraSalidaPM]',  N'HoraSalida2',  'COLUMN';
            END
            """);

        await _context.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'[dbo].[RegistrosHoras]', N'HoraEntrada3') IS NULL
            BEGIN
                ALTER TABLE [dbo].[RegistrosHoras] ADD [HoraEntrada3] time NULL;
                ALTER TABLE [dbo].[RegistrosHoras] ADD [HoraSalida3]  time NULL;
            END
            """);

        // Índice de rendimiento: dashboard y reportes filtran por rango de FechaRegistro
        // sin predicado de UserId — el composite (UserId, FechaRegistro, ...) no aplica en esos casos.
        // INCLUDE (UserId) cubre los JOINs a AspNetUsers sin volver al clúster.
        await _context.Database.ExecuteSqlRawAsync("""
            IF NOT EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE name = N'IX_RegistrosHoras_FechaRegistro'
                  AND object_id = OBJECT_ID(N'[dbo].[RegistrosHoras]')
            )
            BEGIN
                CREATE INDEX [IX_RegistrosHoras_FechaRegistro]
                    ON [dbo].[RegistrosHoras] ([FechaRegistro])
                    INCLUDE ([UserId]);
            END
            """);

        // Índice de rendimiento: queries de dashboard filtran WHERE u.IsActive = 1
        await _context.Database.ExecuteSqlRawAsync("""
            IF NOT EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE name = N'IX_AspNetUsers_IsActive'
                  AND object_id = OBJECT_ID(N'[dbo].[AspNetUsers]')
            )
            BEGIN
                CREATE INDEX [IX_AspNetUsers_IsActive]
                    ON [dbo].[AspNetUsers] ([IsActive]);
            END
            """);

        // ── Estructura organizacional (Fase 4) ────────────────────────────────
        // Jefe directo de cada persona: define el organigrama y la 2a aprobacion.
        await _context.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'[dbo].[AspNetUsers]', N'SupervisorUserId') IS NULL
            BEGIN
                ALTER TABLE [dbo].[AspNetUsers] ADD [SupervisorUserId] nvarchar(450) NULL;
            END
            """);

        // Puesto de la persona (catalogo Empleados): resuelve la 1a aprobacion.
        await _context.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'[dbo].[AspNetUsers]', N'PuestoId') IS NULL
            BEGIN
                ALTER TABLE [dbo].[AspNetUsers] ADD [PuestoId] int NULL;
            END
            """);

        await _context.Database.ExecuteSqlRawAsync("""
            IF NOT EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE name = N'IX_AspNetUsers_SupervisorUserId'
                  AND object_id = OBJECT_ID(N'[dbo].[AspNetUsers]')
            )
            BEGIN
                CREATE INDEX [IX_AspNetUsers_SupervisorUserId]
                    ON [dbo].[AspNetUsers] ([SupervisorUserId]);
            END
            """);

        // Responsable del proyecto: 3a y ultima aprobacion.
        await _context.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'[dbo].[Proyectos]', N'SupervisorUserId') IS NULL
            BEGIN
                ALTER TABLE [dbo].[Proyectos] ADD [SupervisorUserId] nvarchar(450) NULL;
            END
            """);

        await _context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[dbo].[ReglasVentanaRetroactividad]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[ReglasVentanaRetroactividad] (
                    [Id] int NOT NULL IDENTITY,
                    [UserId] nvarchar(450) NULL,
                    [Rol] nvarchar(100) NULL,
                    [Dias] int NOT NULL,
                    [Activo] bit NOT NULL DEFAULT 1,
                    [Created] datetimeoffset NOT NULL,
                    [CreatedBy] nvarchar(max) NULL,
                    [LastModified] datetimeoffset NOT NULL,
                    [LastModifiedBy] nvarchar(max) NULL,
                    CONSTRAINT [PK_ReglasVentanaRetroactividad] PRIMARY KEY ([Id])
                );

                -- Filtrados: la columna que no aplica queda NULL, y un unique normal
                -- trataria esos NULL como iguales dejando una sola regla en la tabla.
                CREATE UNIQUE INDEX [IX_ReglasVentanaRetroactividad_UserId]
                    ON [dbo].[ReglasVentanaRetroactividad] ([UserId])
                    WHERE [UserId] IS NOT NULL;

                CREATE UNIQUE INDEX [IX_ReglasVentanaRetroactividad_Rol]
                    ON [dbo].[ReglasVentanaRetroactividad] ([Rol])
                    WHERE [Rol] IS NOT NULL;
            END
            """);

        await _context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[dbo].[ParametrosRestriccionDia]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[ParametrosRestriccionDia] (
                    [Id] int NOT NULL IDENTITY,
                    [DiaDelaSemana] int NOT NULL,
                    [UserId] nvarchar(450) NULL,
                    [Rol] nvarchar(100) NULL,
                    [Activo] bit NOT NULL DEFAULT 1,
                    [Created] datetimeoffset NOT NULL,
                    [CreatedBy] nvarchar(max) NULL,
                    [LastModified] datetimeoffset NOT NULL,
                    [LastModifiedBy] nvarchar(max) NULL,
                    CONSTRAINT [PK_ParametrosRestriccionDia] PRIMARY KEY ([Id])
                );

                -- Filtrados: la columna que no aplica queda NULL, y un unique normal
                -- trataria esos NULL como iguales dejando una sola regla por dia.
                CREATE UNIQUE INDEX [IX_ParametrosRestriccionDia_Dia_UserId]
                    ON [dbo].[ParametrosRestriccionDia] ([DiaDelaSemana], [UserId])
                    WHERE [UserId] IS NOT NULL;

                CREATE UNIQUE INDEX [IX_ParametrosRestriccionDia_Dia_Rol]
                    ON [dbo].[ParametrosRestriccionDia] ([DiaDelaSemana], [Rol])
                    WHERE [Rol] IS NOT NULL;
            END
            """);

        await _context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[dbo].[ReportesUsuario]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[ReportesUsuario] (
                    [Id] int NOT NULL IDENTITY,
                    [UserId] nvarchar(450) NOT NULL,
                    [Tipo] nvarchar(20) NOT NULL,
                    [Titulo] nvarchar(200) NOT NULL,
                    [Descripcion] nvarchar(2000) NOT NULL,
                    [Estado] nvarchar(20) NOT NULL,
                    [ComentarioRespuesta] nvarchar(2000) NULL,
                    [RespondidoPorUserId] nvarchar(450) NULL,
                    [Created] datetimeoffset NOT NULL,
                    [CreatedBy] nvarchar(max) NULL,
                    [LastModified] datetimeoffset NOT NULL,
                    [LastModifiedBy] nvarchar(max) NULL,
                    CONSTRAINT [PK_ReportesUsuario] PRIMARY KEY ([Id])
                );

                CREATE INDEX [IX_ReportesUsuario_UserId] ON [dbo].[ReportesUsuario] ([UserId]);
                CREATE INDEX [IX_ReportesUsuario_Estado] ON [dbo].[ReportesUsuario] ([Estado]);
            END
            """);

        await _context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[dbo].[SupervisoresPuesto]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[SupervisoresPuesto] (
                    [Id] int NOT NULL IDENTITY,
                    [PuestoId] int NOT NULL,
                    [ClienteId] int NULL,
                    [SupervisorUserId] nvarchar(450) NOT NULL,
                    [Activo] bit NOT NULL DEFAULT 1,
                    [Created] datetimeoffset NOT NULL,
                    [CreatedBy] nvarchar(max) NULL,
                    [LastModified] datetimeoffset NOT NULL,
                    [LastModifiedBy] nvarchar(max) NULL,
                    CONSTRAINT [PK_SupervisoresPuesto] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_SupervisoresPuesto_Empleados] FOREIGN KEY ([PuestoId])
                        REFERENCES [dbo].[Empleados] ([Id]),
                    CONSTRAINT [FK_SupervisoresPuesto_Clientes] FOREIGN KEY ([ClienteId])
                        REFERENCES [dbo].[Clientes] ([Id])
                );

                -- Indices filtrados: la regla general (ClienteId NULL) convive con las
                -- especificas del mismo puesto. Un unique normal trataria los NULL como
                -- iguales y solo dejaria una regla general en toda la tabla.
                CREATE UNIQUE INDEX [IX_SupervisoresPuesto_PuestoId_ClienteId]
                    ON [dbo].[SupervisoresPuesto] ([PuestoId], [ClienteId])
                    WHERE [ClienteId] IS NOT NULL;

                CREATE UNIQUE INDEX [IX_SupervisoresPuesto_PuestoId_General]
                    ON [dbo].[SupervisoresPuesto] ([PuestoId])
                    WHERE [ClienteId] IS NULL;
            END
            """);

        await _context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[dbo].[ParametrosSistema]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[ParametrosSistema] (
                    [Id] int NOT NULL IDENTITY,
                    [Clave] nvarchar(100) NOT NULL,
                    [Valor] nvarchar(max) NOT NULL,
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
            IF EXISTS (
                SELECT 1 FROM sys.columns
                WHERE object_id = OBJECT_ID(N'[dbo].[ParametrosSistema]')
                  AND name = N'Valor'
                  AND max_length <> -1
            )
            BEGIN
                -- Tablas creadas antes de que existiera el logo de reportes quedaron con
                -- nvarchar(500): no alcanza para una imagen en base64. -1 = ya es
                -- nvarchar(max), asi que en instalaciones nuevas esto no hace nada.
                ALTER TABLE [dbo].[ParametrosSistema] ALTER COLUMN [Valor] nvarchar(max) NOT NULL;
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

        // ── Migracion a ProyectoId ───────────────────────────────────────────
        // RegistrosHoras guardaba Cliente y Proyecto como texto suelto. Se sustituyen por
        // la clave del proyecto, que ya lleva su cliente. Pasos separados para que el
        // backfill ocurra con la columna creada y antes de volverla obligatoria.
        await _context.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'[dbo].[RegistrosHoras]', N'ProyectoId') IS NULL
               AND COL_LENGTH(N'[dbo].[RegistrosHoras]', N'Proyecto') IS NOT NULL
            BEGIN
                ALTER TABLE [dbo].[RegistrosHoras] ADD [ProyectoId] int NULL;
            END
            """);

        await _context.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'[dbo].[RegistrosHoras]', N'Proyecto') IS NOT NULL
            BEGIN
                EXEC('
                    UPDATE r
                    SET    r.ProyectoId = p.Id
                    FROM   [dbo].[RegistrosHoras] r
                    JOIN   [dbo].[Clientes]  c ON c.Nombre = r.Cliente
                    JOIN   [dbo].[Proyectos] p ON p.Nombre = r.Proyecto AND p.ClienteId = c.Id
                    WHERE  r.ProyectoId IS NULL
                ');
            END
            """);

        // Si algo no emparejo, el despliegue debe fallar aqui y no dejar la tabla a medias.
        // El diagnostico previo (Docs/operations/diagnostico-parejas-cliente-proyecto.sql)
        // debe dar 0 filas invalidas antes de desplegar.
        await _context.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'[dbo].[RegistrosHoras]', N'Proyecto') IS NOT NULL
            BEGIN
                DECLARE @huerfanos int;
                EXEC sp_executesql
                    N'SELECT @c = COUNT(*) FROM [dbo].[RegistrosHoras] WHERE ProyectoId IS NULL',
                    N'@c int OUTPUT', @c = @huerfanos OUTPUT;

                IF @huerfanos > 0
                BEGIN
                    DECLARE @msg nvarchar(400) = CONCAT(
                        'Migracion a ProyectoId abortada: ', @huerfanos,
                        ' registro(s) con pareja (Cliente, Proyecto) inexistente en el catalogo. ',
                        'Ejecutar Docs/operations/diagnostico-parejas-cliente-proyecto.sql y corregirlos.');
                    THROW 50001, @msg, 1;
                END
            END
            """);

        await _context.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'[dbo].[RegistrosHoras]', N'Proyecto') IS NOT NULL
            BEGIN
                IF EXISTS (SELECT 1 FROM sys.indexes
                           WHERE name = N'IX_RegistrosHoras_UserId_FechaRegistro_Cliente_Proyecto'
                             AND object_id = OBJECT_ID(N'[dbo].[RegistrosHoras]'))
                    DROP INDEX [IX_RegistrosHoras_UserId_FechaRegistro_Cliente_Proyecto] ON [dbo].[RegistrosHoras];

                ALTER TABLE [dbo].[RegistrosHoras] ALTER COLUMN [ProyectoId] int NOT NULL;

                -- Las columnas de texto NO se borran: se renombran y pasan a ser la foto
                -- del nombre en el momento del registro. Asi el historico conserva como se
                -- llamaba el cliente/proyecto aunque luego se renombren en el catalogo.
                EXEC sp_rename N'[dbo].[RegistrosHoras].[Cliente]',  N'ClienteNombre',  'COLUMN';
                EXEC sp_rename N'[dbo].[RegistrosHoras].[Proyecto]', N'ProyectoNombre', 'COLUMN';

                ALTER TABLE [dbo].[RegistrosHoras]
                    ADD CONSTRAINT [FK_RegistrosHoras_Proyectos] FOREIGN KEY ([ProyectoId])
                        REFERENCES [dbo].[Proyectos] ([Id]);

                CREATE UNIQUE INDEX [IX_RegistrosHoras_UserId_FechaRegistro_ProyectoId]
                    ON [dbo].[RegistrosHoras] ([UserId], [FechaRegistro], [ProyectoId]);
            END
            """);

        // Respaldo: una BD que ya hubiera corrido una version anterior de esta migracion
        // pudo quedarse sin las columnas de texto. Se recrean desde el catalogo; el nombre
        // sera el actual, no el historico, pero es lo mejor recuperable.
        await _context.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'[dbo].[RegistrosHoras]', N'ProyectoId') IS NOT NULL
               AND COL_LENGTH(N'[dbo].[RegistrosHoras]', N'ClienteNombre') IS NULL
            BEGIN
                ALTER TABLE [dbo].[RegistrosHoras] ADD [ClienteNombre]  nvarchar(200) NULL;
                ALTER TABLE [dbo].[RegistrosHoras] ADD [ProyectoNombre] nvarchar(200) NULL;
            END
            """);

        await _context.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'[dbo].[RegistrosHoras]', N'ClienteNombre') IS NOT NULL
            BEGIN
                EXEC('
                    UPDATE r
                    SET    r.ClienteNombre  = c.Nombre,
                           r.ProyectoNombre = p.Nombre
                    FROM   [dbo].[RegistrosHoras] r
                    JOIN   [dbo].[Proyectos] p ON p.Id = r.ProyectoId
                    JOIN   [dbo].[Clientes]  c ON c.Id = p.ClienteId
                    WHERE  r.ClienteNombre IS NULL OR r.ProyectoNombre IS NULL
                ');

                IF NOT EXISTS (SELECT 1 FROM [dbo].[RegistrosHoras]
                               WHERE ClienteNombre IS NULL OR ProyectoNombre IS NULL)
                BEGIN
                    ALTER TABLE [dbo].[RegistrosHoras] ALTER COLUMN [ClienteNombre]  nvarchar(200) NOT NULL;
                    ALTER TABLE [dbo].[RegistrosHoras] ALTER COLUMN [ProyectoNombre] nvarchar(200) NOT NULL;
                END
            END
            """);

        // ── Registro adjunto a la solicitud de excepcion (Fase 2) ────────────
        // Nullable: las solicitudes anteriores no llevan registro y siguen siendo validas.
        await _context.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'[dbo].[SolicitudesExcepcion]', N'ProyectoId') IS NULL
            BEGIN
                ALTER TABLE [dbo].[SolicitudesExcepcion] ADD
                    [ProyectoId]     int NULL,
                    [ClienteNombre]  nvarchar(200) NULL,
                    [ProyectoNombre] nvarchar(200) NULL,
                    [HoraEntrada1]   time NULL,
                    [HoraSalida1]    time NULL,
                    [HoraEntrada2]   time NULL,
                    [HoraSalida2]    time NULL,
                    [HoraEntrada3]   time NULL,
                    [HoraSalida3]    time NULL,
                    [Modalidad]      nvarchar(100) NULL,
                    [Recurso]        nvarchar(100) NULL,
                    [Lugar]          nvarchar(200) NULL,
                    [Descripcion]    nvarchar(1000) NULL;
            END
            """);

        // ── Cadena de aprobacion (Fase 5) ────────────────────────────────────
        // Los registros previos quedan en Pendiente (0), que es el estado inicial correcto:
        // nadie los ha revisado todavia.
        await _context.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'[dbo].[RegistrosHoras]', N'Estado') IS NULL
            BEGIN
                ALTER TABLE [dbo].[RegistrosHoras]
                    ADD [Estado] int NOT NULL CONSTRAINT [DF_RegistrosHoras_Estado] DEFAULT(0);
                ALTER TABLE [dbo].[RegistrosHoras] ADD [EstadoPrevioAlRechazo] int NULL;
                ALTER TABLE [dbo].[RegistrosHoras] ADD [ComentarioRechazo] nvarchar(1000) NULL;
            END
            """);

        await _context.Database.ExecuteSqlRawAsync("""
            IF NOT EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE name = N'IX_RegistrosHoras_Estado_FechaRegistro'
                  AND object_id = OBJECT_ID(N'[dbo].[RegistrosHoras]')
            )
            BEGIN
                CREATE INDEX [IX_RegistrosHoras_Estado_FechaRegistro]
                    ON [dbo].[RegistrosHoras] ([Estado], [FechaRegistro]);
            END
            """);

        await _context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[dbo].[AprobacionesRegistro]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[AprobacionesRegistro] (
                    [Id] int NOT NULL IDENTITY,
                    [RegistroHorasId] int NOT NULL,
                    [Accion] nvarchar(50) NOT NULL,
                    [Nivel] int NULL,
                    [ActorUserId] nvarchar(450) NOT NULL,
                    [Comentario] nvarchar(1000) NULL,
                    [Created] datetimeoffset NOT NULL,
                    [CreatedBy] nvarchar(max) NULL,
                    [LastModified] datetimeoffset NOT NULL,
                    [LastModifiedBy] nvarchar(max) NULL,
                    CONSTRAINT [PK_AprobacionesRegistro] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_AprobacionesRegistro_RegistrosHoras] FOREIGN KEY ([RegistroHorasId])
                        REFERENCES [dbo].[RegistrosHoras] ([Id]) ON DELETE CASCADE
                );

                CREATE INDEX [IX_AprobacionesRegistro_RegistroHorasId]
                    ON [dbo].[AprobacionesRegistro] ([RegistroHorasId]);
            END
            """);

        // ── Modalidades: "Presencial" pasa a llamarse "Cliente" e "Hibrido" se retira ──
        // El rename tambien se aplica al historico de RegistrosHoras: es un cambio de
        // nombre, no de significado, y dejarlo a medias partiria los agrupamientos
        // de los reportes entre 'Presencial' y 'Cliente'.
        await _context.Database.ExecuteSqlRawAsync("""
            IF EXISTS (SELECT 1 FROM [dbo].[Modalidades] WHERE [Nombre] = N'Presencial')
            BEGIN
                -- Si 'Cliente' ya existe no se puede renombrar: el indice unico lo impide.
                IF EXISTS (SELECT 1 FROM [dbo].[Modalidades] WHERE [Nombre] = N'Cliente')
                    DELETE FROM [dbo].[Modalidades] WHERE [Nombre] = N'Presencial'
                ELSE
                    UPDATE [dbo].[Modalidades] SET [Nombre] = N'Cliente' WHERE [Nombre] = N'Presencial'

                UPDATE [dbo].[RegistrosHoras] SET [Modalidad] = N'Cliente' WHERE [Modalidad] = N'Presencial';
            END
            """);

        // Hibrido se desactiva en lugar de borrarse: los registros historicos que lo
        // usaron conservan su valor, pero deja de ofrecerse al registrar.
        await _context.Database.ExecuteSqlRawAsync("""
            UPDATE [dbo].[Modalidades]
            SET    [Activo] = 0
            WHERE  [Nombre] IN (N'Hibrido', N'Híbrido') AND [Activo] = 1;
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
                CREATE INDEX [IX_NotificacionesEnviadas_Created]
                    ON [dbo].[NotificacionesEnviadas] ([Created]);
            END
            """);

        await _context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[dbo].[NotificacionesEnviadas]', N'U') IS NOT NULL
               AND NOT EXISTS (
                   SELECT 1 FROM sys.indexes
                   WHERE name = N'IX_NotificacionesEnviadas_Created'
                     AND object_id = OBJECT_ID(N'[dbo].[NotificacionesEnviadas]')
               )
            BEGIN
                CREATE INDEX [IX_NotificacionesEnviadas_Created]
                    ON [dbo].[NotificacionesEnviadas] ([Created]);
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
