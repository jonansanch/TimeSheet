using KPG.Timesheet.Application.Features.SolicitudesExcepcion.Commands.AprobarSolicitudExcepcion;
using KPG.Timesheet.Application.Features.SolicitudesExcepcion.Commands.CreateSolicitudExcepcion;
using KPG.Timesheet.Application.Features.SolicitudesExcepcion.Commands.RechazarSolicitudExcepcion;
using KPG.Timesheet.Application.Features.SolicitudesExcepcion.Queries.GetSolicitudesExcepcion;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KPG.Timesheet.Api.Endpoints;

public class SolicitudesExcepcion : IEndpointGroup
{
    public static string RoutePrefix => "/api/solicitudes-excepcion";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost("/", Create).RequireAuthorization();
        groupBuilder.MapGet("/", GetAll).RequireAuthorization();
        groupBuilder.MapPost("/{id:int}/aprobar", Aprobar).RequireAuthorization();
        groupBuilder.MapPost("/{id:int}/rechazar", Rechazar).RequireAuthorization();
    }

    [EndpointSummary("Crear solicitud de excepción para registro fuera de ventana")]
    [ProducesResponseType<SolicitudExcepcionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    private static async Task<IResult> Create(
        [FromBody] CreateSolicitudExcepcionCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var dto = await sender.Send(command, cancellationToken);
        return Results.Created($"/api/solicitudes-excepcion/{dto.Id}", dto);
    }

    [EndpointSummary("Listar todas las solicitudes de excepción (Admin)")]
    [ProducesResponseType<IEnumerable<SolicitudExcepcionAdminDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    private static async Task<IResult> GetAll(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSolicitudesExcepcionQuery(), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Aprobar solicitud de excepción (Admin)")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    private static async Task<IResult> Aprobar(int id, ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(new AprobarSolicitudExcepcionCommand(id), cancellationToken);
        return Results.NoContent();
    }

    [EndpointSummary("Rechazar solicitud de excepción (Admin)")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    private static async Task<IResult> Rechazar(int id, ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(new RechazarSolicitudExcepcionCommand(id), cancellationToken);
        return Results.NoContent();
    }
}
