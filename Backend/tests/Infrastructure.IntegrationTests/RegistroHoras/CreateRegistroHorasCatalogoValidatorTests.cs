using KPG.Timesheet.Application.Features.RegistroHoras.Commands.CreateRegistroHoras;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.RegistroHoras;

/// <summary>
/// Tras la migracion a ProyectoId, la pertenencia proyecto→cliente la garantiza la FK.
/// Lo que sigue validandose aqui es que ni el proyecto ni su cliente esten dados de baja.
/// </summary>
public class CreateRegistroHorasCatalogoValidatorTests
{
    [Fact]
    public async Task Validate_WhenProyectoIsActive_ShouldPass()
    {
        await using var context = await CreateContextConCatalogoAsync();
        var proyectoId = await IdProyectoAsync(context, "Core Bancario");

        var result = await Validar(context, proyectoId);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WhenProyectoDoesNotExist_ShouldFail()
    {
        await using var context = await CreateContextConCatalogoAsync();

        var result = await Validar(context, 9999);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateRegistroHorasCommand.ProyectoId));
    }

    [Fact]
    public async Task Validate_WhenProyectoIsInactive_ShouldFail()
    {
        await using var context = await CreateContextConCatalogoAsync();
        var proyecto = await context.Proyectos.FirstAsync(p => p.Nombre == "Core Bancario");
        proyecto.Desactivar();
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await Validar(context, proyecto.Id);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_WhenClienteIsInactive_ShouldFail()
    {
        // El proyecto sigue activo, pero su cliente no: no deben imputarse horas ahi.
        await using var context = await CreateContextConCatalogoAsync();
        var cliente = await context.Clientes.FirstAsync(c => c.Nombre == "Banco Nacional");
        cliente.Desactivar();
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await Validar(context, await IdProyectoAsync(context, "Core Bancario"));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_WhenProyectoIdIsZero_ShouldNotRunCatalogRules()
    {
        // El id vacio lo reporta CreateRegistroHorasCommandValidator; aqui no se duplica.
        await using var context = await CreateContextConCatalogoAsync();

        var result = await Validar(context, 0);

        result.IsValid.Should().BeTrue();
    }

    private static Task<FluentValidation.Results.ValidationResult> Validar(
        ApplicationDbContext context, int proyectoId) =>
        new CreateRegistroHorasCatalogoValidator(context).ValidateAsync(Command(proyectoId));

    private static Task<int> IdProyectoAsync(ApplicationDbContext context, string nombre) =>
        context.Proyectos.Where(p => p.Nombre == nombre).Select(p => p.Id).FirstAsync();

    private static CreateRegistroHorasCommand Command(int proyectoId) =>
        new(new DateOnly(2026, 5, 14),
            new TimeOnly(8, 0), new TimeOnly(13, 0),
            null, null,
            null, null,
            proyectoId, "Remoto", "Consultor", "Desarrollo", "Bogota");

    private static async Task<ApplicationDbContext> CreateContextConCatalogoAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);

        var banco = new Cliente("Banco Nacional");
        var salud = new Cliente("Ministerio de Salud");
        context.Clientes.AddRange(banco, salud);
        await context.SaveChangesAsync(CancellationToken.None);

        context.Proyectos.AddRange(
            new Proyecto(banco.Id, "Core Bancario"),
            new Proyecto(salud.Id, "Sistema RIPS"));
        await context.SaveChangesAsync(CancellationToken.None);

        return context;
    }
}
