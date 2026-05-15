using KPG.Timesheet.Application.Features.Catalogos.LugaresTrabajo.Commands.CreateLugarTrabajo;
using KPG.Timesheet.Application.Features.Catalogos.LugaresTrabajo.Commands.ToggleLugarTrabajoActivo;
using KPG.Timesheet.Application.Features.Catalogos.LugaresTrabajo.Commands.UpdateLugarTrabajo;
using KPG.Timesheet.Application.Features.Catalogos.LugaresTrabajo.Queries.GetLugaresTrabajo;
using KPG.Timesheet.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPG.Timesheet.Api.Endpoints;

public class LugaresTrabajo : IEndpointGroup
{
    public static string RoutePrefix => "/api/lugares-trabajo";

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

    [EndpointSummary("Listar lugares de trabajo")]
    [ProducesResponseType<List<LugarTrabajoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<IResult> GetAll(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetLugaresTrabajosQuery(SoloActivos: false), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Listar lugares de trabajo activos")]
    [ProducesResponseType<List<LugarTrabajoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public static async Task<IResult> GetActivos(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetLugaresTrabajosQuery(SoloActivos: true), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Crear lugar de trabajo")]
    [ProducesResponseType<LugarTrabajoDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<IResult> Create(
        [FromBody] CreateLugarTrabajoCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return Results.Created($"/api/lugares-trabajo/{result.Id}", result);
    }

    [EndpointSummary("Actualizar nombre de lugar de trabajo")]
    [ProducesResponseType<LugarTrabajoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> Update(
        int id,
        [FromBody] UpdateLugarTrabajoRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateLugarTrabajoCommand(id, request.Nombre), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Activar o desactivar lugar de trabajo")]
    [ProducesResponseType<LugarTrabajoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> Toggle(
        int id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ToggleLugarTrabajoActivoCommand(id), cancellationToken);
        return Results.Ok(result);
    }
}

public record UpdateLugarTrabajoRequest(string Nombre);
