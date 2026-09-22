using NSubstitute;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Services;
using KPG.Timesheet.Application.Features.RegistroHoras.Commands.CreateRegistroHoras;
using KPG.Timesheet.Application.Features.RegistroHoras.Queries.GetMisRegistros;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.RegistroHoras;

/// <summary>
/// El registro guarda ProyectoId (para agrupar) y ademas una foto de los nombres
/// (para el historial). Estos tests fijan esa doble naturaleza.
/// </summary>
public class NombreHistoricoTests
{
    private static readonly DateOnly Hoy = new(2026, 5, 14);

    [Fact]
    public async Task Crear_ShouldSnapshotClienteAndProyectoNames()
    {
        await using var context = await CrearContextoAsync();

        var dto = await CrearRegistroAsync(context);

        dto.ProyectoId.Should().Be(1);
        dto.Cliente.Should().Be("Banco Nacional");
        dto.Proyecto.Should().Be("Core Bancario");
    }

    [Fact]
    public async Task RenombrarProyecto_ShouldNotChangeExistingRegistros()
    {
        // El motivo de guardar el texto: un timesheet es un documento historico y debe
        // decir como se llamaba el proyecto cuando se hizo el trabajo.
        await using var context = await CrearContextoAsync();
        await CrearRegistroAsync(context);

        var proyecto = await context.Proyectos.FirstAsync();
        proyecto.ActualizarNombre("Core Bancario v2");
        var cliente = await context.Clientes.FirstAsync();
        cliente.ActualizarNombre("Banco Nacional S.A.");
        await context.SaveChangesAsync(CancellationToken.None);

        var registro = await context.RegistrosHoras.FirstAsync();
        registro.ClienteNombre.Should().Be("Banco Nacional");
        registro.ProyectoNombre.Should().Be("Core Bancario");
    }

    [Fact]
    public async Task RenombrarProyecto_ShouldKeepPointingToTheSameProyectoId()
    {
        // La relacion no se rompe: sigue siendo el mismo proyecto, solo cambio su nombre.
        await using var context = await CrearContextoAsync();
        await CrearRegistroAsync(context);

        var proyecto = await context.Proyectos.FirstAsync();
        proyecto.ActualizarNombre("Core Bancario v2");
        await context.SaveChangesAsync(CancellationToken.None);

        var registro = await context.RegistrosHoras.FirstAsync();
        registro.ProyectoId.Should().Be(proyecto.Id);
    }

    [Fact]
    public async Task MisRegistros_ShouldShowTheHistoricNameNotTheCurrentOne()
    {
        await using var context = await CrearContextoAsync();
        await CrearRegistroAsync(context);

        var proyecto = await context.Proyectos.FirstAsync();
        proyecto.ActualizarNombre("Core Bancario v2");
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetMisRegistrosQueryHandler(context, new TestUser("user-1"), IdentityDePrueba());
        var items = (await handler.Handle(new GetMisRegistrosQuery(null, null), CancellationToken.None)).Items;

        items.Should().ContainSingle()
            .Which.Proyecto.Should().Be("Core Bancario");
    }

    [Fact]
    public async Task Crear_WhenProyectoDoesNotExist_ShouldFailInsteadOfSavingBlankNames()
    {
        await using var context = await CrearContextoAsync();
        var handler = MakeHandler(context);

        var act = () => handler.Handle(Command(proyectoId: 9999), CancellationToken.None);

        await act.Should().ThrowAsync<Application.Common.Exceptions.ValidationException>();
    }

    private static async Task<Application.Features.RegistroHoras.Commands.CreateRegistroHoras.RegistroHorasDto>
        CrearRegistroAsync(ApplicationDbContext context) =>
        await MakeHandler(context).Handle(Command(proyectoId: 1), CancellationToken.None);

    private static CreateRegistroHorasCommandHandler MakeHandler(ApplicationDbContext context) =>
        new(context, new TestUser("user-1"), new TestClock(Hoy), new NullBitacora(),
            new VentanaRetroactividadService(context, new ParametrosSistemaService(context)),
            new RestriccionDiaService(context));

    private static CreateRegistroHorasCommand Command(int proyectoId) =>
        new(Hoy,
            new TimeOnly(8, 0), new TimeOnly(13, 0),
            null, null,
            null, null,
            proyectoId, "Remoto", "Consultor", "Desarrollo", "Bogota");

    private static async Task<ApplicationDbContext> CrearContextoAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);

        var cliente = new Cliente("Banco Nacional");
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync(CancellationToken.None);

        context.Proyectos.Add(new Proyecto(cliente.Id, "Core Bancario"));
        await context.SaveChangesAsync(CancellationToken.None);

        return context;
    }

    private sealed class TestUser : IUser
    {
        public TestUser(string id) => Id = id;
        public string? Id { get; }
        public string? Email => null;
        public List<string>? Roles => [KPG.Timesheet.Domain.Constants.Roles.Empleado];
    }

    private sealed class TestClock : IClock
    {
        public TestClock(DateOnly today) => Today = today;
        public DateOnly Today { get; }
        public DateTimeOffset UtcNow => Today.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
    }

    private sealed class NullBitacora : IBitacoraService
    {
        public Task RegistrarAsync(string tipoEvento, string actorId, string? actorEmail,
            string entidadAfectada, string? entidadId, object? metadata = null,
            CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    /// <summary>
    /// El handler resuelve los nombres de quien aprobo cada nivel. A estas pruebas no les
    /// importa ese dato: lo que verifican es la consulta de registros.
    /// </summary>
    private static IIdentityService IdentityDePrueba()
    {
        var identity = Substitute.For<IIdentityService>();
        identity.GetUserNamesAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
                .Returns(new Dictionary<string, string>());
        return identity;
    }
}
