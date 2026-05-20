using KPG.Timesheet.Application.Features.Reportes.Queries.ExportarReporteHoras;
using KPG.Timesheet.Application.Features.Reportes.Queries.GetReporteHoras;
using KPG.Timesheet.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPG.Timesheet.Api.Endpoints;

public class Reportes : IEndpointGroup
{
    public static string RoutePrefix => "/api/reportes";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        var supervisorAndAbove = new AuthorizeAttribute
        {
            Roles = $"{Roles.Supervisor},{Roles.Gerente},{Roles.Admin}"
        };

        groupBuilder.MapGet("horas", GetReporteHoras).RequireAuthorization(supervisorAndAbove);
        groupBuilder.MapGet("horas/excel", ExportarExcel).RequireAuthorization(supervisorAndAbove);
        groupBuilder.MapGet("horas/pdf", ExportarPdf).RequireAuthorization(supervisorAndAbove);
    }

    [EndpointSummary("Reporte de horas con filtros")]
    [EndpointDescription("Retorna registros de horas filtrados por período, empleado, cliente y proyecto. Máximo 1000 filas.")]
    [ProducesResponseType<ReporteHorasResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<IResult> GetReporteHoras(
        ISender sender,
        CancellationToken cancellationToken,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        [FromQuery] string? userId = null,
        [FromQuery] string? cliente = null,
        [FromQuery] string? proyecto = null)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var desdeEfectivo = desde ?? new DateOnly(hoy.Year, hoy.Month, 1);
        var hastaEfectivo = hasta ?? hoy;

        if (desdeEfectivo > hastaEfectivo)
            return Results.BadRequest("'desde' no puede ser posterior a 'hasta'.");

        var query = new GetReporteHorasQuery(desdeEfectivo, hastaEfectivo, userId, cliente, proyecto);
        var result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Exportar reporte de horas a Excel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<IResult> ExportarExcel(
        ISender sender,
        CancellationToken cancellationToken,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        [FromQuery] string? userId = null,
        [FromQuery] string? cliente = null,
        [FromQuery] string? proyecto = null)
        => await Exportar(sender, cancellationToken, desde, hasta, userId, cliente, proyecto, ExportFormato.Excel);

    [EndpointSummary("Exportar reporte de horas a PDF")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<IResult> ExportarPdf(
        ISender sender,
        CancellationToken cancellationToken,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        [FromQuery] string? userId = null,
        [FromQuery] string? cliente = null,
        [FromQuery] string? proyecto = null)
        => await Exportar(sender, cancellationToken, desde, hasta, userId, cliente, proyecto, ExportFormato.Pdf);

    private static async Task<IResult> Exportar(
        ISender sender,
        CancellationToken cancellationToken,
        DateOnly? desde, DateOnly? hasta,
        string? userId, string? cliente, string? proyecto,
        ExportFormato formato)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var desdeEfectivo = desde ?? new DateOnly(hoy.Year, hoy.Month, 1);
        var hastaEfectivo = hasta ?? hoy;

        if (desdeEfectivo > hastaEfectivo)
            return Results.BadRequest("'desde' no puede ser posterior a 'hasta'.");

        var query = new ExportarReporteHorasQuery(desdeEfectivo, hastaEfectivo, userId, cliente, proyecto, formato);
        var result = await sender.Send(query, cancellationToken);
        return Results.File(result.Contenido, result.ContentType, result.FileName);
    }
}
