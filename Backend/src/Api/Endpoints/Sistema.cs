using KPG.Timesheet.Application.Features.Sistema.Commands.UpdateUmbralNotificacion;
using KPG.Timesheet.Application.Features.Sistema.Commands.UpdateVentanaRetroactividad;
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
        groupBuilder.MapPut("ventana-retroactividad", UpdateVentanaRetroactividad).RequireAuthorization(adminOnly);
        groupBuilder.MapGet("umbral-notificacion", GetUmbralNotificacion).RequireAuthorization(anyAuth);
        groupBuilder.MapPut("umbral-notificacion", UpdateUmbralNotificacion).RequireAuthorization(adminOnly);
    }

    [EndpointSummary("Obtener ventana de registro retroactivo")]
    private static async Task<IResult> GetVentanaRetroactividad(ISender sender, CancellationToken cancellationToken)
    {
        var ventana = await sender.Send(new GetVentanaRetroactividadQuery(), cancellationToken);
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
}

public record UpdateVentanaRequest(int Dias);
public record UpdateUmbralRequest(int Dias);
