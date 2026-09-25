using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Voz;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KPG.Timesheet.Api.Endpoints;

public class Voz : IEndpointGroup
{
    public static string RoutePrefix => "/api/voz";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost("/interpretar", Interpretar).RequireAuthorization();
        groupBuilder.MapGet("/disponible", Disponible).RequireAuthorization();
    }

    [EndpointSummary("Interpreta un dictado y devuelve los campos del registro")]
    [ProducesResponseType<InterpretacionVozDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    private static async Task<IResult> Interpretar(
        [FromBody] InterpretarVozCommand command,
        ISender sender,
        CancellationToken cancellationToken)
        => Results.Ok(await sender.Send(command, cancellationToken));

    [EndpointSummary("Indica si la interpretacion por IA esta configurada en este ambiente")]
    [EndpointDescription("Permite al cliente decidir si ofrece el dictado por IA o su parser local.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    private static async Task<IResult> Disponible(IInterpreteVoz interprete, CancellationToken cancellationToken)
        => Results.Ok(new { disponible = await interprete.DisponibleAsync(cancellationToken) });
}
