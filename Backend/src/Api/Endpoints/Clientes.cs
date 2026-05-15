using KPG.Timesheet.Application.Features.Catalogos.Clientes.Commands.CreateCliente;
using KPG.Timesheet.Application.Features.Catalogos.Clientes.Commands.ToggleClienteActivo;
using KPG.Timesheet.Application.Features.Catalogos.Clientes.Commands.UpdateCliente;
using KPG.Timesheet.Application.Features.Catalogos.Clientes.Queries.GetCatalogoClientesConProyectos;
using KPG.Timesheet.Application.Features.Catalogos.Clientes.Queries.GetClientes;
using KPG.Timesheet.Application.Features.Catalogos.Proyectos.Commands.CreateProyecto;
using KPG.Timesheet.Application.Features.Catalogos.Proyectos.Commands.ToggleProyectoActivo;
using KPG.Timesheet.Application.Features.Catalogos.Proyectos.Commands.UpdateProyecto;
using KPG.Timesheet.Application.Features.Catalogos.Proyectos.Queries.GetProyectosPorCliente;
using KPG.Timesheet.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPG.Timesheet.Api.Endpoints;

public class Clientes : IEndpointGroup
{
    public static string RoutePrefix => "/api/clientes";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        var adminOnly = new AuthorizeAttribute { Roles = Roles.Admin };
        var anyAuth = new AuthorizeAttribute
        {
            Roles = $"{Roles.Empleado},{Roles.Supervisor},{Roles.Gerente},{Roles.Admin}"
        };

        groupBuilder.MapGet("", GetAll).RequireAuthorization(adminOnly);
        groupBuilder.MapGet("catalogo", GetCatalogo).RequireAuthorization(anyAuth);
        groupBuilder.MapPost("", Create).RequireAuthorization(adminOnly);
        groupBuilder.MapPut("{id:int}", Update).RequireAuthorization(adminOnly);
        groupBuilder.MapPut("{id:int}/toggle", Toggle).RequireAuthorization(adminOnly);
        groupBuilder.MapGet("{id:int}/proyectos", GetProyectos).RequireAuthorization(adminOnly);
        groupBuilder.MapPost("{id:int}/proyectos", CreateProyecto).RequireAuthorization(adminOnly);
    }

    [EndpointSummary("Listar clientes")]
    [EndpointDescription("Retorna todos los clientes del catalogo (activos e inactivos) para administracion.")]
    [ProducesResponseType<List<ClienteDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<IResult> GetAll(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetClientesQuery(SoloActivos: false), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Catalogo de clientes con proyectos activos")]
    [EndpointDescription("Retorna solo clientes activos con sus proyectos activos para el formulario de registro.")]
    [ProducesResponseType<List<ClienteConProyectosDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public static async Task<IResult> GetCatalogo(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCatalogoClientesConProyectosQuery(), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Crear cliente")]
    [EndpointDescription("Crea un cliente activo en el catalogo.")]
    [ProducesResponseType<ClienteDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<IResult> Create(
        [FromBody] CreateClienteCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return Results.Created($"/api/clientes/{result.Id}", result);
    }

    [EndpointSummary("Actualizar nombre de cliente")]
    [EndpointDescription("Actualiza el nombre de un cliente existente.")]
    [ProducesResponseType<ClienteDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> Update(
        int id,
        [FromBody] UpdateClienteRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateClienteCommand(id, request.Nombre), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Activar o desactivar cliente")]
    [EndpointDescription("Cambia el estado del cliente entre activo e inactivo.")]
    [ProducesResponseType<ClienteDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> Toggle(
        int id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ToggleClienteActivoCommand(id), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Listar proyectos de un cliente")]
    [EndpointDescription("Retorna todos los proyectos (activos e inactivos) de un cliente para administracion.")]
    [ProducesResponseType<List<ProyectoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<IResult> GetProyectos(
        int id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProyectosPorClienteQuery(id, SoloActivos: false), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Crear proyecto bajo un cliente")]
    [EndpointDescription("Crea un proyecto activo vinculado al cliente indicado.")]
    [ProducesResponseType<ProyectoDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> CreateProyecto(
        int id,
        [FromBody] CreateProyectoRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateProyectoCommand(id, request.Nombre), cancellationToken);
        return Results.Created($"/api/proyectos/{result.Id}", result);
    }
}

public class Proyectos : IEndpointGroup
{
    public static string RoutePrefix => "/api/proyectos";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        var adminOnly = new AuthorizeAttribute { Roles = Roles.Admin };

        groupBuilder.MapPut("{id:int}", Update).RequireAuthorization(adminOnly);
        groupBuilder.MapPut("{id:int}/toggle", Toggle).RequireAuthorization(adminOnly);
    }

    [EndpointSummary("Actualizar nombre de proyecto")]
    [EndpointDescription("Actualiza el nombre de un proyecto existente.")]
    [ProducesResponseType<ProyectoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> Update(
        int id,
        [FromBody] UpdateProyectoRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateProyectoCommand(id, request.Nombre), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Activar o desactivar proyecto")]
    [EndpointDescription("Cambia el estado del proyecto entre activo e inactivo.")]
    [ProducesResponseType<ProyectoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> Toggle(
        int id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ToggleProyectoActivoCommand(id), cancellationToken);
        return Results.Ok(result);
    }
}

public record UpdateClienteRequest(string Nombre);
public record CreateProyectoRequest(string Nombre);
public record UpdateProyectoRequest(string Nombre);
