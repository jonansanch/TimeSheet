using KPG.Timesheet.Application.Common.Services;
using KPG.Timesheet.Application.Features.RegistroHoras.Commands.CreateRegistroHoras;
using KPG.Timesheet.Application.Features.RegistroHoras.Commands.CreateRegistrosRango;
using KPG.Timesheet.Application.Features.RegistroHoras.Commands.UpdateDescripcionRegistroHoras;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Domain.Enums;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using RegistroHorasEntity = KPG.Timesheet.Domain.Entities.RegistroHoras;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.RegistroHoras;

/// <summary>
/// Prueba solo el cableado de los 3 validadores de descripcion (que llegan a
/// <see cref="ValidadorDescripcionService"/> y respetan <c>Bloquea</c>); la logica de
/// calidad en si ya la cubren <c>EvaluadorDescripcionTests</c> y
/// <c>ValidadorDescripcionServiceTests</c>.
/// </summary>
public class DescripcionValidatorsTests
{
    [Fact]
    public async Task CreateRegistroHoras_ConDescripcionBloqueada_Falla()
    {
        await using var context = await CrearContextoConTerminoBloqueanteAsync();
        var proyectoId = await SeedProyectoAsync(context);
        var validador = new ValidadorDescripcionService(context, new ParametrosSistemaService(context));

        var result = await new CreateRegistroHorasDescripcionValidator(validador)
            .ValidateAsync(CrearComandoRegistro(proyectoId, "varios"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateRegistroHorasCommand.Descripcion));
    }

    [Fact]
    public async Task CreateRegistroHoras_ConDescripcionDeCalidad_Pasa()
    {
        await using var context = await CrearContextoConTerminoBloqueanteAsync();
        var proyectoId = await SeedProyectoAsync(context);
        var validador = new ValidadorDescripcionService(context, new ParametrosSistemaService(context));

        var result = await new CreateRegistroHorasDescripcionValidator(validador)
            .ValidateAsync(CrearComandoRegistro(proyectoId, "Ajuste en modulo de evidencias para cancelaciones."));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task CreateRegistrosRango_ConDescripcionBloqueada_Falla()
    {
        await using var context = await CrearContextoConTerminoBloqueanteAsync();
        var proyectoId = await SeedProyectoAsync(context);
        var validador = new ValidadorDescripcionService(context, new ParametrosSistemaService(context));

        var comando = new CreateRegistrosRangoCommand(
            new DateOnly(2026, 5, 11), new DateOnly(2026, 5, 15),
            new TimeOnly(8, 0), new TimeOnly(13, 0), null, null, null, null,
            proyectoId, "Remoto", "Consultor", "varios", "Bogota");

        var result = await new CreateRegistrosRangoDescripcionValidator(validador).ValidateAsync(comando);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateDescripcion_ConDescripcionBloqueada_ResuelveElProyectoDelRegistroYFalla()
    {
        await using var context = await CrearContextoConTerminoBloqueanteAsync();
        var proyectoId = await SeedProyectoAsync(context);

        var registro = new RegistroHorasEntity(
            "user-1", new DateOnly(2026, 5, 14),
            new TimeOnly(8, 0), new TimeOnly(13, 0), null, null, null, null,
            proyectoId, "Banco Nacional", "Core Bancario",
            "Remoto", "Consultor", "Descripcion original valida y detallada.", "Bogota");
        context.RegistrosHoras.Add(registro);
        await context.SaveChangesAsync(CancellationToken.None);

        var validador = new ValidadorDescripcionService(context, new ParametrosSistemaService(context));
        var comando = new UpdateDescripcionRegistroHorasCommand(registro.Id, "varios");

        var result = await new UpdateDescripcionRegistroHorasDescripcionValidator(context, validador)
            .ValidateAsync(comando);

        result.IsValid.Should().BeFalse();
    }

    private static CreateRegistroHorasCommand CrearComandoRegistro(int proyectoId, string descripcion) =>
        new(new DateOnly(2026, 5, 14),
            new TimeOnly(8, 0), new TimeOnly(13, 0),
            null, null,
            null, null,
            proyectoId, "Remoto", "Consultor", descripcion, "Bogota");

    private static async Task<int> SeedProyectoAsync(ApplicationDbContext context)
    {
        var cliente = new Cliente("Banco Nacional");
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync(CancellationToken.None);
        var proyecto = new Proyecto(cliente.Id, "Core Bancario");
        context.Proyectos.Add(proyecto);
        await context.SaveChangesAsync(CancellationToken.None);
        return proyecto.Id;
    }

    private static async Task<ApplicationDbContext> CrearContextoConTerminoBloqueanteAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();

        context.TerminosDescripcion.Add(new TerminoDescripcion(
            "varios", TipoTermino.Palabra, ReglaTermino.ProhibidoSiempre,
            SeveridadTermino.Bloquear, "No es trazable, agrupa tareas distintas."));
        await context.SaveChangesAsync(CancellationToken.None);

        return context;
    }
}
