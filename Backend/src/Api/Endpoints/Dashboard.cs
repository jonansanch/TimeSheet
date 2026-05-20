using KPG.Timesheet.Application.Features.Dashboard.Queries.GetEstadoEquipo;
using KPG.Timesheet.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPG.Timesheet.Api.Endpoints;

public class Dashboard : IEndpointGroup
{
    public static string RoutePrefix => "/api/dashboard";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        var supervisorAndAbove = new AuthorizeAttribute
        {
            Roles = $"{Roles.Supervisor},{Roles.Gerente},{Roles.Admin}"
        };

        groupBuilder.MapGet("estado-equipo", GetEstadoEquipo).RequireAuthorization(supervisorAndAbove);
    }

    [EndpointSummary("Estado diario del equipo")]
    [EndpointDescription("Retorna el estado de registro AM/PM de cada miembro del equipo para la fecha indicada.")]
    [ProducesResponseType<EstadoEquipoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<IResult> GetEstadoEquipo(
        ISender sender,
        CancellationToken cancellationToken,
        [FromQuery] DateOnly? fecha = null)
    {
        var query = new GetEstadoEquipoQuery(fecha ?? DateOnly.FromDateTime(DateTime.Today));
        var result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }
}
