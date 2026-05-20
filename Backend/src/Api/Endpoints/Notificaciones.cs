using KPG.Timesheet.Application.Features.Notificaciones.Queries.GetHistorialNotificaciones;
using KPG.Timesheet.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPG.Timesheet.Api.Endpoints;

public class Notificaciones : IEndpointGroup
{
    public static string RoutePrefix => "/api/notificaciones";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        var supervisorAndAbove = new AuthorizeAttribute
        {
            Roles = $"{Roles.Supervisor},{Roles.Gerente},{Roles.Admin}"
        };

        groupBuilder.MapGet("historial", GetHistorial).RequireAuthorization(supervisorAndAbove);
    }

    [EndpointSummary("Historial de notificaciones enviadas")]
    [EndpointDescription("Retorna el historial de notificaciones por pendientes de registro, con filtros opcionales.")]
    [ProducesResponseType<HistorialNotificacionesResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<IResult> GetHistorial(
        ISender sender,
        CancellationToken cancellationToken,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        [FromQuery] string? userId = null,
        [FromQuery] bool? soloErrores = null)
    {
        var query = new GetHistorialNotificacionesQuery(desde, hasta, userId, soloErrores);
        var result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }
}
