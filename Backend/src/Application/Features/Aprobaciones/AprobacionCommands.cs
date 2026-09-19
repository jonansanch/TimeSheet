using FluentValidation.Results;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Domain.Enums;
using NotFoundException = KPG.Timesheet.Application.Common.Exceptions.NotFoundException;
using ForbiddenAccessException = KPG.Timesheet.Application.Common.Exceptions.ForbiddenAccessException;
using ValidationException = KPG.Timesheet.Application.Common.Exceptions.ValidationException;
using RegistroHorasEntity = KPG.Timesheet.Domain.Entities.RegistroHoras;

namespace KPG.Timesheet.Application.Features.Aprobaciones;

public record EstadoRegistroDto(
    int Id,
    EstadoAprobacion Estado,
    int? NivelPendiente,
    string? ComentarioRechazo);

// ── Aprobar ──────────────────────────────────────────────────────────────────

[Authorize(Roles = $"{Roles.Supervisor},{Roles.Gerente},{Roles.Admin}")]
public record AprobarRegistroCommand(int RegistroHorasId) : IRequest<EstadoRegistroDto>;

public class AprobarRegistroCommandHandler(
    IApplicationDbContext context,
    ICadenaAprobacionService cadena,
    IBitacoraService bitacora,
    IUser actor)
    : IRequestHandler<AprobarRegistroCommand, EstadoRegistroDto>
{
    public async Task<EstadoRegistroDto> Handle(
        AprobarRegistroCommand request,
        CancellationToken cancellationToken)
    {
        var (registro, cadenaAprobacion, actorId) =
            await AprobacionHelper.CargarAsync(context, cadena, actor, request.RegistroHorasId, cancellationToken);

        var nivel = registro.NivelPendiente
            ?? throw new ValidationException([new ValidationFailure(
                nameof(request.RegistroHorasId),
                registro.EstaRechazado
                    ? "El registro esta rechazado: el empleado debe corregirlo y reenviarlo."
                    : "El registro ya esta aprobado en todos los niveles.")]);

        AprobacionHelper.ExigirAprobadorDelNivel(cadenaAprobacion, nivel, actorId);

        registro.Aprobar(nivel);
        context.AprobacionesRegistro.Add(new AprobacionRegistro(
            registro.Id, AprobacionRegistro.AccionAprobar, nivel, actorId));

        // Si la misma persona tambien es el aprobador del nivel siguiente, no tiene sentido
        // pedirle que apruebe dos veces seguidas: la cadena avanza sola.
        while (registro.NivelPendiente is { } siguiente
            && AprobacionHelper.AprobadorDe(cadenaAprobacion, siguiente) == actorId)
        {
            registro.Aprobar(siguiente);
            context.AprobacionesRegistro.Add(new AprobacionRegistro(
                registro.Id, AprobacionRegistro.AccionAprobar, siguiente, actorId));
        }

        await bitacora.RegistrarAsync(
            TipoEventoBitacora.AprobacionRegistro, actorId, null,
            "RegistrosHoras", registro.Id.ToString(),
            new { registro.Estado, registro.FechaRegistro }, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return AprobacionHelper.ToDto(registro);
    }
}

// ── Rechazar ─────────────────────────────────────────────────────────────────

[Authorize(Roles = $"{Roles.Supervisor},{Roles.Gerente},{Roles.Admin}")]
public record RechazarRegistroCommand(int RegistroHorasId, string Comentario) : IRequest<EstadoRegistroDto>;

public class RechazarRegistroCommandValidator : AbstractValidator<RechazarRegistroCommand>
{
    public RechazarRegistroCommandValidator()
    {
        RuleFor(x => x.RegistroHorasId).GreaterThan(0);
        RuleFor(x => x.Comentario)
            .NotEmpty().WithMessage("El rechazo requiere un comentario que explique el motivo.")
            .MaximumLength(1000).WithMessage("El comentario no puede superar 1000 caracteres.");
    }
}

public class RechazarRegistroCommandHandler(
    IApplicationDbContext context,
    ICadenaAprobacionService cadena,
    IBitacoraService bitacora,
    INotificadorAprobacion notificador,
    IUser actor)
    : IRequestHandler<RechazarRegistroCommand, EstadoRegistroDto>
{
    public async Task<EstadoRegistroDto> Handle(
        RechazarRegistroCommand request,
        CancellationToken cancellationToken)
    {
        var (registro, cadenaAprobacion, actorId) =
            await AprobacionHelper.CargarAsync(context, cadena, actor, request.RegistroHorasId, cancellationToken);

        var nivel = registro.NivelPendiente
            ?? throw new ValidationException([new ValidationFailure(
                nameof(request.RegistroHorasId),
                "El registro no esta pendiente de revision.")]);

        AprobacionHelper.ExigirAprobadorDelNivel(cadenaAprobacion, nivel, actorId);

        registro.Rechazar(request.Comentario);
        context.AprobacionesRegistro.Add(new AprobacionRegistro(
            registro.Id, AprobacionRegistro.AccionRechazar, nivel, actorId, request.Comentario));

        await bitacora.RegistrarAsync(
            TipoEventoBitacora.RechazoRegistro, actorId, null,
            "RegistrosHoras", registro.Id.ToString(),
            new { Nivel = nivel, registro.FechaRegistro, request.Comentario }, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        // Despues de guardar y fuera de la transaccion logica: el aviso no debe
        // condicionar el rechazo, que ya esta decidido.
        await notificador.NotificarRechazoAsync(
            registro.UserId, registro.FechaRegistro, registro.ProyectoNombre,
            request.Comentario, cancellationToken);

        return AprobacionHelper.ToDto(registro);
    }
}

// ── Revertir aprobacion ──────────────────────────────────────────────────────

[Authorize(Roles = $"{Roles.Supervisor},{Roles.Gerente},{Roles.Admin}")]
public record RevertirAprobacionCommand(int RegistroHorasId) : IRequest<EstadoRegistroDto>;

public class RevertirAprobacionCommandHandler(
    IApplicationDbContext context,
    ICadenaAprobacionService cadena,
    IBitacoraService bitacora,
    IUser actor)
    : IRequestHandler<RevertirAprobacionCommand, EstadoRegistroDto>
{
    public async Task<EstadoRegistroDto> Handle(
        RevertirAprobacionCommand request,
        CancellationToken cancellationToken)
    {
        var (registro, cadenaAprobacion, actorId) =
            await AprobacionHelper.CargarAsync(context, cadena, actor, request.RegistroHorasId, cancellationToken);

        // Solo puede deshacer quien aprobo ese nivel: el nivel a revertir es el ultimo
        // aprobado, es decir el anterior al pendiente.
        var nivelARevertir = registro.Estado switch
        {
            EstadoAprobacion.AprobadoNivel1 => 1,
            EstadoAprobacion.AprobadoNivel2 => 2,
            EstadoAprobacion.Aprobado       => 3,
            _ => throw new ValidationException([new ValidationFailure(
                nameof(request.RegistroHorasId),
                "El registro no tiene ninguna aprobacion que revertir.")])
        };

        AprobacionHelper.ExigirAprobadorDelNivel(cadenaAprobacion, nivelARevertir, actorId);

        registro.RevertirAprobacion();
        context.AprobacionesRegistro.Add(new AprobacionRegistro(
            registro.Id, AprobacionRegistro.AccionRevertirAprobado, nivelARevertir, actorId));

        await bitacora.RegistrarAsync(
            TipoEventoBitacora.ReversionAprobacion, actorId, null,
            "RegistrosHoras", registro.Id.ToString(),
            new { Nivel = nivelARevertir, registro.Estado }, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return AprobacionHelper.ToDto(registro);
    }
}

// ── Revertir rechazo ─────────────────────────────────────────────────────────

[Authorize(Roles = $"{Roles.Supervisor},{Roles.Gerente},{Roles.Admin}")]
public record RevertirRechazoCommand(int RegistroHorasId) : IRequest<EstadoRegistroDto>;

public class RevertirRechazoCommandHandler(
    IApplicationDbContext context,
    ICadenaAprobacionService cadena,
    IBitacoraService bitacora,
    IUser actor)
    : IRequestHandler<RevertirRechazoCommand, EstadoRegistroDto>
{
    public async Task<EstadoRegistroDto> Handle(
        RevertirRechazoCommand request,
        CancellationToken cancellationToken)
    {
        var (registro, cadenaAprobacion, actorId) =
            await AprobacionHelper.CargarAsync(context, cadena, actor, request.RegistroHorasId, cancellationToken);

        if (!registro.EstaRechazado)
            throw new ValidationException([new ValidationFailure(
                nameof(request.RegistroHorasId), "El registro no esta rechazado.")]);

        // Deshacer el rechazo corresponde a quien lo rechazo.
        var quienRechazo = await context.AprobacionesRegistro
            .Where(a => a.RegistroHorasId == registro.Id && a.Accion == AprobacionRegistro.AccionRechazar)
            .OrderByDescending(a => a.Id)
            .Select(a => a.ActorUserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (quienRechazo is not null && quienRechazo != actorId)
            throw new ForbiddenAccessException();

        registro.RevertirRechazo();
        context.AprobacionesRegistro.Add(new AprobacionRegistro(
            registro.Id, AprobacionRegistro.AccionRevertirRechazo, null, actorId));

        await bitacora.RegistrarAsync(
            TipoEventoBitacora.ReversionRechazo, actorId, null,
            "RegistrosHoras", registro.Id.ToString(),
            new { registro.Estado }, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return AprobacionHelper.ToDto(registro);
    }
}

// ── Reenviar (empleado) ──────────────────────────────────────────────────────

[Authorize(Roles = $"{Roles.Empleado},{Roles.Supervisor},{Roles.Gerente},{Roles.Admin}")]
public record ReenviarRegistroCommand(int RegistroHorasId) : IRequest<EstadoRegistroDto>;

public class ReenviarRegistroCommandHandler(
    IApplicationDbContext context,
    IBitacoraService bitacora,
    IUser actor)
    : IRequestHandler<ReenviarRegistroCommand, EstadoRegistroDto>
{
    public async Task<EstadoRegistroDto> Handle(
        ReenviarRegistroCommand request,
        CancellationToken cancellationToken)
    {
        var actorId = actor.Id
            ?? throw new UnauthorizedAccessException("No existe usuario autenticado.");

        var registro = await context.RegistrosHoras
            .FirstOrDefaultAsync(r => r.Id == request.RegistroHorasId, cancellationToken)
            ?? throw new NotFoundException($"Registro con id '{request.RegistroHorasId}' no fue encontrado.");

        // Solo el dueno del registro lo reenvia: es quien lo corrigio.
        if (registro.UserId != actorId)
            throw new ForbiddenAccessException();

        registro.Reenviar();
        context.AprobacionesRegistro.Add(new AprobacionRegistro(
            registro.Id, AprobacionRegistro.AccionReenviar, null, actorId));

        await bitacora.RegistrarAsync(
            TipoEventoBitacora.ReenvioRegistro, actorId, null,
            "RegistrosHoras", registro.Id.ToString(),
            new { registro.FechaRegistro }, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return AprobacionHelper.ToDto(registro);
    }
}

// ── Helper compartido ────────────────────────────────────────────────────────

internal static class AprobacionHelper
{
    public static async Task<(RegistroHorasEntity Registro, CadenaAprobacionDto Cadena, string ActorId)> CargarAsync(
        IApplicationDbContext context,
        ICadenaAprobacionService cadena,
        IUser actor,
        int registroId,
        CancellationToken cancellationToken)
    {
        var actorId = actor.Id
            ?? throw new UnauthorizedAccessException("No existe usuario autenticado.");

        var registro = await context.RegistrosHoras
            .FirstOrDefaultAsync(r => r.Id == registroId, cancellationToken)
            ?? throw new NotFoundException($"Registro con id '{registroId}' no fue encontrado.");

        var cadenaAprobacion = await cadena.ResolverAsync(
            registro.UserId, registro.ProyectoId, cancellationToken);

        return (registro, cadenaAprobacion, actorId);
    }

    public static string? AprobadorDe(CadenaAprobacionDto cadena, int nivel) => nivel switch
    {
        1 => cadena.SupervisorPuestoUserId,
        2 => cadena.SupervisorUsuarioUserId,
        3 => cadena.SupervisorProyectoUserId,
        _ => null
    };

    /// <summary>
    /// Solo el aprobador asignado a ese nivel puede actuar sobre el. Un nivel sin
    /// aprobador asignado no lo puede aprobar nadie: hay que configurarlo primero,
    /// en vez de dejar que cualquier supervisor se lo apropie.
    /// </summary>
    public static void ExigirAprobadorDelNivel(CadenaAprobacionDto cadena, int nivel, string actorId)
    {
        if (AprobadorDe(cadena, nivel) != actorId)
            throw new ForbiddenAccessException();
    }

    public static EstadoRegistroDto ToDto(RegistroHorasEntity r) =>
        new(r.Id, r.Estado, r.NivelPendiente, r.ComentarioRechazo);
}
