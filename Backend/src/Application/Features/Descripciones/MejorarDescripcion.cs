using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Domain.Constants;

namespace KPG.Timesheet.Application.Features.Descripciones;

/// <summary>
/// "Mejorar redacción" con IA (ver Docs/plan-calidad-descripciones.md, story 10.8). Nunca
/// reemplaza el texto por su cuenta: el usuario acepta o descarta la propuesta desde el
/// formulario, y la propuesta pasa por el mismo evaluador que todo lo demas.
/// </summary>
public record MejorarDescripcionCommand(string Texto, int? ProyectoId) : IRequest<MejorarDescripcionResultDto>;

public record MejorarDescripcionResultDto(
    /// <summary>False si no hay IA configurada en este ambiente: el frontend oculta el boton.</summary>
    bool Disponible,
    string? Propuesta,
    IReadOnlyList<string> Cambios,
    bool PropuestaBloquea,
    IReadOnlyList<string> ObservacionesPropuesta);

public class MejorarDescripcionCommandValidator : AbstractValidator<MejorarDescripcionCommand>
{
    public MejorarDescripcionCommandValidator()
    {
        RuleFor(x => x.Texto).NotEmpty().MaximumLength(1000);
    }
}

public class MejorarDescripcionCommandHandler(
    IRedactorDescripcion redactor,
    IValidadorDescripcion validador,
    IApplicationDbContext context,
    IBitacoraService bitacora,
    IClock clock,
    IUser user) : IRequestHandler<MejorarDescripcionCommand, MejorarDescripcionResultDto>
{
    private static readonly MejorarDescripcionResultDto SinPropuesta =
        new(Disponible: true, Propuesta: null, Cambios: [], PropuestaBloquea: false, ObservacionesPropuesta: []);

    public async Task<MejorarDescripcionResultDto> Handle(
        MejorarDescripcionCommand request, CancellationToken cancellationToken)
    {
        if (!await redactor.DisponibleAsync(cancellationToken))
            return new MejorarDescripcionResultDto(false, null, [], false, []);

        var actorId = user.Id
            ?? throw new UnauthorizedAccessException("No existe usuario autenticado.");

        await ExigirLimiteDiarioNoSuperadoAsync(actorId, cancellationToken);

        var (nombreCliente, nombreProyecto) = await ResolverClienteYProyectoAsync(request.ProyectoId, cancellationToken);

        var evaluacionActual = await validador.EvaluarAsync(request.Texto, request.ProyectoId, cancellationToken);
        var hallazgosActuales = evaluacionActual.Hallazgos.Select(h => h.Mensaje).ToList();

        var propuesta = await redactor.MejorarAsync(
            request.Texto, nombreCliente, nombreProyecto, hallazgosActuales, cancellationToken);

        await bitacora.RegistrarAsync(
            TipoEventoBitacora.MejoraDescripcionSolicitada,
            actorId, user.Email,
            "RegistrosHoras", null,
            new { request.ProyectoId, ObtuvoPropuesta = propuesta.Propuesta is not null },
            cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(propuesta.Propuesta))
            return SinPropuesta;

        // La propuesta de la IA no es una excepcion a las reglas: pasa por el mismo
        // evaluador, y si igual queda con hallazgos, el usuario lo ve antes de aceptarla.
        var evaluacionPropuesta = await validador.EvaluarAsync(propuesta.Propuesta, request.ProyectoId, cancellationToken);

        return new MejorarDescripcionResultDto(
            Disponible: true,
            Propuesta: propuesta.Propuesta,
            Cambios: propuesta.Cambios,
            PropuestaBloquea: evaluacionPropuesta.Bloquea,
            ObservacionesPropuesta: evaluacionPropuesta.Hallazgos.Select(h => h.Mensaje).ToList());
    }

    private async Task ExigirLimiteDiarioNoSuperadoAsync(string actorId, CancellationToken cancellationToken)
    {
        var limite = redactor.LimiteDiarioPorUsuario;
        if (limite <= 0)
            return;

        // BitacoraAuditoria.Timestamp siempre se guarda en UTC (ver BitacoraService): el
        // corte del dia tiene que calcularse en UTC tambien. Construir el DateTimeOffset a
        // partir de un DateTime sin Kind (como da clock.Today.ToDateTime) lo interpretaria
        // en la hora local del servidor y podia dejar pasar o descartar usos de hoy segun
        // la zona horaria.
        var inicioDelDia = new DateTimeOffset(clock.UtcNow.UtcDateTime.Date, TimeSpan.Zero);
        var usosHoy = await context.BitacoraAuditoria.CountAsync(
            b => b.ActorId == actorId
              && b.TipoEvento == TipoEventoBitacora.MejoraDescripcionSolicitada
              && b.Timestamp >= inicioDelDia,
            cancellationToken);

        if (usosHoy >= limite)
            throw new Common.Exceptions.ValidationException([new FluentValidation.Results.ValidationFailure(
                nameof(MejorarDescripcionCommand.Texto),
                $"Alcanzaste el límite de {limite} mejoras con IA por día. Probá de nuevo mañana o completá la descripción a mano.")]);
    }

    private async Task<(string? Cliente, string? Proyecto)> ResolverClienteYProyectoAsync(
        int? proyectoId, CancellationToken cancellationToken)
    {
        if (proyectoId is not > 0)
            return (null, null);

        var fila = await (
            from p in context.Proyectos
            join c in context.Clientes on p.ClienteId equals c.Id
            where p.Id == proyectoId
            select new { Cliente = c.Nombre, Proyecto = p.Nombre })
            .FirstOrDefaultAsync(cancellationToken);

        return (fila?.Cliente, fila?.Proyecto);
    }
}
