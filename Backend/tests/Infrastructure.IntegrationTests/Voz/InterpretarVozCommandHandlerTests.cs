using KPG.Timesheet.Application.Common.Exceptions;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Voz;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.Voz;

/// <summary>
/// Lo que importa aqui no es lo que responde el modelo, sino que el catalogo que se le
/// entrega salga de la base y solo traiga lo activo: es lo unico que evita que sugiera
/// clientes o proyectos que ya no existen.
/// </summary>
public class InterpretarVozCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldPasarleAlModeloElCatalogoRealDeLaBase()
    {
        await using var context = await EscenarioAsync();
        var espia = new InterpreteEspia();

        await Handle(context, espia, "entré a las ocho");

        espia.Catalogo!.Clientes.Should().BeEquivalentTo("Banco Nacional");
        espia.Catalogo.ProyectosPorCliente["Banco Nacional"].Should().BeEquivalentTo("Core Bancario");
        espia.Catalogo.Modalidades.Should().BeEquivalentTo("Remoto");
        espia.Catalogo.Recursos.Should().BeEquivalentTo("Consultor");
        espia.Catalogo.Lugares.Should().BeEquivalentTo("Bogota");
    }

    [Fact]
    public async Task Handle_ShouldExcluirLoInactivo()
    {
        // Un proyecto desactivado no puede aparecer como opcion: el registro lo rechazaria.
        await using var context = await EscenarioAsync();
        (await context.Proyectos.FirstAsync()).Desactivar();
        (await context.Modalidades.FirstAsync()).Desactivar();
        await context.SaveChangesAsync(CancellationToken.None);

        var espia = new InterpreteEspia();
        await Handle(context, espia, "entré a las ocho");

        espia.Catalogo!.Clientes.Should().BeEmpty();
        espia.Catalogo.Modalidades.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ConClienteInactivo_ShouldExcluirSusProyectos()
    {
        await using var context = await EscenarioAsync();
        (await context.Clientes.FirstAsync()).Desactivar();
        await context.SaveChangesAsync(CancellationToken.None);

        var espia = new InterpreteEspia();
        await Handle(context, espia, "entré a las ocho");

        espia.Catalogo!.ProyectosPorCliente.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldPasarLaFechaDeHoy()
    {
        // Sin la fecha de hoy el modelo no puede resolver "ayer" ni "el lunes pasado".
        await using var context = await EscenarioAsync();
        var espia = new InterpreteEspia();

        await Handle(context, espia, "ayer entré a las ocho");

        espia.Hoy.Should().Be(Hoy);
    }

    [Fact]
    public async Task Handle_SinIaConfigurada_ShouldFallarComoNoDisponible()
    {
        // 503 y no 500: el frontend distingue "no configurado" y cae a su parser local.
        await using var context = await EscenarioAsync();
        var espia = new InterpreteEspia { Disponible = false };

        var act = () => Handle(context, espia, "entré a las ocho");

        await act.Should().ThrowAsync<ServicioNoDisponibleException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validator_SinTexto_ShouldRechazar(string transcripcion)
    {
        var resultado = new InterpretarVozCommandValidator()
            .Validate(new InterpretarVozCommand(transcripcion));

        resultado.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validator_ConUnTextoEnorme_ShouldRechazar()
    {
        var largo = new string('a', InterpretarVozCommandValidator.MaxCaracteres + 1);

        var resultado = new InterpretarVozCommandValidator()
            .Validate(new InterpretarVozCommand(largo));

        resultado.IsValid.Should().BeFalse();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static readonly DateOnly Hoy = new(2026, 5, 14);

    private static Task<InterpretacionVozDto> Handle(
        ApplicationDbContext context, IInterpreteVoz interprete, string transcripcion) =>
        new InterpretarVozCommandHandler(context, interprete, new TestClock(Hoy))
            .Handle(new InterpretarVozCommand(transcripcion), CancellationToken.None);

    private static async Task<ApplicationDbContext> EscenarioAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);

        var cliente = new Cliente("Banco Nacional");
        context.Clientes.Add(cliente);
        context.Modalidades.Add(new Modalidad("Remoto"));
        context.Empleados.Add(new Empleado("Consultor"));
        context.LugaresTrabajo.Add(new LugarTrabajo("Bogota"));
        await context.SaveChangesAsync(CancellationToken.None);

        context.Proyectos.Add(new Proyecto(cliente.Id, "Core Bancario"));
        await context.SaveChangesAsync(CancellationToken.None);

        return context;
    }

    /// <summary>Captura lo que el handler le entrega, sin llamar a ningun modelo.</summary>
    private sealed class InterpreteEspia : IInterpreteVoz
    {
        public bool Disponible { get; init; } = true;
        public CatalogoVozDto? Catalogo { get; private set; }
        public DateOnly Hoy { get; private set; }

        public Task<InterpretacionVozDto> InterpretarAsync(
            string transcripcion, CatalogoVozDto catalogo, DateOnly hoy,
            CancellationToken cancellationToken = default)
        {
            Catalogo = catalogo;
            Hoy      = hoy;
            return Task.FromResult(InterpretacionVozDto.Vacia);
        }
    }

    private sealed class TestClock(DateOnly today) : IClock
    {
        public DateOnly Today => today;
        public DateTimeOffset UtcNow => today.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
    }
}
