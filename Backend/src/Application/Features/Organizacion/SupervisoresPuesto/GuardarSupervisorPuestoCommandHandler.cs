using FluentValidation.Results;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Domain.Constants;
using SupervisorPuestoEntity = KPG.Timesheet.Domain.Entities.SupervisorPuesto;
using NotFoundException = KPG.Timesheet.Application.Common.Exceptions.NotFoundException;
using ValidationException = KPG.Timesheet.Application.Common.Exceptions.ValidationException;

namespace KPG.Timesheet.Application.Features.Organizacion.SupervisoresPuesto;

public class GuardarSupervisorPuestoCommandHandler(
    IApplicationDbContext context,
    IBitacoraService bitacora,
    IUser actor)
    : IRequestHandler<GuardarSupervisorPuestoCommand, SupervisorPuestoDto>
{
    public async Task<SupervisorPuestoDto> Handle(
        GuardarSupervisorPuestoCommand request,
        CancellationToken cancellationToken)
    {
        var puesto = await context.Empleados
            .FirstOrDefaultAsync(e => e.Id == request.PuestoId && e.Activo, cancellationToken)
            ?? throw new ValidationException([
                new ValidationFailure(nameof(request.PuestoId), "El puesto indicado no existe o esta inactivo.")]);

        if (request.ClienteId.HasValue &&
            !await context.Clientes.AnyAsync(c => c.Id == request.ClienteId && c.Activo, cancellationToken))
        {
            throw new ValidationException([
                new ValidationFailure(nameof(request.ClienteId), "El cliente indicado no existe o esta inactivo.")]);
        }

        // Una sola regla por (puesto, cliente): la BD tambien lo impide con indices unicos
        // filtrados, pero aqui el mensaje es entendible en lugar de un error de SQL.
        var duplicada = await context.SupervisoresPuesto.AnyAsync(
            s => s.PuestoId == request.PuestoId
              && s.ClienteId == request.ClienteId
              && (request.Id == null || s.Id != request.Id),
            cancellationToken);

        if (duplicada)
        {
            throw new ValidationException([
                new ValidationFailure(nameof(request.PuestoId),
                    request.ClienteId.HasValue
                        ? "Ya existe una regla para ese puesto en ese cliente."
                        : "Ya existe una regla general para ese puesto.")]);
        }

        SupervisorPuestoEntity regla;

        if (request.Id is null)
        {
            regla = new SupervisorPuestoEntity(request.PuestoId, request.SupervisorUserId, request.ClienteId);
            context.SupervisoresPuesto.Add(regla);
        }
        else
        {
            regla = await context.SupervisoresPuesto
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException($"Regla de supervisor con id '{request.Id}' no fue encontrada.");

            regla.CambiarSupervisor(request.SupervisorUserId);
        }

        await bitacora.RegistrarAsync(
            TipoEventoBitacora.CambioSupervisorPuesto,
            actor.Id ?? "system", null,
            "SupervisoresPuesto", request.Id?.ToString(),
            new { request.PuestoId, request.ClienteId, request.SupervisorUserId },
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        var clienteNombre = request.ClienteId is null
            ? null
            : await context.Clientes.Where(c => c.Id == request.ClienteId)
                .Select(c => c.Nombre).FirstOrDefaultAsync(cancellationToken);

        return new SupervisorPuestoDto(
            regla.Id, regla.PuestoId, puesto.Nombre,
            regla.ClienteId, clienteNombre, regla.SupervisorUserId, regla.Activo);
    }
}

public class ToggleSupervisorPuestoCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ToggleSupervisorPuestoCommand, SupervisorPuestoDto>
{
    public async Task<SupervisorPuestoDto> Handle(
        ToggleSupervisorPuestoCommand request,
        CancellationToken cancellationToken)
    {
        var regla = await context.SupervisoresPuesto
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException($"Regla de supervisor con id '{request.Id}' no fue encontrada.");

        if (regla.Activo) regla.Desactivar(); else regla.Activar();
        await context.SaveChangesAsync(cancellationToken);

        var puestoNombre = await context.Empleados.Where(e => e.Id == regla.PuestoId)
            .Select(e => e.Nombre).FirstAsync(cancellationToken);
        var clienteNombre = regla.ClienteId is null
            ? null
            : await context.Clientes.Where(c => c.Id == regla.ClienteId)
                .Select(c => c.Nombre).FirstOrDefaultAsync(cancellationToken);

        return new SupervisorPuestoDto(
            regla.Id, regla.PuestoId, puestoNombre,
            regla.ClienteId, clienteNombre, regla.SupervisorUserId, regla.Activo);
    }
}
