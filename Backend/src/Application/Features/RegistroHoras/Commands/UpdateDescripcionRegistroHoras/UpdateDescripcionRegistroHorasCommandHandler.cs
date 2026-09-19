using FluentValidation.Results;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using NotFoundException = KPG.Timesheet.Application.Common.Exceptions.NotFoundException;
using ForbiddenAccessException = KPG.Timesheet.Application.Common.Exceptions.ForbiddenAccessException;
using ValidationException = KPG.Timesheet.Application.Common.Exceptions.ValidationException;

namespace KPG.Timesheet.Application.Features.RegistroHoras.Commands.UpdateDescripcionRegistroHoras;

public class UpdateDescripcionRegistroHorasCommandHandler(
    IApplicationDbContext context,
    ICadenaAprobacionService cadena,
    IBitacoraService bitacora,
    IUser user)
    : IRequestHandler<UpdateDescripcionRegistroHorasCommand>
{
    public async Task Handle(UpdateDescripcionRegistroHorasCommand request, CancellationToken cancellationToken)
    {
        var actorId = user.Id
            ?? throw new UnauthorizedAccessException("No existe usuario autenticado.");

        var registro = await context.RegistrosHoras
            .FirstOrDefaultAsync(r => r.Id == request.RegistroId, cancellationToken)
            ?? throw new NotFoundException($"RegistroHoras con id '{request.RegistroId}' no fue encontrado.");

        // Editar un registro ya aprobado invalidaria en silencio las tres aprobaciones.
        // Para corregirlo hay que revertir primero, que deja rastro.
        if (registro.Estado == EstadoAprobacion.Aprobado)
            throw new ValidationException([new ValidationFailure(
                nameof(request.RegistroId),
                "El registro esta aprobado: revierte la aprobacion antes de editarlo.")]);

        await ExigirSupervisorDelRegistroAsync(registro.UserId, registro.ProyectoId, actorId, cancellationToken);

        registro.UpdateDescripcion(request.Descripcion);

        // Los metadatos se actualizan juntos: si viene alguno, el resto conserva su valor.
        if (request.Modalidad is not null || request.Recurso is not null || request.Lugar is not null)
        {
            registro.UpdateMetadata(
                request.Modalidad ?? registro.Modalidad,
                request.Recurso   ?? registro.Recurso,
                request.Lugar     ?? registro.Lugar);
        }

        await bitacora.RegistrarAsync(
            TipoEventoBitacora.ModificacionDescripcion,
            actorId, null,
            "RegistrosHoras", request.RegistroId.ToString(),
            new { OwnerUserId = registro.UserId, registro.FechaRegistro },
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Puede editar cualquiera de los tres supervisores del registro: el del puesto, el
    /// jefe directo del empleado o el responsable del proyecto. Un supervisor ajeno no,
    /// aunque tenga el rol: el rol habilita la pantalla, no el acceso a cualquier registro.
    /// Admin queda exento porque ya puede eliminar registros.
    /// </summary>
    private async Task ExigirSupervisorDelRegistroAsync(
        string empleadoUserId,
        int proyectoId,
        string actorId,
        CancellationToken cancellationToken)
    {
        if (user.Roles?.Contains(Roles.Admin) == true) return;

        var cadenaAprobacion = await cadena.ResolverAsync(empleadoUserId, proyectoId, cancellationToken);

        var esSupervisorDelRegistro =
            cadenaAprobacion.SupervisorPuestoUserId   == actorId ||
            cadenaAprobacion.SupervisorUsuarioUserId  == actorId ||
            cadenaAprobacion.SupervisorProyectoUserId == actorId;

        if (!esSupervisorDelRegistro)
            throw new ForbiddenAccessException();
    }
}
