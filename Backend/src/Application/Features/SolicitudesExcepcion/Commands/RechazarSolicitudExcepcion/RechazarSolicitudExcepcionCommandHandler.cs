using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Domain.Entities;
using NotFoundException = KPG.Timesheet.Application.Common.Exceptions.NotFoundException;

namespace KPG.Timesheet.Application.Features.SolicitudesExcepcion.Commands.RechazarSolicitudExcepcion;

public class RechazarSolicitudExcepcionCommandHandler : IRequestHandler<RechazarSolicitudExcepcionCommand>
{
    private readonly IApplicationDbContext _context;

    public RechazarSolicitudExcepcionCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task Handle(RechazarSolicitudExcepcionCommand request, CancellationToken cancellationToken)
    {
        var solicitud = await _context.SolicitudesExcepcion
            .FindAsync([request.Id], cancellationToken);

        if (solicitud is null)
            throw new NotFoundException($"SolicitudExcepcion con Id {request.Id} no encontrada.");

        solicitud.Rechazar();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
