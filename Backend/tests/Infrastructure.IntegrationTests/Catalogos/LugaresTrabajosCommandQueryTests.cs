using KPG.Timesheet.Application.Common.Exceptions;
using KPG.Timesheet.Application.Features.Catalogos.LugaresTrabajo.Commands.CreateLugarTrabajo;
using KPG.Timesheet.Application.Features.Catalogos.LugaresTrabajo.Commands.ToggleLugarTrabajoActivo;
using KPG.Timesheet.Application.Features.Catalogos.LugaresTrabajo.Commands.UpdateLugarTrabajo;
using KPG.Timesheet.Application.Features.Catalogos.LugaresTrabajo.Queries.GetLugaresTrabajo;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.Catalogos;

public class LugaresTrabajosCommandQueryTests
{
    // -------------------------------------------------------------------
    // CreateLugarTrabajoCommandHandler
    // -------------------------------------------------------------------

    [Fact]
    public async Task CreateLugarTrabajo_WithNombreValido_ReturnsDto()
    {
        await using var context = CreateContext();
        var handler = new CreateLugarTrabajoCommandHandler(context);

        var dto = await handler.Handle(new CreateLugarTrabajoCommand("Presencial Oficina"), CancellationToken.None);

        dto.Id.Should().BeGreaterThan(0);
        dto.Nombre.Should().Be("Presencial Oficina");
        dto.Activo.Should().BeTrue();
        context.LugaresTrabajo.Should().ContainSingle(l => l.Nombre == "Presencial Oficina" && l.Activo);
    }

    [Fact]
    public async Task CreateLugarTrabajo_TrimsNombre()
    {
        await using var context = CreateContext();
        var handler = new CreateLugarTrabajoCommandHandler(context);

        var dto = await handler.Handle(new CreateLugarTrabajoCommand("  Remoto  "), CancellationToken.None);

        dto.Nombre.Should().Be("Remoto");
    }

    // -------------------------------------------------------------------
    // UpdateLugarTrabajoCommandHandler
    // -------------------------------------------------------------------

    [Fact]
    public async Task UpdateLugarTrabajo_WithNombreValido_ReturnsUpdatedDto()
    {
        await using var context = CreateContext();
        var lugar = await SeedLugarAsync(context, "Presencial Oficina");
        var handler = new UpdateLugarTrabajoCommandHandler(context);

        var dto = await handler.Handle(new UpdateLugarTrabajoCommand(lugar.Id, "Presencial Cliente"), CancellationToken.None);

        dto.Nombre.Should().Be("Presencial Cliente");
        context.LugaresTrabajo.Single(l => l.Id == lugar.Id).Nombre.Should().Be("Presencial Cliente");
    }

    [Fact]
    public async Task UpdateLugarTrabajo_WithIdInexistente_ThrowsNotFoundException()
    {
        await using var context = CreateContext();
        var handler = new UpdateLugarTrabajoCommandHandler(context);

        var act = () => handler.Handle(new UpdateLugarTrabajoCommand(999, "Remoto"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // -------------------------------------------------------------------
    // ToggleLugarTrabajoActivoCommandHandler
    // -------------------------------------------------------------------

    [Fact]
    public async Task ToggleLugarTrabajo_WhenActivo_Desactiva()
    {
        await using var context = CreateContext();
        var lugar = await SeedLugarAsync(context, "Presencial Oficina");
        var handler = new ToggleLugarTrabajoActivoCommandHandler(context);

        var dto = await handler.Handle(new ToggleLugarTrabajoActivoCommand(lugar.Id), CancellationToken.None);

        dto.Activo.Should().BeFalse();
        context.LugaresTrabajo.Single(l => l.Id == lugar.Id).Activo.Should().BeFalse();
    }

    [Fact]
    public async Task ToggleLugarTrabajo_WhenInactivo_Activa()
    {
        await using var context = CreateContext();
        var lugar = await SeedLugarAsync(context, "Presencial Oficina", activo: false);
        var handler = new ToggleLugarTrabajoActivoCommandHandler(context);

        var dto = await handler.Handle(new ToggleLugarTrabajoActivoCommand(lugar.Id), CancellationToken.None);

        dto.Activo.Should().BeTrue();
    }

    // -------------------------------------------------------------------
    // GetLugaresTrabajosQueryHandler
    // -------------------------------------------------------------------

    [Fact]
    public async Task GetLugaresTrabajo_SoloActivosFalse_ReturnsAll()
    {
        await using var context = CreateContext();
        await SeedLugarAsync(context, "Presencial Oficina");
        await SeedLugarAsync(context, "Remoto", activo: false);
        var handler = new GetLugaresTrabajosQueryHandler(context);

        var result = await handler.Handle(new GetLugaresTrabajosQuery(SoloActivos: false), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetLugaresTrabajo_SoloActivosTrue_ReturnsOnlyActivos()
    {
        await using var context = CreateContext();
        await SeedLugarAsync(context, "Presencial Oficina");
        await SeedLugarAsync(context, "Remoto", activo: false);
        var handler = new GetLugaresTrabajosQueryHandler(context);

        var result = await handler.Handle(new GetLugaresTrabajosQuery(SoloActivos: true), CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Nombre.Should().Be("Presencial Oficina");
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

    private static async Task<LugarTrabajo> SeedLugarAsync(ApplicationDbContext context, string nombre, bool activo = true)
    {
        var lugar = new LugarTrabajo(nombre);
        if (!activo) lugar.Desactivar();
        context.LugaresTrabajo.Add(lugar);
        await context.SaveChangesAsync(CancellationToken.None);
        return lugar;
    }
}
