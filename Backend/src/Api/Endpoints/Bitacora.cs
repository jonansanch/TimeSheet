using KPG.Timesheet.Application.Features.Bitacora.Queries.ExportarBitacora;
using KPG.Timesheet.Application.Features.Bitacora.Queries.GetBitacora;
using KPG.Timesheet.Application.Features.Bitacora.Queries.GetBitacoraAlcance;
using KPG.Timesheet.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPG.Timesheet.Api.Endpoints;

public class Bitacora : IEndpointGroup
{
    public static string RoutePrefix => "/api/bitacora";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        var adminOnly = new AuthorizeAttribute { Roles = Roles.Admin };
        var supervisorAndGerente = new AuthorizeAttribute
        {
            Roles = $"{Roles.Supervisor},{Roles.Gerente}"
        };

        groupBuilder.MapGet("", GetBitacora).RequireAuthorization(adminOnly);
        groupBuilder.MapGet("mi-alcance", GetBitacoraAlcance).RequireAuthorization(supervisorAndGerente);
        groupBuilder.MapGet("excel", ExportarBitacoraExcel).RequireAuthorization(adminOnly);
    }

    [EndpointSummary("Consultar bitácora de auditoría")]
    [EndpointDescription("Retorna eventos de auditoría filtrados por fecha, usuario y tipo. Solo Admin. Máximo 500 resultados, ordenados por Timestamp DESC.")]
    [ProducesResponseType<BitacoraResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<IResult> GetBitacora(
        ISender sender,
        CancellationToken cancellationToken,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        [FromQuery] string? actorId = null,
        [FromQuery] string? tipoEvento = null)
    {
        var query = new GetBitacoraQuery(desde, hasta, actorId, tipoEvento);
        var result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Consultar bitácora según alcance del rol")]
    [EndpointDescription("Supervisor: eventos de actores Empleado/Supervisor. Gerente: todos los eventos. Máximo 500, Timestamp DESC.")]
    [ProducesResponseType<BitacoraResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<IResult> GetBitacoraAlcance(
        ISender sender,
        CancellationToken cancellationToken,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        [FromQuery] string? actorId = null,
        [FromQuery] string? tipoEvento = null)
    {
        var query = new GetBitacoraAlcanceQuery(desde, hasta, actorId, tipoEvento);
        var result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Exportar bitácora a Excel")]
    [EndpointDescription("Genera y descarga un .xlsx con los eventos filtrados. Solo Admin. Máximo 500 eventos.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<IResult> ExportarBitacoraExcel(
        ISender sender,
        CancellationToken cancellationToken,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        [FromQuery] string? actorId = null,
        [FromQuery] string? tipoEvento = null)
    {
        var query = new ExportarBitacoraQuery(desde, hasta, actorId, tipoEvento);
        var result = await sender.Send(query, cancellationToken);
        return Results.File(result.Contenido, result.ContentType, result.FileName);
    }
}
