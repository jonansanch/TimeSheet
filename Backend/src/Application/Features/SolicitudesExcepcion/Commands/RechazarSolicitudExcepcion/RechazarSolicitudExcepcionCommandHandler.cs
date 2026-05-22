using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Domain.Entities;
using NotFoundException = KPG.Timesheet.Application.Common.Exceptions.NotFoundException;

namespace KPG.Timesheet.Application.Features.SolicitudesExcepcion.Commands.RechazarSolicitudExcepcion;

public class RechazarSolicitudExcepcionCommandHandler : IRequestHandler<RechazarSolicitudExcepcionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IBitacoraService _bitacora;
    private readonly IUser _user;

    public RechazarSolicitudExcepcionCommandHandler(IApplicationDbContext context, IBitacoraService bitacora, IUser user)
    {
        _context = context;
        _bitacora = bitacora;
        _user = user;
    }

    public async Task Handle(RechazarSolicitudExcepcionCommand request, CancellationToken cancellationToken)
    {
        var solicitud = await _context.SolicitudesExcepcion
            .FindAsync([request.Id], cancellationToken);

        if (solicitud is null)
            throw new NotFoundException($"SolicitudExcepcion con Id {request.Id} no encontrada.");

        solicitud.Rechazar();
        await _bitacora.RegistrarAsync(
            TipoEventoBitacora.RechazoExcepcion,
            _user.Id ?? "system", null,
            "SolicitudesExcepcion", request.Id.ToString(),
            new { solicitud.UserId, Fecha = solicitud.FechaRegistro },
            cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
