using KPG.Timesheet.Application.Features.Dashboard.Queries.GetDashboardGerencial;
using KPG.Timesheet.Application.Features.Dashboard.Queries.GetDistribucionHoras;
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
        var gerenteAndAbove = new AuthorizeAttribute
        {
            Roles = $"{Roles.Gerente},{Roles.Admin}"
        };

        groupBuilder.MapGet("estado-equipo", GetEstadoEquipo).RequireAuthorization(supervisorAndAbove);
        groupBuilder.MapGet("distribucion-horas", GetDistribucionHoras).RequireAuthorization(supervisorAndAbove);
        groupBuilder.MapGet("gerencial", GetDashboardGerencial).RequireAuthorization(gerenteAndAbove);
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

    [EndpointSummary("Distribución de horas del equipo")]
    [EndpointDescription("Retorna el total de horas registradas por consultor en el período indicado.")]
    [ProducesResponseType<DistribucionHorasResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<IResult> GetDistribucionHoras(
        ISender sender,
        CancellationToken cancellationToken,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var desdeEfectivo = desde ?? InicioSemana(hoy);
        var hastaEfectivo = hasta ?? hoy;

        if (desdeEfectivo > hastaEfectivo)
            return Results.BadRequest("'desde' no puede ser posterior a 'hasta'.");

        var query = new GetDistribucionHorasQuery(desdeEfectivo, hastaEfectivo);
        var result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Dashboard gerencial por cliente y proyecto")]
    [EndpointDescription("Retorna horas agrupadas por cliente y proyecto para el período indicado. Solo Gerente y Admin.")]
    [ProducesResponseType<DashboardGerencialResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<IResult> GetDashboardGerencial(
        ISender sender,
        CancellationToken cancellationToken,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var desdeEfectivo = desde ?? InicioSemana(hoy);
        var hastaEfectivo = hasta ?? hoy;

        if (desdeEfectivo > hastaEfectivo)
            return Results.BadRequest("'desde' no puede ser posterior a 'hasta'.");

        var query = new GetDashboardGerencialQuery(desdeEfectivo, hastaEfectivo);
        var result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    private static DateOnly InicioSemana(DateOnly fecha)
    {
        var diff = (int)fecha.DayOfWeek - (int)DayOfWeek.Monday;
        if (diff < 0) diff += 7;
        return fecha.AddDays(-diff);
    }
}
