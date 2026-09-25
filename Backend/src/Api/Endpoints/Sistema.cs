using KPG.Timesheet.Application.Features.Sistema.Commands.UpdateGeminiApiKey;
using KPG.Timesheet.Application.Features.Sistema.Commands.UpdateLogoReportes;
using KPG.Timesheet.Application.Features.Sistema.Commands.UpdateUmbralNotificacion;
using KPG.Timesheet.Application.Features.Sistema.Commands.UpdateVentanaRetroactividad;
using KPG.Timesheet.Application.Features.Sistema.Queries.GetGeminiApiKeyEstado;
using KPG.Timesheet.Application.Features.Sistema.Queries.GetLogoReportes;
using KPG.Timesheet.Application.Features.Sistema.Queries.GetUmbralNotificacion;
using KPG.Timesheet.Application.Features.Sistema.Queries.GetVentanaRetroactividad;
using KPG.Timesheet.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPG.Timesheet.Api.Endpoints;

public class Sistema : IEndpointGroup
{
    public static string RoutePrefix => "/api/sistema";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        var adminOnly = new AuthorizeAttribute { Roles = Roles.Admin };
        var anyAuth = new AuthorizeAttribute
        {
            Roles = $"{Roles.Empleado},{Roles.Supervisor},{Roles.Gerente},{Roles.Admin}"
        };

        groupBuilder.MapGet("ventana-retroactividad", GetVentanaRetroactividad).RequireAuthorization(anyAuth);
        groupBuilder.MapGet("ventana-retroactividad/global", GetVentanaRetroactividadGlobal).RequireAuthorization(adminOnly);
        groupBuilder.MapPut("ventana-retroactividad", UpdateVentanaRetroactividad).RequireAuthorization(adminOnly);
        groupBuilder.MapGet("periodo-aprobacion", GetPeriodoAprobacion).RequireAuthorization(anyAuth);
        groupBuilder.MapGet("umbral-notificacion", GetUmbralNotificacion).RequireAuthorization(anyAuth);
        groupBuilder.MapPut("umbral-notificacion", UpdateUmbralNotificacion).RequireAuthorization(adminOnly);
        groupBuilder.MapGet("logo-reportes", GetLogoReportes).RequireAuthorization(adminOnly);
        groupBuilder.MapPut("logo-reportes", UpdateLogoReportes).RequireAuthorization(adminOnly);
        groupBuilder.MapGet("gemini-api-key", GetGeminiApiKeyEstado).RequireAuthorization(adminOnly);
        groupBuilder.MapPut("gemini-api-key", UpdateGeminiApiKey).RequireAuthorization(adminOnly);
    }

    [EndpointSummary("Obtener ventana de registro retroactivo del usuario autenticado")]
    [EndpointDescription("Ya aplica las excepciones por persona o por rol; es la ventana real que el backend hara cumplir.")]
    private static async Task<IResult> GetVentanaRetroactividad(ISender sender, CancellationToken cancellationToken)
    {
        var ventana = await sender.Send(new GetVentanaRetroactividadQuery(), cancellationToken);
        return Results.Ok(new { ventana });
    }

    [EndpointSummary("Obtener el periodo de aprobacion")]
    [EndpointDescription("Corte con el que el supervisor revisa: Semanal o Quincenal.")]
    private static async Task<IResult> GetPeriodoAprobacion(ISender sender, CancellationToken cancellationToken)
    {
        var periodo = await sender.Send(new GetPeriodoAprobacionQuery(), cancellationToken);
        return Results.Ok(new { periodo });
    }

    [EndpointSummary("Obtener ventana de registro retroactivo global")]
    [EndpointDescription("Valor base del sistema, sin excepciones aplicadas. Es el que edita la pantalla de parametros.")]
    private static async Task<IResult> GetVentanaRetroactividadGlobal(ISender sender, CancellationToken cancellationToken)
    {
        var ventana = await sender.Send(new GetVentanaRetroactividadGlobalQuery(), cancellationToken);
        return Results.Ok(new { ventana });
    }

    [EndpointSummary("Actualizar ventana de registro retroactivo")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    private static async Task<IResult> UpdateVentanaRetroactividad(
        [FromBody] UpdateVentanaRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new UpdateVentanaRetroactividadCommand(request.Dias), cancellationToken);
        return Results.NoContent();
    }

    [EndpointSummary("Obtener umbral de notificaciones")]
    private static async Task<IResult> GetUmbralNotificacion(ISender sender, CancellationToken cancellationToken)
    {
        var dias = await sender.Send(new GetUmbralNotificacionQuery(), cancellationToken);
        return Results.Ok(new { dias });
    }

    [EndpointSummary("Actualizar umbral de notificaciones")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    private static async Task<IResult> UpdateUmbralNotificacion(
        [FromBody] UpdateUmbralRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new UpdateUmbralNotificacionCommand(request.Dias), cancellationToken);
        return Results.NoContent();
    }

    [EndpointSummary("Obtener el logo parametrizado para los reportes")]
    [EndpointDescription("Data URI completo (data:image/png;base64,...) o cadena vacia si no hay logo configurado.")]
    private static async Task<IResult> GetLogoReportes(ISender sender, CancellationToken cancellationToken)
    {
        var logo = await sender.Send(new GetLogoReportesQuery(), cancellationToken);
        return Results.Ok(new { logo });
    }

    [EndpointSummary("Actualizar el logo de los reportes")]
    [EndpointDescription("PNG o JPG como data URI, hasta 1 MB. Enviar null o vacio para quitarlo.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    private static async Task<IResult> UpdateLogoReportes(
        [FromBody] UpdateLogoReportesRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new UpdateLogoReportesCommand(request.ImagenDataUri), cancellationToken);
        return Results.NoContent();
    }
    [EndpointSummary("Obtener el estado de la API key de Gemini")]
    [EndpointDescription("No devuelve la key completa, solo si esta configurada y sus ultimos caracteres.")]
    private static async Task<IResult> GetGeminiApiKeyEstado(ISender sender, CancellationToken cancellationToken)
    {
        var estado = await sender.Send(new GetGeminiApiKeyEstadoQuery(), cancellationToken);
        return Results.Ok(estado);
    }

    [EndpointSummary("Actualizar la API key de Gemini")]
    [EndpointDescription("Usada por el dictado de voz y 'mejorar redaccion'. Enviar vacio o null la quita.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    private static async Task<IResult> UpdateGeminiApiKey(
        [FromBody] UpdateGeminiApiKeyRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new UpdateGeminiApiKeyCommand(request.ApiKey), cancellationToken);
        return Results.NoContent();
    }
}

public record UpdateVentanaRequest(int Dias);
public record UpdateUmbralRequest(int Dias);
public record UpdateLogoReportesRequest(string? ImagenDataUri);
public record UpdateGeminiApiKeyRequest(string? ApiKey);
