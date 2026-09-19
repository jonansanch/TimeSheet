using KPG.Timesheet.Application.Features.Sistema.ReglasVentana;
using KPG.Timesheet.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPG.Timesheet.Api.Endpoints;

public class ReglasVentana : IEndpointGroup
{
    public static string RoutePrefix => "/api/reglas-ventana";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        var adminOnly = new AuthorizeAttribute { Roles = Roles.Admin };

        groupBuilder.MapGet("", GetAll).RequireAuthorization(adminOnly);
        groupBuilder.MapPost("", Crear).RequireAuthorization(adminOnly);
        groupBuilder.MapPut("{id:int}", Actualizar).RequireAuthorization(adminOnly);
        groupBuilder.MapPut("{id:int}/toggle", Toggle).RequireAuthorization(adminOnly);
    }

    [EndpointSummary("Listar excepciones de ventana retroactiva")]
    [EndpointDescription("Reglas que amplian o reducen los dias de registro retroactivo para una persona o un rol concreto.")]
    [ProducesResponseType<IReadOnlyList<ReglaVentanaDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<IResult> GetAll(ISender sender, CancellationToken cancellationToken)
        => Results.Ok(await sender.Send(new GetReglasVentanaQuery(), cancellationToken));

    [EndpointSummary("Crear excepcion de ventana")]
    [ProducesResponseType<ReglaVentanaDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public static async Task<IResult> Crear(
        [FromBody] GuardarReglaVentanaRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GuardarReglaVentanaCommand(null, request.UserId, request.Rol, request.Dias),
            cancellationToken);
        return Results.Created($"/api/reglas-ventana/{result.Id}", result);
    }

    [EndpointSummary("Actualizar dias de una excepcion")]
    [ProducesResponseType<ReglaVentanaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> Actualizar(
        int id,
        [FromBody] GuardarReglaVentanaRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GuardarReglaVentanaCommand(id, request.UserId, request.Rol, request.Dias),
            cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Activar o desactivar excepcion")]
    [ProducesResponseType<ReglaVentanaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> Toggle(int id, ISender sender, CancellationToken cancellationToken)
        => Results.Ok(await sender.Send(new ToggleReglaVentanaCommand(id), cancellationToken));
}

public record GuardarReglaVentanaRequest(string? UserId, string? Rol, int Dias);
