using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Domain.Entities;
using NotFoundException = KPG.Timesheet.Application.Common.Exceptions.NotFoundException;

namespace KPG.Timesheet.Application.Features.SolicitudesExcepcion.Commands.AprobarSolicitudExcepcion;

public class AprobarSolicitudExcepcionCommandHandler : IRequestHandler<AprobarSolicitudExcepcionCommand>
{
    private readonly IApplicationDbContext _context;

    public AprobarSolicitudExcepcionCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task Handle(AprobarSolicitudExcepcionCommand request, CancellationToken cancellationToken)
    {
        var solicitud = await _context.SolicitudesExcepcion
            .FindAsync([request.Id], cancellationToken);

        if (solicitud is null)
            throw new NotFoundException($"SolicitudExcepcion con Id {request.Id} no encontrada.");

        solicitud.Aprobar();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
