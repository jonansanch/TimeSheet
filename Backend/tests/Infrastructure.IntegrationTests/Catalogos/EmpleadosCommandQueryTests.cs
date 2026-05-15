using KPG.Timesheet.Application.Common.Exceptions;
using KPG.Timesheet.Application.Features.Catalogos.Empleados.Commands.CreateEmpleado;
using KPG.Timesheet.Application.Features.Catalogos.Empleados.Commands.ToggleEmpleadoActivo;
using KPG.Timesheet.Application.Features.Catalogos.Empleados.Commands.UpdateEmpleado;
using KPG.Timesheet.Application.Features.Catalogos.Empleados.Queries.GetEmpleados;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.Catalogos;

public class EmpleadosCommandQueryTests
{
    // -------------------------------------------------------------------
    // CreateEmpleadoCommandHandler
    // -------------------------------------------------------------------

    [Fact]
    public async Task CreateEmpleado_WithNombreValido_ReturnsDto()
    {
        await using var context = CreateContext();
        var handler = new CreateEmpleadoCommandHandler(context);

        var dto = await handler.Handle(new CreateEmpleadoCommand("Consultor"), CancellationToken.None);

        dto.Id.Should().BeGreaterThan(0);
        dto.Nombre.Should().Be("Consultor");
        dto.Activo.Should().BeTrue();
        context.Empleados.Should().ContainSingle(e => e.Nombre == "Consultor" && e.Activo);
    }

    [Fact]
    public async Task CreateEmpleado_TrimsNombre()
    {
        await using var context = CreateContext();
        var handler = new CreateEmpleadoCommandHandler(context);

        var dto = await handler.Handle(new CreateEmpleadoCommand("  Analista  "), CancellationToken.None);

        dto.Nombre.Should().Be("Analista");
    }

    // -------------------------------------------------------------------
    // UpdateEmpleadoCommandHandler
    // -------------------------------------------------------------------

    [Fact]
    public async Task UpdateEmpleado_WithNombreValido_ReturnsUpdatedDto()
    {
        await using var context = CreateContext();
        var empleado = await SeedEmpleadoAsync(context, "Consultor");
        var handler = new UpdateEmpleadoCommandHandler(context);

        var dto = await handler.Handle(new UpdateEmpleadoCommand(empleado.Id, "Analista Senior"), CancellationToken.None);

        dto.Nombre.Should().Be("Analista Senior");
        dto.Id.Should().Be(empleado.Id);
    }

    [Fact]
    public async Task UpdateEmpleado_WhenIdNotFound_ThrowsNotFoundException()
    {
        await using var context = CreateContext();
        var handler = new UpdateEmpleadoCommandHandler(context);

        var act = () => handler.Handle(new UpdateEmpleadoCommand(9999, "Nuevo"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // -------------------------------------------------------------------
    // ToggleEmpleadoActivoCommandHandler
    // -------------------------------------------------------------------

    [Fact]
    public async Task ToggleEmpleadoActivo_WhenActivo_Desactiva()
    {
        await using var context = CreateContext();
        var empleado = await SeedEmpleadoAsync(context, "Consultor");
        var handler = new ToggleEmpleadoActivoCommandHandler(context);

        var dto = await handler.Handle(new ToggleEmpleadoActivoCommand(empleado.Id), CancellationToken.None);

        dto.Activo.Should().BeFalse();
        context.Empleados.Single(e => e.Id == empleado.Id).Activo.Should().BeFalse();
    }

    [Fact]
    public async Task ToggleEmpleadoActivo_WhenInactivo_Activa()
    {
        await using var context = CreateContext();
        var empleado = await SeedEmpleadoAsync(context, "Consultor", activo: false);
        var handler = new ToggleEmpleadoActivoCommandHandler(context);

        var dto = await handler.Handle(new ToggleEmpleadoActivoCommand(empleado.Id), CancellationToken.None);

        dto.Activo.Should().BeTrue();
        context.Empleados.Single(e => e.Id == empleado.Id).Activo.Should().BeTrue();
    }

    [Fact]
    public async Task ToggleEmpleadoActivo_WhenIdNotFound_ThrowsNotFoundException()
    {
        await using var context = CreateContext();
        var handler = new ToggleEmpleadoActivoCommandHandler(context);

        var act = () => handler.Handle(new ToggleEmpleadoActivoCommand(9999), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // -------------------------------------------------------------------
    // GetEmpleadosQueryHandler
    // -------------------------------------------------------------------

    [Fact]
    public async Task GetEmpleados_SoloActivosFalse_ReturnsActivosEInactivos()
    {
        await using var context = CreateContext();
        await SeedEmpleadoAsync(context, "Activo1");
        await SeedEmpleadoAsync(context, "Inactivo1", activo: false);
        var handler = new GetEmpleadosQueryHandler(context);

        var result = await handler.Handle(new GetEmpleadosQuery(SoloActivos: false), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetEmpleados_SoloActivosTrue_ReturnsSoloActivos()
    {
        await using var context = CreateContext();
        await SeedEmpleadoAsync(context, "Activo1");
        await SeedEmpleadoAsync(context, "Inactivo1", activo: false);
        var handler = new GetEmpleadosQueryHandler(context);

        var result = await handler.Handle(new GetEmpleadosQuery(SoloActivos: true), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Nombre.Should().Be("Activo1");
    }

    [Fact]
    public async Task GetEmpleados_EmpleadoDesactivado_NoApareceEnSoloActivos()
    {
        await using var context = CreateContext();
        var empleado = await SeedEmpleadoAsync(context, "Empleado");
        var toggleHandler = new ToggleEmpleadoActivoCommandHandler(context);
        await toggleHandler.Handle(new ToggleEmpleadoActivoCommand(empleado.Id), CancellationToken.None);
        var queryHandler = new GetEmpleadosQueryHandler(context);

        var result = await queryHandler.Handle(new GetEmpleadosQuery(SoloActivos: true), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEmpleados_OrdersByNombre()
    {
        await using var context = CreateContext();
        await SeedEmpleadoAsync(context, "Zorro");
        await SeedEmpleadoAsync(context, "Analista");
        var handler = new GetEmpleadosQueryHandler(context);

        var result = await handler.Handle(new GetEmpleadosQuery(), CancellationToken.None);

        result[0].Nombre.Should().Be("Analista");
        result[1].Nombre.Should().Be("Zorro");
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

    private static async Task<Empleado> SeedEmpleadoAsync(ApplicationDbContext context, string nombre, bool activo = true)
    {
        var empleado = new Empleado(nombre);
        if (!activo) empleado.Desactivar();
        context.Empleados.Add(empleado);
        await context.SaveChangesAsync(CancellationToken.None);
        return empleado;
    }
}
