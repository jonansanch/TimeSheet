using KPG.Timesheet.Application.Common.Exceptions;
using KPG.Timesheet.Application.Features.Catalogos.Clientes.Commands.CreateCliente;
using KPG.Timesheet.Application.Features.Catalogos.Clientes.Commands.ToggleClienteActivo;
using KPG.Timesheet.Application.Features.Catalogos.Clientes.Commands.UpdateCliente;
using KPG.Timesheet.Application.Features.Catalogos.Clientes.Queries.GetCatalogoClientesConProyectos;
using KPG.Timesheet.Application.Features.Catalogos.Clientes.Queries.GetClientes;
using KPG.Timesheet.Application.Features.Catalogos.Proyectos.Commands.CreateProyecto;
using KPG.Timesheet.Application.Features.Catalogos.Proyectos.Commands.ToggleProyectoActivo;
using KPG.Timesheet.Application.Features.Catalogos.Proyectos.Queries.GetProyectosPorCliente;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.Catalogos;

public class ClientesCommandQueryTests
{
    // -------------------------------------------------------------------
    // CreateClienteCommandHandler
    // -------------------------------------------------------------------

    [Fact]
    public async Task CreateCliente_WithNombreValido_ReturnsDto()
    {
        await using var context = CreateContext();
        var handler = new CreateClienteCommandHandler(context);

        var dto = await handler.Handle(new CreateClienteCommand("KPG"), CancellationToken.None);

        dto.Id.Should().BeGreaterThan(0);
        dto.Nombre.Should().Be("KPG");
        dto.Activo.Should().BeTrue();
        context.Clientes.Should().ContainSingle(c => c.Nombre == "KPG" && c.Activo);
    }

    [Fact]
    public async Task CreateCliente_TrimsNombre()
    {
        await using var context = CreateContext();
        var handler = new CreateClienteCommandHandler(context);

        var dto = await handler.Handle(new CreateClienteCommand("  KPG  "), CancellationToken.None);

        dto.Nombre.Should().Be("KPG");
    }

    // -------------------------------------------------------------------
    // UpdateClienteCommandHandler
    // -------------------------------------------------------------------

    [Fact]
    public async Task UpdateCliente_WithNombreValido_ReturnsUpdatedDto()
    {
        await using var context = CreateContext();
        var cliente = await SeedClienteAsync(context, "KPG");
        var handler = new UpdateClienteCommandHandler(context);

        var dto = await handler.Handle(new UpdateClienteCommand(cliente.Id, "KPG Actualizado"), CancellationToken.None);

        dto.Nombre.Should().Be("KPG Actualizado");
        dto.Id.Should().Be(cliente.Id);
    }

    [Fact]
    public async Task UpdateCliente_WhenIdNotFound_ThrowsNotFoundException()
    {
        await using var context = CreateContext();
        var handler = new UpdateClienteCommandHandler(context);

        var act = () => handler.Handle(new UpdateClienteCommand(9999, "Nuevo"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // -------------------------------------------------------------------
    // ToggleClienteActivoCommandHandler
    // -------------------------------------------------------------------

    [Fact]
    public async Task ToggleClienteActivo_WhenActivo_Desactiva()
    {
        await using var context = CreateContext();
        var cliente = await SeedClienteAsync(context, "KPG");
        var handler = new ToggleClienteActivoCommandHandler(context);

        var dto = await handler.Handle(new ToggleClienteActivoCommand(cliente.Id), CancellationToken.None);

        dto.Activo.Should().BeFalse();
        context.Clientes.Single(c => c.Id == cliente.Id).Activo.Should().BeFalse();
    }

    [Fact]
    public async Task ToggleClienteActivo_WhenInactivo_Activa()
    {
        await using var context = CreateContext();
        var cliente = await SeedClienteAsync(context, "KPG", activo: false);
        var handler = new ToggleClienteActivoCommandHandler(context);

        var dto = await handler.Handle(new ToggleClienteActivoCommand(cliente.Id), CancellationToken.None);

        dto.Activo.Should().BeTrue();
    }

    [Fact]
    public async Task ToggleClienteActivo_WhenIdNotFound_ThrowsNotFoundException()
    {
        await using var context = CreateContext();
        var handler = new ToggleClienteActivoCommandHandler(context);

        var act = () => handler.Handle(new ToggleClienteActivoCommand(9999), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // -------------------------------------------------------------------
    // GetClientesQueryHandler
    // -------------------------------------------------------------------

    [Fact]
    public async Task GetClientes_SoloActivosFalse_ReturnsActivosEInactivos()
    {
        await using var context = CreateContext();
        await SeedClienteAsync(context, "Activo1");
        await SeedClienteAsync(context, "Inactivo1", activo: false);
        var handler = new GetClientesQueryHandler(context);

        var result = await handler.Handle(new GetClientesQuery(SoloActivos: false), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetClientes_SoloActivosTrue_ReturnsSoloActivos()
    {
        await using var context = CreateContext();
        await SeedClienteAsync(context, "Activo1");
        await SeedClienteAsync(context, "Inactivo1", activo: false);
        var handler = new GetClientesQueryHandler(context);

        var result = await handler.Handle(new GetClientesQuery(SoloActivos: true), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Nombre.Should().Be("Activo1");
    }

    // -------------------------------------------------------------------
    // CreateProyectoCommandHandler
    // -------------------------------------------------------------------

    [Fact]
    public async Task CreateProyecto_WithClienteValido_ReturnsDto()
    {
        await using var context = CreateContext();
        var cliente = await SeedClienteAsync(context, "KPG");
        var handler = new CreateProyectoCommandHandler(context);

        var dto = await handler.Handle(new CreateProyectoCommand(cliente.Id, "Timesheet"), CancellationToken.None);

        dto.Id.Should().BeGreaterThan(0);
        dto.Nombre.Should().Be("Timesheet");
        dto.ClienteId.Should().Be(cliente.Id);
        dto.Activo.Should().BeTrue();
    }

    [Fact]
    public async Task CreateProyecto_WhenClienteNotFound_ThrowsNotFoundException()
    {
        await using var context = CreateContext();
        var handler = new CreateProyectoCommandHandler(context);

        var act = () => handler.Handle(new CreateProyectoCommand(9999, "Timesheet"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // -------------------------------------------------------------------
    // ToggleProyectoActivoCommandHandler
    // -------------------------------------------------------------------

    [Fact]
    public async Task ToggleProyectoActivo_WhenActivo_Desactiva()
    {
        await using var context = CreateContext();
        var cliente = await SeedClienteAsync(context, "KPG");
        var proyecto = await SeedProyectoAsync(context, cliente.Id, "Timesheet");
        var handler = new ToggleProyectoActivoCommandHandler(context);

        var dto = await handler.Handle(new ToggleProyectoActivoCommand(proyecto.Id), CancellationToken.None);

        dto.Activo.Should().BeFalse();
    }

    [Fact]
    public async Task ToggleProyectoActivo_WhenInactivo_Activa()
    {
        await using var context = CreateContext();
        var cliente = await SeedClienteAsync(context, "KPG");
        var proyecto = await SeedProyectoAsync(context, cliente.Id, "Timesheet", activo: false);
        var handler = new ToggleProyectoActivoCommandHandler(context);

        var dto = await handler.Handle(new ToggleProyectoActivoCommand(proyecto.Id), CancellationToken.None);

        dto.Activo.Should().BeTrue();
    }

    // -------------------------------------------------------------------
    // GetProyectosPorClienteQueryHandler
    // -------------------------------------------------------------------

    [Fact]
    public async Task GetProyectosPorCliente_ReturnsSoloProyectosDelCliente()
    {
        await using var context = CreateContext();
        var cliente1 = await SeedClienteAsync(context, "KPG");
        var cliente2 = await SeedClienteAsync(context, "Otro");
        await SeedProyectoAsync(context, cliente1.Id, "Timesheet");
        await SeedProyectoAsync(context, cliente2.Id, "Portal");
        var handler = new GetProyectosPorClienteQueryHandler(context);

        var result = await handler.Handle(new GetProyectosPorClienteQuery(cliente1.Id), CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Nombre.Should().Be("Timesheet");
        result[0].ClienteId.Should().Be(cliente1.Id);
    }

    // -------------------------------------------------------------------
    // GetCatalogoClientesConProyectosQueryHandler
    // -------------------------------------------------------------------

    [Fact]
    public async Task GetCatalogo_ReturnsSoloActivosConProyectosActivos()
    {
        await using var context = CreateContext();
        var clienteActivo = await SeedClienteAsync(context, "KPG");
        var clienteInactivo = await SeedClienteAsync(context, "Inactivo", activo: false);
        await SeedProyectoAsync(context, clienteActivo.Id, "Timesheet");
        await SeedProyectoAsync(context, clienteActivo.Id, "ProyInactivo", activo: false);
        await SeedProyectoAsync(context, clienteInactivo.Id, "Portal");
        var handler = new GetCatalogoClientesConProyectosQueryHandler(context);

        var result = await handler.Handle(new GetCatalogoClientesConProyectosQuery(), CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Nombre.Should().Be("KPG");
        result[0].ProyectosActivos.Should().ContainSingle().Which.Should().Be("Timesheet");
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

    private static async Task<Cliente> SeedClienteAsync(ApplicationDbContext context, string nombre, bool activo = true)
    {
        var cliente = new Cliente(nombre);
        if (!activo) cliente.Desactivar();
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync(CancellationToken.None);
        return cliente;
    }

    private static async Task<Proyecto> SeedProyectoAsync(
        ApplicationDbContext context, int clienteId, string nombre, bool activo = true)
    {
        var proyecto = new Proyecto(clienteId, nombre);
        if (!activo) proyecto.Desactivar();
        context.Proyectos.Add(proyecto);
        await context.SaveChangesAsync(CancellationToken.None);
        return proyecto;
    }
}
