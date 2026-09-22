using KPG.Timesheet.Application.Features.Sistema.RestriccionesDia;
using KPG.Timesheet.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPG.Timesheet.Api.Endpoints;

public class RestriccionesDia : IEndpointGroup
{
    public static string RoutePrefix => "/api/restricciones-dia";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        var adminOnly = new AuthorizeAttribute { Roles = Roles.Admin };

        groupBuilder.MapGet("", GetAll).RequireAuthorization(adminOnly);
        groupBuilder.MapPost("", Crear).RequireAuthorization(adminOnly);
        groupBuilder.MapPut("{id:int}/toggle", Toggle).RequireAuthorization(adminOnly);
    }

    [EndpointSummary("Listar restricciones de dia")]
    [EndpointDescription("Dias de la semana en los que una persona o un rol no puede registrar horas.")]
    [ProducesResponseType<IReadOnlyList<RestriccionDiaDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<IResult> GetAll(ISender sender, CancellationToken cancellationToken)
        => Results.Ok(await sender.Send(new GetRestriccionesDiaQuery(), cancellationToken));

    [EndpointSummary("Crear restriccion de dia")]
    [ProducesResponseType<RestriccionDiaDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public static async Task<IResult> Crear(
        [FromBody] GuardarRestriccionDiaRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GuardarRestriccionDiaCommand(request.DiaDelaSemana, request.UserId, request.Rol),
            cancellationToken);
        return Results.Created($"/api/restricciones-dia/{result.Id}", result);
    }

    [EndpointSummary("Activar o desactivar restriccion de dia")]
    [ProducesResponseType<RestriccionDiaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> Toggle(int id, ISender sender, CancellationToken cancellationToken)
        => Results.Ok(await sender.Send(new ToggleRestriccionDiaCommand(id), cancellationToken));
}

public record GuardarRestriccionDiaRequest(DayOfWeek DiaDelaSemana, string? UserId, string? Rol);
