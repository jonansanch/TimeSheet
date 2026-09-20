using KPG.Timesheet.Application.Features.Aprobaciones;
using KPG.Timesheet.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPG.Timesheet.Api.Endpoints;

public class Aprobaciones : IEndpointGroup
{
    public static string RoutePrefix => "/api/aprobaciones";

    public static void Map(RouteGroupBuilder groupBuilder)
    {
        var revisores = new AuthorizeAttribute
        {
            Roles = $"{Roles.Supervisor},{Roles.Gerente},{Roles.Admin}"
        };
        var propietario = new AuthorizeAttribute
        {
            Roles = $"{Roles.Empleado},{Roles.Supervisor},{Roles.Gerente},{Roles.Admin}"
        };

        groupBuilder.MapGet("pendientes", GetPendientes).RequireAuthorization(revisores);
        groupBuilder.MapGet("empleados", GetEmpleadosRevisables).RequireAuthorization(revisores);
        groupBuilder.MapPost("{id:int}/aprobar", Aprobar).RequireAuthorization(revisores);
        groupBuilder.MapPost("{id:int}/rechazar", Rechazar).RequireAuthorization(revisores);
        groupBuilder.MapPost("{id:int}/revertir-aprobacion", RevertirAprobacion).RequireAuthorization(revisores);
        groupBuilder.MapPost("{id:int}/revertir-rechazo", RevertirRechazo).RequireAuthorization(revisores);
        groupBuilder.MapPost("{id:int}/reenviar", Reenviar).RequireAuthorization(propietario);
        groupBuilder.MapPost("importar", Importar)
            .RequireAuthorization(new AuthorizeAttribute { Roles = Roles.Admin })
            .DisableAntiforgery();
    }

    [EndpointSummary("Empleados que le toca revisar al usuario autenticado")]
    [EndpointDescription("Alimenta el filtro de la pantalla de aprobaciones. Solo devuelve a quienes el revisor les aprueba algun nivel.")]
    [ProducesResponseType<IReadOnlyList<EmpleadoRevisableDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    private static async Task<IResult> GetEmpleadosRevisables(
        ISender sender,
        CancellationToken cancellationToken)
        => Results.Ok(await sender.Send(new GetEmpleadosRevisablesQuery(), cancellationToken));

    [EndpointSummary("Registros que esperan mi aprobacion")]
    [EndpointDescription("Devuelve los dias agrupados por empleado cuyo nivel pendiente le corresponde al usuario autenticado. Filtros opcionales por empleado, cliente, proyecto y puesto.")]
    [ProducesResponseType<PendientesAprobacionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<IResult> GetPendientes(
        ISender sender,
        CancellationToken cancellationToken,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        [FromQuery] string? userId = null,
        [FromQuery] int? clienteId = null,
        [FromQuery] int? proyectoId = null,
        [FromQuery] int? puestoId = null,
        [FromQuery] bool incluirRevisados = false)
    {
        // Por defecto, la semana en curso: es el corte con el que el supervisor revisa.
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var lunes = hoy.AddDays(-(((int)hoy.DayOfWeek + 6) % 7));

        var result = await sender.Send(new GetPendientesAprobacionQuery(
            desde ?? lunes,
            hasta ?? lunes.AddDays(6),
            userId, clienteId, proyectoId, puestoId, incluirRevisados), cancellationToken);

        return Results.Ok(result);
    }

    [EndpointSummary("Aprobar el nivel pendiente de un registro")]
    [EndpointDescription("Solo puede aprobar el supervisor asignado a ese nivel. Si la misma persona es tambien el aprobador del nivel siguiente, la cadena avanza sola para no pedirle dos confirmaciones seguidas.")]
    [ProducesResponseType<EstadoRegistroDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> Aprobar(int id, ISender sender, CancellationToken cancellationToken)
        => Results.Ok(await sender.Send(new AprobarRegistroCommand(id), cancellationToken));

    [EndpointSummary("Rechazar un registro")]
    [EndpointDescription("El comentario es obligatorio. El registro vuelve siempre al empleado, sin importar en que nivel se rechace.")]
    [ProducesResponseType<EstadoRegistroDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<IResult> Rechazar(
        int id,
        [FromBody] RechazarRequest request,
        ISender sender,
        CancellationToken cancellationToken)
        => Results.Ok(await sender.Send(new RechazarRegistroCommand(id, request.Comentario), cancellationToken));

    [EndpointSummary("Deshacer la ultima aprobacion")]
    [EndpointDescription("Retrocede un nivel. Solo puede hacerlo quien aprobo ese nivel.")]
    [ProducesResponseType<EstadoRegistroDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<IResult> RevertirAprobacion(int id, ISender sender, CancellationToken cancellationToken)
        => Results.Ok(await sender.Send(new RevertirAprobacionCommand(id), cancellationToken));

    [EndpointSummary("Deshacer un rechazo")]
    [EndpointDescription("Devuelve el registro al punto de la cadena en el que estaba, sin que el empleado tenga que reenviarlo. Solo puede hacerlo quien lo rechazo.")]
    [ProducesResponseType<EstadoRegistroDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<IResult> RevertirRechazo(int id, ISender sender, CancellationToken cancellationToken)
        => Results.Ok(await sender.Send(new RevertirRechazoCommand(id), cancellationToken));

    [EndpointSummary("Reenviar un registro rechazado")]
    [EndpointDescription("Lo hace el propio empleado tras corregirlo. La cadena recomienza desde el primer nivel.")]
    [ProducesResponseType<EstadoRegistroDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<IResult> Reenviar(int id, ISender sender, CancellationToken cancellationToken)
        => Results.Ok(await sender.Send(new ReenviarRegistroCommand(id), cancellationToken));


    [EndpointSummary("Importar timesheet desde Excel")]
    [EndpointDescription("Carga historica de horas desde la plantilla de los consultores. Omite las filas que ya existan y salta la ventana de retroactividad, por lo que solo Admin puede ejecutarla.")]
    [ProducesResponseType<ImportacionResultadoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<IResult> Importar(
        IFormFile archivo,
        [FromForm] string userId,
        ISender sender,
        CancellationToken cancellationToken,
        [FromForm] bool marcarAprobado = true)
    {
        if (archivo is null || archivo.Length == 0)
            return Results.BadRequest("No se recibio ningun archivo.");

        using var ms = new MemoryStream();
        await archivo.CopyToAsync(ms, cancellationToken);

        var result = await sender.Send(
            new ImportarTimesheetCommand(ms.ToArray(), userId, marcarAprobado), cancellationToken);

        return Results.Ok(result);
    }
}

public record RechazarRequest(string Comentario);
