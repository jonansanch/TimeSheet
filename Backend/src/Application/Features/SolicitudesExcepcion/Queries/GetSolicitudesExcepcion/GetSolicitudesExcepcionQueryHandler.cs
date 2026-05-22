using KPG.Timesheet.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Application.Features.SolicitudesExcepcion.Queries.GetSolicitudesExcepcion;

public class GetSolicitudesExcepcionQueryHandler
    : IRequestHandler<GetSolicitudesExcepcionQuery, IEnumerable<SolicitudExcepcionAdminDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public GetSolicitudesExcepcionQueryHandler(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<IEnumerable<SolicitudExcepcionAdminDto>> Handle(
        GetSolicitudesExcepcionQuery request,
        CancellationToken cancellationToken)
    {
        var solicitudes = await _context.SolicitudesExcepcion
            .OrderByDescending(s => s.Created)
            .ToListAsync(cancellationToken);

        var userIds = solicitudes.Select(s => s.UserId).Distinct();
        var emails = await _identityService.GetUserEmailsAsync(userIds, cancellationToken);

        return solicitudes.Select(s => new SolicitudExcepcionAdminDto(
            s.Id, s.UserId,
            emails.GetValueOrDefault(s.UserId, s.UserId),
            s.FechaRegistro, s.Justificacion,
            s.Estado.ToString(), s.Created));
    }
}
