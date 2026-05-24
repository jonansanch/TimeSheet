using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Application.Features.SolicitudesExcepcion.Queries.GetMisExcepcionesAprobadas;

[Authorize]
public record GetMisExcepcionesAprobadasQuery : IRequest<IEnumerable<DateOnly>>;

public class GetMisExcepcionesAprobadasQueryHandler : IRequestHandler<GetMisExcepcionesAprobadasQuery, IEnumerable<DateOnly>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetMisExcepcionesAprobadasQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<IEnumerable<DateOnly>> Handle(GetMisExcepcionesAprobadasQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_user.Id))
            return [];

        return await _context.SolicitudesExcepcion
            .Where(s => s.UserId == _user.Id && s.Estado == EstadoSolicitud.Aprobada)
            .Select(s => s.FechaRegistro)
            .ToListAsync(cancellationToken);
    }
}
