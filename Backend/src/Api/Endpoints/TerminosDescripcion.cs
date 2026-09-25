using KPG.Timesheet.Application.Features.Catalogos.TerminosDescripcion.Commands.CreateTerminoDescripcion;
using KPG.Timesheet.Application.Features.Catalogos.TerminosDescripcion.Commands.ToggleTerminoDescripcionActivo;
using KPG.Timesheet.Application.Features.Catalogos.TerminosDescripcion.Commands.UpdateTerminoDescripcion;
using KPG.Timesheet.Application.Features.Catalogos.TerminosDescripcion.Queries.GetTerminosDescripcion;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPG.Timesheet.Api.Endpoints;

/// <summary>
/// Catalogo de terminos genericos para la calidad de descripciones (ver
/// Docs/plan-calidad-descripciones.md). Solo Admin: cambiar una regla aqui afecta el aviso
/// y el bloqueo de todos los que registran horas.
/// </summary>
public class TerminosDescripcion : IEndpointGroup
{
    public static string RoutePrefix => "/api/terminos-descripcion";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        var adminOnly = new AuthorizeAttribute { Roles = Roles.Admin };

        groupBuilder.MapGet("", GetAll).RequireAuthorization(adminOnly);
        groupBuilder.MapPost("", Create).RequireAuthorization(adminOnly);
        groupBuilder.MapPut("{id:int}", Update).RequireAuthorization(adminOnly);
        groupBuilder.MapPut("{id:int}/toggle", Toggle).RequireAuthorization(adminOnly);
    }

    [EndpointSummary("Listar terminos del catalogo de calidad de descripciones")]
    [ProducesResponseType<List<TerminoDescripcionDto>>(StatusCodes.Status200OK)]
    public static async Task<IResult> GetAll(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTerminosDescripcionQuery(SoloActivos: false), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Crear termino")]
    [ProducesResponseType<TerminoDescripcionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public static async Task<IResult> Create(
        [FromBody] CreateTerminoDescripcionRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateTerminoDescripcionCommand(
            request.Termino, request.Tipo, request.Regla, request.Severidad, request.Motivo, request.Sugerencia),
            cancellationToken);
        return Results.Created($"/api/terminos-descripcion/{result.Id}", result);
    }

    [EndpointSummary("Actualizar termino")]
    [ProducesResponseType<TerminoDescripcionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> Update(
        int id, [FromBody] UpdateTerminoDescripcionRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateTerminoDescripcionCommand(
            id, request.Termino, request.Tipo, request.Regla, request.Severidad, request.Motivo, request.Sugerencia),
            cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Activar o desactivar termino")]
    [ProducesResponseType<TerminoDescripcionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> Toggle(int id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ToggleTerminoDescripcionActivoCommand(id), cancellationToken);
        return Results.Ok(result);
    }
}

public record CreateTerminoDescripcionRequest(
    string Termino, TipoTermino Tipo, ReglaTermino Regla, SeveridadTermino Severidad, string Motivo, string? Sugerencia);

public record UpdateTerminoDescripcionRequest(
    string Termino, TipoTermino Tipo, ReglaTermino Regla, SeveridadTermino Severidad, string Motivo, string? Sugerencia);
