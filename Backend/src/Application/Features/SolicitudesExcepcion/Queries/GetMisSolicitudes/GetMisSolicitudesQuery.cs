using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Security;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Application.Features.SolicitudesExcepcion.Queries.GetMisSolicitudes;

public record MiSolicitudDto(
    int Id,
    DateOnly FechaRegistro,
    string Justificacion,
    string Estado,
    DateTimeOffset Created);

[Authorize]
public record GetMisSolicitudesQuery : IRequest<IEnumerable<MiSolicitudDto>>;

public class GetMisSolicitudesQueryHandler : IRequestHandler<GetMisSolicitudesQuery, IEnumerable<MiSolicitudDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetMisSolicitudesQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<IEnumerable<MiSolicitudDto>> Handle(GetMisSolicitudesQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_user.Id))
            return [];

        return await _context.SolicitudesExcepcion
            .Where(s => s.UserId == _user.Id)
            .OrderByDescending(s => s.Created)
            .Select(s => new MiSolicitudDto(s.Id, s.FechaRegistro, s.Justificacion, s.Estado.ToString(), s.Created))
            .ToListAsync(cancellationToken);
    }
}
