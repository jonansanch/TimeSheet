using KPG.Timesheet.Application.Features.Organizacion.SupervisoresPuesto;
using KPG.Timesheet.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPG.Timesheet.Api.Endpoints;

public class SupervisoresPuesto : IEndpointGroup
{
    public static string RoutePrefix => "/api/supervisores-puesto";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        var adminOnly = new AuthorizeAttribute { Roles = Roles.Admin };

        groupBuilder.MapGet("", GetAll).RequireAuthorization(adminOnly);
        groupBuilder.MapPost("", Guardar).RequireAuthorization(adminOnly);
        groupBuilder.MapPut("{id:int}", Actualizar).RequireAuthorization(adminOnly);
        groupBuilder.MapPut("{id:int}/toggle", Toggle).RequireAuthorization(adminOnly);
    }

    [EndpointSummary("Listar reglas de supervisor por puesto")]
    [EndpointDescription("Retorna quien aprueba en primer nivel los registros de cada puesto. Una regla sin cliente aplica a todos los clientes.")]
    [ProducesResponseType<IReadOnlyList<SupervisorPuestoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<IResult> GetAll(ISender sender, CancellationToken cancellationToken)
        => Results.Ok(await sender.Send(new GetSupervisoresPuestoQuery(), cancellationToken));

    [EndpointSummary("Crear regla de supervisor por puesto")]
    [ProducesResponseType<SupervisorPuestoDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<IResult> Guardar(
        [FromBody] GuardarSupervisorPuestoRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GuardarSupervisorPuestoCommand(null, request.PuestoId, request.ClienteId, request.SupervisorUserId),
            cancellationToken);
        return Results.Created($"/api/supervisores-puesto/{result.Id}", result);
    }

    [EndpointSummary("Actualizar regla de supervisor por puesto")]
    [ProducesResponseType<SupervisorPuestoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> Actualizar(
        int id,
        [FromBody] GuardarSupervisorPuestoRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GuardarSupervisorPuestoCommand(id, request.PuestoId, request.ClienteId, request.SupervisorUserId),
            cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Activar o desactivar regla")]
    [ProducesResponseType<SupervisorPuestoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> Toggle(int id, ISender sender, CancellationToken cancellationToken)
        => Results.Ok(await sender.Send(new ToggleSupervisorPuestoCommand(id), cancellationToken));
}

public record GuardarSupervisorPuestoRequest(int PuestoId, int? ClienteId, string SupervisorUserId);
