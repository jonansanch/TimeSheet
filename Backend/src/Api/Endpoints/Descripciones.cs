using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Descripciones;
using KPG.Timesheet.Application.Features.Sistema.Commands.UpdateParametrosDescripcion;
using KPG.Timesheet.Application.Features.Sistema.Queries.GetParametrosDescripcion;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPG.Timesheet.Api.Endpoints;

/// <summary>
/// Aviso en vivo de calidad de descripciones (ver Docs/plan-calidad-descripciones.md).
/// Cualquier usuario autenticado lo puede llamar: es el mismo chequeo que corre al
/// guardar, expuesto para que el formulario avise mientras el usuario escribe.
/// </summary>
public class Descripciones : IEndpointGroup
{
    public static string RoutePrefix => "/api/descripciones";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        var anyAuth = new AuthorizeAttribute
        {
            Roles = $"{Roles.Empleado},{Roles.Supervisor},{Roles.Gerente},{Roles.Admin}"
        };

        var adminOnly = new AuthorizeAttribute { Roles = Roles.Admin };

        groupBuilder.MapPost("evaluar", Evaluar).RequireAuthorization(anyAuth);
        groupBuilder.MapPost("mejorar", Mejorar).RequireAuthorization(anyAuth);
        groupBuilder.MapGet("mejorar/disponible", MejorarDisponible).RequireAuthorization(anyAuth);
        groupBuilder.MapGet("parametros", GetParametros).RequireAuthorization(adminOnly);
        groupBuilder.MapPut("parametros", UpdateParametros).RequireAuthorization(adminOnly);
    }

    [EndpointSummary("Evaluar la calidad de una descripcion de registro de horas")]
    [ProducesResponseType<EvaluacionDescripcionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public static async Task<IResult> Evaluar(
        [FromBody] EvaluarDescripcionRequest request,
        IValidadorDescripcion validador,
        CancellationToken cancellationToken)
    {
        var evaluacion = await validador.EvaluarAsync(request.Texto, request.ProyectoId, cancellationToken);

        return Results.Ok(new EvaluacionDescripcionResponse(
            evaluacion.Bloquea,
            evaluacion.Hallazgos
                .Select(h => new HallazgoDescripcionResponse(
                    h.Codigo, h.Severidad.ToString(), h.Mensaje, h.Fragmento, h.Sugerencia))
                .ToList()));
    }

    [EndpointSummary("Mejorar la redaccion de una descripcion con IA")]
    [EndpointDescription("Devuelve una propuesta para que el usuario acepte o descarte; nunca reemplaza el texto solo. Vacia (Propuesta null) si no hay IA configurada o el modelo no pudo mejorarla.")]
    [ProducesResponseType<MejorarDescripcionResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public static async Task<IResult> Mejorar(
        [FromBody] MejorarDescripcionRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new MejorarDescripcionCommand(request.Texto, request.ProyectoId), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Indica si mejorar la redaccion con IA esta configurado en este ambiente")]
    [EndpointDescription("Permite al cliente decidir si ofrece el boton de mejorar, igual que /api/voz/disponible.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    private static IResult MejorarDisponible(IRedactorDescripcion redactor)
        => Results.Ok(new { disponible = redactor.Disponible });

    [EndpointSummary("Obtener los parametros de calidad de descripciones")]
    [ProducesResponseType<ParametrosDescripcionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    private static async Task<IResult> GetParametros(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetParametrosDescripcionQuery(), cancellationToken);
        return Results.Ok(result);
    }

    [EndpointSummary("Actualizar los parametros de calidad de descripciones")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    private static async Task<IResult> UpdateParametros(
        [FromBody] UpdateParametrosDescripcionRequest request, ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(new UpdateParametrosDescripcionCommand(
            request.ValidacionActiva, request.MinPalabras, request.MinPalabrasContexto, request.SeveridadReglasBase),
            cancellationToken);
        return Results.NoContent();
    }
}

public record EvaluarDescripcionRequest(string? Texto, int? ProyectoId);

public record MejorarDescripcionRequest(string Texto, int? ProyectoId);

public record UpdateParametrosDescripcionRequest(
    bool ValidacionActiva, int MinPalabras, int MinPalabrasContexto, SeveridadTermino SeveridadReglasBase);

public record HallazgoDescripcionResponse(
    string Codigo, string Severidad, string Mensaje, string? Fragmento, string? Sugerencia);

public record EvaluacionDescripcionResponse(bool Bloquea, List<HallazgoDescripcionResponse> Hallazgos);
