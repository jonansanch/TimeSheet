using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Domain.Enums;
using KPG.Timesheet.Infrastructure.Data;
using KPG.Timesheet.Infrastructure.Organizacion;
using Microsoft.EntityFrameworkCore;
using RegistroHorasEntity = KPG.Timesheet.Domain.Entities.RegistroHoras;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.Data;

/// <summary>
/// El seed no sirve de nada si deja la cadena de aprobacion resolviendo vacia: un nivel
/// sin aprobador no lo aprueba nadie, asi que un ambiente recien levantado tendria todos
/// los registros trabados. Esto verifica que los tres niveles resuelven a alguien.
/// </summary>
public class SeedEstructuraTests
{
    [Fact]
    public async Task Cadena_ConLaEstructuraDelSeed_ShouldResolverLosTresNiveles()
    {
        await using var context = await EscenarioAsync();
        var proyectoId = await ProyectoDeAsync(context, "Banco Nacional");

        var cadena = await new CadenaAprobacionService(context)
            .ResolverAsync(EmpleadoId, proyectoId, CancellationToken.None);

        cadena.SupervisorPuestoUserId.Should().Be(SupervisorId);   // por el puesto
        cadena.SupervisorUsuarioUserId.Should().Be(SupervisorId);  // jefe directo
        cadena.SupervisorProyectoUserId.Should().Be(GerenteId);    // responsable del proyecto
    }

    [Fact]
    public async Task Cadena_ConReglaEspecificaDeCliente_ShouldGanarSobreLaGeneral()
    {
        // El seed carga una regla de Petrocol para el mismo puesto: debe ganar.
        await using var context = await EscenarioAsync();
        var proyectoPetrocol = await ProyectoDeAsync(context, "Petrocol");

        var cadena = await new CadenaAprobacionService(context)
            .ResolverAsync(ConsultorSapId, proyectoPetrocol, CancellationToken.None);

        cadena.SupervisorPuestoUserId.Should().Be(GerenteId);
    }

    [Fact]
    public async Task Cadena_DelMismoPuestoEnOtroCliente_ShouldUsarLaReglaGeneral()
    {
        await using var context = await EscenarioAsync();
        var otroProyecto = await ProyectoDeAsync(context, "Banco Nacional");

        var cadena = await new CadenaAprobacionService(context)
            .ResolverAsync(ConsultorSapId, otroProyecto, CancellationToken.None);

        cadena.SupervisorPuestoUserId.Should().Be(SupervisorId);
    }

    [Fact]
    public async Task Cadena_SinPuestoAsignado_ShouldDejarElNivel1Vacio()
    {
        // Documenta el sintoma que motivo todo esto: sin puesto, nadie puede aprobar.
        await using var context = await EscenarioAsync();
        var usuario = await context.Users.FirstAsync(u => u.Id == EmpleadoId);
        usuario.PuestoId = null;
        await context.SaveChangesAsync(CancellationToken.None);

        var cadena = await new CadenaAprobacionService(context)
            .ResolverAsync(EmpleadoId, await ProyectoDeAsync(context, "Banco Nacional"), CancellationToken.None);

        cadena.SupervisorPuestoUserId.Should().BeNull();
    }

    [Fact]
    public async Task Registro_ConLaCadenaCompleta_ShouldPoderRecorrerLosTresNiveles()
    {
        await using var context = await EscenarioAsync();
        var registro = new RegistroHorasEntity(
            EmpleadoId, new DateOnly(2026, 5, 14),
            new TimeOnly(8, 0), new TimeOnly(17, 0), null, null, null, null,
            await ProyectoDeAsync(context, "Banco Nacional"),
            "Banco Nacional", "Core Bancario", "Remoto", "Desarrollador", "Trabajo", "Bogota");

        registro.Aprobar(1);
        registro.Aprobar(2);
        registro.Aprobar(3);

        registro.Estado.Should().Be(EstadoAprobacion.Aprobado);
        registro.NivelPendiente.Should().BeNull();
    }

    [Theory]
    [InlineData("Líder Técnico")]   // como esta escrito en el catalogo real
    [InlineData("Lider Tecnico")]
    [InlineData("lider tecnico")]
    [InlineData("LÍDER TÉCNICO")]
    public void BuscarPuesto_ShouldIgnorarTildesYMayusculas(string comoEstaEnElCatalogo)
    {
        // El bug que esto evita: el seed buscaba "Lider tecnico" y el catalogo decia
        // "Lider Tecnico" con tildes. No matcheaba, esa gente quedaba sin puesto, y sus
        // registros no los podia aprobar nadie porque el nivel 1 resolvia vacio.
        var catalogo = new Dictionary<string, string> { [Normalizar(comoEstaEnElCatalogo)] = "encontrado" };

        catalogo.TryGetValue(Normalizar("Lider tecnico"), out var resultado).Should().BeTrue();
        resultado.Should().Be("encontrado");
    }

    /// <summary>Misma normalizacion que usa el seed (ver ApplicationDbContextInitialiser).</summary>
    private static string Normalizar(string nombre)
    {
        var d = nombre.Trim().ToLowerInvariant().Normalize(System.Text.NormalizationForm.FormD);
        var sinTildes = new string(d.Where(c =>
            System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
            != System.Globalization.UnicodeCategory.NonSpacingMark).ToArray());
        return sinTildes.Normalize(System.Text.NormalizationForm.FormC);
    }

    [Fact]
    public async Task Cadena_DeLaCabezaQueEsSuPropioJefe_ShouldResolverElNivel2()
    {
        // Decision del cliente: todos tienen jefe, y el de mas arriba es el suyo propio.
        // Sin esto sus registros quedaban trabados para siempre en el nivel 2.
        await using var context = await EscenarioAsync();
        var cabeza = await context.Users.FirstAsync(u => u.Id == EmpleadoId);
        cabeza.SupervisorUserId = cabeza.Id;
        await context.SaveChangesAsync(CancellationToken.None);

        var cadena = await new CadenaAprobacionService(context)
            .ResolverAsync(EmpleadoId, await ProyectoDeAsync(context, "Banco Nacional"), CancellationToken.None);

        cadena.SupervisorUsuarioUserId.Should().Be(EmpleadoId);
    }

    // ── Escenario equivalente al que deja el seed ────────────────────────────

    private const string GerenteId       = "gerente-1";
    private const string SupervisorId    = "supervisor-1";
    private const string EmpleadoId      = "empleado-1";
    private const string ConsultorSapId  = "empleado-3";

    private static async Task<int> ProyectoDeAsync(ApplicationDbContext context, string cliente) =>
        await context.Proyectos
            .Where(p => context.Clientes.Any(c => c.Id == p.ClienteId && c.Nombre == cliente))
            .Select(p => p.Id)
            .FirstAsync();

    private static async Task<ApplicationDbContext> EscenarioAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);

        var banco    = new Cliente("Banco Nacional");
        var petrocol = new Cliente("Petrocol");
        context.Clientes.AddRange(banco, petrocol);

        var desarrollador = new Empleado("Desarrollador");
        var consultorSap  = new Empleado("Consultor SAP");
        context.Empleados.AddRange(desarrollador, consultorSap);
        await context.SaveChangesAsync(CancellationToken.None);

        var core = new Proyecto(banco.Id, "Core Bancario");
        var sap  = new Proyecto(petrocol.Id, "SAP FI");
        core.AsignarSupervisor(GerenteId);
        sap.AsignarSupervisor(GerenteId);
        context.Proyectos.AddRange(core, sap);

        // Reglas de puesto: generales al supervisor, y la de Petrocol a la gerente.
        context.SupervisoresPuesto.AddRange(
            new SupervisorPuesto(desarrollador.Id, SupervisorId),
            new SupervisorPuesto(consultorSap.Id,  SupervisorId),
            new SupervisorPuesto(consultorSap.Id,  GerenteId, petrocol.Id));

        context.Users.AddRange(
            new Infrastructure.Identity.ApplicationUser
            {
                Id = EmpleadoId, Email = "empleado@kpg.com", UserName = "empleado@kpg.com",
                IsActive = true, PuestoId = desarrollador.Id, SupervisorUserId = SupervisorId
            },
            new Infrastructure.Identity.ApplicationUser
            {
                Id = ConsultorSapId, Email = "carlos@kpg.com", UserName = "carlos@kpg.com",
                IsActive = true, PuestoId = consultorSap.Id, SupervisorUserId = SupervisorId
            });

        await context.SaveChangesAsync(CancellationToken.None);
        return context;
    }
}
