using KPG.Timesheet.Application.Features.ReportesUsuario;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPG.Timesheet.Api.Endpoints;

public class ReportesUsuario : IEndpointGroup
{
    public static string RoutePrefix => "/api/reportes-usuario";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        var cualquierUsuario = new AuthorizeAttribute
        {
            Roles = $"{Roles.Empleado},{Roles.Supervisor},{Roles.Gerente},{Roles.Admin}"
        };
        var adminOnly = new AuthorizeAttribute { Roles = Roles.Admin };

        groupBuilder.MapPost("", Crear).RequireAuthorization(cualquierUsuario);
        groupBuilder.MapGet("mios", GetMios).RequireAuthorization(cualquierUsuario);
        groupBuilder.MapGet("", GetTodos).RequireAuthorization(adminOnly);
        groupBuilder.MapPut("{id:int}/estado", CambiarEstado).RequireAuthorization(adminOnly);
    }

    [EndpointSummary("Reportar una falla o pedir una mejora")]
    [ProducesResponseType<ReporteUsuarioDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public static async Task<IResult> Crear(
        [FromBody] CrearReporteUsuarioRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateReporteUsuarioCommand(request.Tipo, request.Titulo, request.Descripcion),
            cancellationToken);
        return Results.Created($"/api/reportes-usuario/{result.Id}", result);
    }

    [EndpointSummary("Listar mis reportes")]
    [ProducesResponseType<IReadOnlyList<ReporteUsuarioDto>>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetMios(ISender sender, CancellationToken cancellationToken)
        => Results.Ok(await sender.Send(new GetMisReportesQuery(), cancellationToken));

    [EndpointSummary("Listar todos los reportes (Admin)")]
    [ProducesResponseType<IReadOnlyList<ReporteUsuarioDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<IResult> GetTodos(ISender sender, CancellationToken cancellationToken)
        => Results.Ok(await sender.Send(new GetTodosReportesQuery(), cancellationToken));

    [EndpointSummary("Cambiar el estado de un reporte (Admin)")]
    [ProducesResponseType<ReporteUsuarioDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> CambiarEstado(
        int id,
        [FromBody] CambiarEstadoReporteRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CambiarEstadoReporteCommand(id, request.NuevoEstado, request.Comentario),
            cancellationToken);
        return Results.Ok(result);
    }
}

public record CrearReporteUsuarioRequest(TipoReporte Tipo, string Titulo, string Descripcion);
public record CambiarEstadoReporteRequest(EstadoReporte NuevoEstado, string? Comentario);
