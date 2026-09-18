using KPG.Timesheet.Application.Features.RegistroHoras.Commands.CreateRegistroHoras;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.RegistroHoras;

public class CreateRegistroHorasCatalogoValidatorTests
{
    [Fact]
    public async Task Validate_WhenProyectoBelongsToCliente_ShouldPass()
    {
        await using var context = await CreateContextConCatalogoAsync();
        var validator = new CreateRegistroHorasCatalogoValidator(context);

        var result = await validator.ValidateAsync(Command("Banco Nacional", "Core Bancario"));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WhenProyectoBelongsToAnotherCliente_ShouldFail()
    {
        // El hueco que cierra esta regla: la UI cascadea, pero un POST directo
        // podia mezclar el cliente de uno con el proyecto de otro.
        await using var context = await CreateContextConCatalogoAsync();
        var validator = new CreateRegistroHorasCatalogoValidator(context);

        var result = await validator.ValidateAsync(Command("Banco Nacional", "Sistema RIPS"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateRegistroHorasCommand.Proyecto));
    }

    [Fact]
    public async Task Validate_WhenClienteDoesNotExist_ShouldFail()
    {
        await using var context = await CreateContextConCatalogoAsync();
        var validator = new CreateRegistroHorasCatalogoValidator(context);

        var result = await validator.ValidateAsync(Command("Cliente Inventado", "Core Bancario"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateRegistroHorasCommand.Cliente));
    }

    [Fact]
    public async Task Validate_WhenClienteDoesNotExist_ShouldNotAlsoReportProyecto()
    {
        // Un solo error por causa: reportar tambien el proyecto confunde al usuario.
        await using var context = await CreateContextConCatalogoAsync();
        var validator = new CreateRegistroHorasCatalogoValidator(context);

        var result = await validator.ValidateAsync(Command("Cliente Inventado", "Core Bancario"));

        result.Errors.Should().ContainSingle();
    }

    [Fact]
    public async Task Validate_WhenProyectoIsInactive_ShouldFail()
    {
        await using var context = await CreateContextConCatalogoAsync();
        var proyecto = await context.Proyectos.FirstAsync(p => p.Nombre == "Core Bancario");
        proyecto.Desactivar();
        await context.SaveChangesAsync(CancellationToken.None);

        var validator = new CreateRegistroHorasCatalogoValidator(context);
        var result = await validator.ValidateAsync(Command("Banco Nacional", "Core Bancario"));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_WhenClienteIsInactive_ShouldFail()
    {
        await using var context = await CreateContextConCatalogoAsync();
        var cliente = await context.Clientes.FirstAsync(c => c.Nombre == "Banco Nacional");
        cliente.Desactivar();
        await context.SaveChangesAsync(CancellationToken.None);

        var validator = new CreateRegistroHorasCatalogoValidator(context);
        var result = await validator.ValidateAsync(Command("Banco Nacional", "Core Bancario"));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_ShouldTrimSurroundingWhitespace()
    {
        await using var context = await CreateContextConCatalogoAsync();
        var validator = new CreateRegistroHorasCatalogoValidator(context);

        var result = await validator.ValidateAsync(Command("  Banco Nacional  ", "  Core Bancario  "));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WhenClienteIsEmpty_ShouldNotRunCatalogRules()
    {
        // El vacio lo reporta CreateRegistroHorasCommandValidator; aqui no se duplica.
        await using var context = await CreateContextConCatalogoAsync();
        var validator = new CreateRegistroHorasCatalogoValidator(context);

        var result = await validator.ValidateAsync(Command("", "Core Bancario"));

        result.IsValid.Should().BeTrue();
    }

    private static CreateRegistroHorasCommand Command(string cliente, string proyecto) =>
        new(new DateOnly(2026, 5, 14),
            new TimeOnly(8, 0), new TimeOnly(13, 0),
            null, null,
            null, null,
            cliente, proyecto, "Remoto", "Consultor", "Desarrollo", "Bogota");

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
