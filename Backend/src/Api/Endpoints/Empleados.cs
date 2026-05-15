using KPG.Timesheet.Application.Features.Catalogos.Empleados.Commands.CreateEmpleado;
using KPG.Timesheet.Application.Features.Catalogos.Empleados.Commands.ToggleEmpleadoActivo;
using KPG.Timesheet.Application.Features.Catalogos.Empleados.Commands.UpdateEmpleado;
using KPG.Timesheet.Application.Features.Catalogos.Empleados.Queries.GetEmpleados;
using KPG.Timesheet.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPG.Timesheet.Api.Endpoints;

public class Empleados : IEndpointGroup
{
    public static string RoutePrefix => "/api/empleados";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        var adminOnly = new AuthorizeAttribute { Roles = Roles.Admin };
        var anyAuth = new AuthorizeAttribute
        {
            Roles = $"{Roles.Empleado},{Roles.Supervisor},{Roles.Gerente},{Roles.Admin}"
        };

        groupBuilder.MapGet("", GetAll).RequireAuthorization(adminOnly);
        groupBuilder.MapGet("activos", GetActivos).RequireAuthorization(anyAuth);
        groupBuilder.MapPost("", Create).RequireAuthorization(adminOnly);
        groupBuilder.MapPut("{id:int}", Update).RequireAuthorization(adminOnly);
        groupBuilder.MapPut("{id:int}/toggle", Toggle).RequireAuthorization(adminOnly);
    }

    [EndpointSummary("Listar empleados")]
    [EndpointDescription("Retorna todos los empleados del catalogo (activos e inactivos) para administracion.")]
    [ProducesResponseType<List<EmpleadoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<IResult> GetAll(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetEmpleadosQuery(SoloActivos: false), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Listar empleados activos")]
    [EndpointDescription("Retorna solo empleados activos para poblar el selector de recurso en el formulario de registro.")]
    [ProducesResponseType<List<EmpleadoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public static async Task<IResult> GetActivos(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetEmpleadosQuery(SoloActivos: true), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Crear empleado")]
    [EndpointDescription("Crea un empleado activo en el catalogo.")]
    [ProducesResponseType<EmpleadoDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<IResult> Create(
        [FromBody] CreateEmpleadoCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return Results.Created($"/api/empleados/{result.Id}", result);
    }

    [EndpointSummary("Actualizar nombre de empleado")]
    [EndpointDescription("Actualiza el nombre de un empleado existente.")]
    [ProducesResponseType<EmpleadoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> Update(
        int id,
        [FromBody] UpdateEmpleadoRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateEmpleadoCommand(id, request.Nombre), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Activar o desactivar empleado")]
    [EndpointDescription("Cambia el estado del empleado entre activo e inactivo.")]
    [ProducesResponseType<EmpleadoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> Toggle(
        int id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ToggleEmpleadoActivoCommand(id), cancellationToken);
        return Results.Ok(result);
    }
}

public record UpdateEmpleadoRequest(string Nombre);
