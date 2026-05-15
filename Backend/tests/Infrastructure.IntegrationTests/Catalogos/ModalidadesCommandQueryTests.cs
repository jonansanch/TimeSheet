using KPG.Timesheet.Application.Common.Exceptions;
using KPG.Timesheet.Application.Features.Catalogos.Modalidades.Commands.CreateModalidad;
using KPG.Timesheet.Application.Features.Catalogos.Modalidades.Commands.ToggleModalidadActiva;
using KPG.Timesheet.Application.Features.Catalogos.Modalidades.Commands.UpdateModalidad;
using KPG.Timesheet.Application.Features.Catalogos.Modalidades.Queries.GetModalidades;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.Catalogos;

public class ModalidadesCommandQueryTests
{
    // -------------------------------------------------------------------
    // CreateModalidadCommandHandler
    // -------------------------------------------------------------------

    [Fact]
    public async Task CreateModalidad_WithNombreValido_ReturnsDto()
    {
        await using var context = CreateContext();
        var handler = new CreateModalidadCommandHandler(context);

        var dto = await handler.Handle(new CreateModalidadCommand("Presencial"), CancellationToken.None);

        dto.Id.Should().BeGreaterThan(0);
        dto.Nombre.Should().Be("Presencial");
        dto.Activo.Should().BeTrue();
        context.Modalidades.Should().ContainSingle(m => m.Nombre == "Presencial" && m.Activo);
    }

    [Fact]
    public async Task CreateModalidad_TrimsNombre()
    {
        await using var context = CreateContext();
        var handler = new CreateModalidadCommandHandler(context);

        var dto = await handler.Handle(new CreateModalidadCommand("  Remoto  "), CancellationToken.None);

        dto.Nombre.Should().Be("Remoto");
    }

    // -------------------------------------------------------------------
    // UpdateModalidadCommandHandler
    // -------------------------------------------------------------------

    [Fact]
    public async Task UpdateModalidad_WithNombreValido_ReturnsUpdatedDto()
    {
        await using var context = CreateContext();
        var modalidad = await SeedModalidadAsync(context, "Presencial");
        var handler = new UpdateModalidadCommandHandler(context);

        var dto = await handler.Handle(new UpdateModalidadCommand(modalidad.Id, "Hibrido"), CancellationToken.None);

        dto.Nombre.Should().Be("Hibrido");
        context.Modalidades.Single(m => m.Id == modalidad.Id).Nombre.Should().Be("Hibrido");
    }

    [Fact]
    public async Task UpdateModalidad_WithIdInexistente_ThrowsNotFoundException()
    {
        await using var context = CreateContext();
        var handler = new UpdateModalidadCommandHandler(context);

        var act = () => handler.Handle(new UpdateModalidadCommand(999, "Hibrido"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // -------------------------------------------------------------------
    // ToggleModalidadActivaCommandHandler
    // -------------------------------------------------------------------

    [Fact]
    public async Task ToggleModalidad_WhenActiva_Desactiva()
    {
        await using var context = CreateContext();
        var modalidad = await SeedModalidadAsync(context, "Presencial");
        var handler = new ToggleModalidadActivaCommandHandler(context);

        var dto = await handler.Handle(new ToggleModalidadActivaCommand(modalidad.Id), CancellationToken.None);

        dto.Activo.Should().BeFalse();
        context.Modalidades.Single(m => m.Id == modalidad.Id).Activo.Should().BeFalse();
    }

    [Fact]
    public async Task ToggleModalidad_WhenInactiva_Activa()
    {
        await using var context = CreateContext();
        var modalidad = await SeedModalidadAsync(context, "Presencial", activo: false);
        var handler = new ToggleModalidadActivaCommandHandler(context);

        var dto = await handler.Handle(new ToggleModalidadActivaCommand(modalidad.Id), CancellationToken.None);

        dto.Activo.Should().BeTrue();
    }

    // -------------------------------------------------------------------
    // GetModalidadesQueryHandler
    // -------------------------------------------------------------------

    [Fact]
    public async Task GetModalidades_SoloActivasFalse_ReturnsAll()
    {
        await using var context = CreateContext();
        await SeedModalidadAsync(context, "Presencial");
        await SeedModalidadAsync(context, "Remoto", activo: false);
        var handler = new GetModalidadesQueryHandler(context);

        var result = await handler.Handle(new GetModalidadesQuery(SoloActivas: false), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetModalidades_SoloActivasTrue_ReturnsOnlyActivas()
    {
        await using var context = CreateContext();
        await SeedModalidadAsync(context, "Presencial");
        await SeedModalidadAsync(context, "Remoto", activo: false);
        var handler = new GetModalidadesQueryHandler(context);

        var result = await handler.Handle(new GetModalidadesQuery(SoloActivas: true), CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Nombre.Should().Be("Presencial");
    }

    // -------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static async Task<Modalidad> SeedModalidadAsync(ApplicationDbContext context, string nombre, bool activo = true)
    {
        var modalidad = new Modalidad(nombre);
        if (!activo) modalidad.Desactivar();
        context.Modalidades.Add(modalidad);
        await context.SaveChangesAsync(CancellationToken.None);
        return modalidad;
    }
}
