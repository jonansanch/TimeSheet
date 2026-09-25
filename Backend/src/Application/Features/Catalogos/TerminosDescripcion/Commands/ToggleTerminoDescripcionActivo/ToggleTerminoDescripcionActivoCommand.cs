using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Catalogos.TerminosDescripcion.Commands.CreateTerminoDescripcion;
using KPG.Timesheet.Application.Features.Catalogos.TerminosDescripcion.Queries.GetTerminosDescripcion;
using KPG.Timesheet.Domain.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NotFoundException = KPG.Timesheet.Application.Common.Exceptions.NotFoundException;

namespace KPG.Timesheet.Application.Features.Catalogos.TerminosDescripcion.Commands.ToggleTerminoDescripcionActivo;

public record ToggleTerminoDescripcionActivoCommand(int Id) : IRequest<TerminoDescripcionDto>;

public class ToggleTerminoDescripcionActivoCommandHandler(IApplicationDbContext context, IBitacoraService bitacora, IUser actor)
    : IRequestHandler<ToggleTerminoDescripcionActivoCommand, TerminoDescripcionDto>
{
    public async Task<TerminoDescripcionDto> Handle(
        ToggleTerminoDescripcionActivoCommand request, CancellationToken cancellationToken)
    {
        var actorId = actor.Id
            ?? throw new UnauthorizedAccessException("No existe usuario autenticado.");

        var termino = await context.TerminosDescripcion
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException($"TerminoDescripcion con id '{request.Id}' no fue encontrado.");

        if (termino.Activo)
            termino.Desactivar();
        else
            termino.Activar();

        await bitacora.RegistrarAsync(
            TipoEventoBitacora.TerminoDescripcionActivado,
            actorId, actor.Email,
            "TerminosDescripcion", termino.Id.ToString(),
            new { termino.Termino, termino.Activo },
            cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return CreateTerminoDescripcionCommandHandler.ToDto(termino);
    }
}
