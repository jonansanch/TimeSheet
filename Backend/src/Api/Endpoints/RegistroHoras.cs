using KPG.Timesheet.Application.Features.RegistroHoras.Commands.CreateRegistroHoras;
using KPG.Timesheet.Application.Features.RegistroHoras.Commands.DeleteRegistroHoras;
using KPG.Timesheet.Application.Features.RegistroHoras.Commands.UpdateDescripcionRegistroHoras;
using KPG.Timesheet.Application.Features.RegistroHoras.Queries.GetMisRegistros;
using KPG.Timesheet.Application.Features.RegistroHoras.Queries.GetRegistrosRecientes;
using KPG.Timesheet.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPG.Timesheet.Api.Endpoints;

public class RegistroHoras : IEndpointGroup
{
    public static string RoutePrefix => "/api/registros-horas";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        var auth = new AuthorizeAttribute
        {
            Roles = $"{Roles.Empleado},{Roles.Supervisor},{Roles.Gerente},{Roles.Admin}"
        };

        var supervisorAdmin = new AuthorizeAttribute
        {
            Roles = $"{Roles.Supervisor},{Roles.Admin}"
        };

        groupBuilder.MapPost(Create).RequireAuthorization(auth);
        groupBuilder.MapGet("", GetMisRegistros).RequireAuthorization(auth);
        groupBuilder.MapGet("recientes", GetRecientes).RequireAuthorization(auth);
        groupBuilder.MapDelete("{id}", Delete).RequireAuthorization(auth);
        groupBuilder.MapPatch("{id}/descripcion", UpdateDescripcion).RequireAuthorization(supervisorAdmin);
    }

    [EndpointSummary("Obtener historial de registros del usuario autenticado")]
    [EndpointDescription("Retorna registros del usuario autenticado en el rango indicado, ordenados por fecha descendente. Por defecto retorna el mes en curso.")]
    [ProducesResponseType<IEnumerable<MisRegistrosItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public static async Task<IResult> GetMisRegistros(
        ISender sender,
        CancellationToken cancellationToken,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var desdeEfectivo = desde ?? new DateOnly(hoy.Year, hoy.Month, 1);
        var hastaEfectivo = hasta ?? hoy;

        var result = await sender.Send(new GetMisRegistrosQuery(desdeEfectivo, hastaEfectivo), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Registrar turno AM/PM")]
    [EndpointDescription("Crea un registro de horas para el usuario autenticado en la fecha y turno indicados.")]
    [ProducesResponseType<RegistroHorasDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public static async Task<IResult> Create(
        [FromBody] CreateRegistroHorasCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return Results.Created($"/api/registros-horas/{result.Id}", result);
    }

    [EndpointSummary("Eliminar registro de horas propio")]
    [EndpointDescription("Elimina un registro del usuario autenticado. Retorna 403 si el registro pertenece a otro usuario.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public static async Task<IResult> Delete(
        int id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteRegistroHorasCommand(id), cancellationToken);
        return Results.NoContent();
    }

    [EndpointSummary("Actualizar descripción de un registro")]
    [EndpointDescription("Actualiza únicamente el campo descripción de un registro existente. Solo Supervisor y Admin pueden ejecutar esta acción.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public static async Task<IResult> UpdateDescripcion(
        int id,
        [FromBody] UpdateDescripcionRequest body,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new UpdateDescripcionRegistroHorasCommand(id, body.Descripcion), cancellationToken);
        return Results.NoContent();
    }

    [EndpointSummary("Obtener sugerencias recientes de cliente/proyecto")]
    [EndpointDescription("Retorna las últimas combinaciones cliente/proyecto usadas por el usuario autenticado.")]
    [ProducesResponseType<IEnumerable<RegistroRecienteDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public static async Task<IResult> GetRecientes(
        ISender sender,
        CancellationToken cancellationToken,
        [FromQuery] int top = 5)
    {
        var result = await sender.Send(new GetRegistrosRecientesQuery(top), cancellationToken);
        return Results.Ok(result);
    }
}

public record UpdateDescripcionRequest(string Descripcion);
