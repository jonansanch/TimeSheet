using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Domain.Entities;

namespace KPG.Timesheet.Application.Features.SolicitudesExcepcion.Commands.CreateSolicitudExcepcion;

public class CreateSolicitudExcepcionCommandHandler : IRequestHandler<CreateSolicitudExcepcionCommand, SolicitudExcepcionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CreateSolicitudExcepcionCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<SolicitudExcepcionDto> Handle(CreateSolicitudExcepcionCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.Id;
        if (string.IsNullOrWhiteSpace(userId))
            throw new UnauthorizedAccessException("No existe usuario autenticado para asociar la solicitud.");

        var solicitud = new SolicitudExcepcion(userId, request.FechaRegistro, request.Justificacion);
        _context.SolicitudesExcepcion.Add(solicitud);
        await _context.SaveChangesAsync(cancellationToken);

        return new SolicitudExcepcionDto(
            solicitud.Id,
            solicitud.UserId,
            solicitud.FechaRegistro,
            solicitud.Justificacion,
            solicitud.Estado.ToString());
    }
}
