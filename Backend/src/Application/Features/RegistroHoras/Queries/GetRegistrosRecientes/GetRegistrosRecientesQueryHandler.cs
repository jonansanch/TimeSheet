using KPG.Timesheet.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Application.Features.RegistroHoras.Queries.GetRegistrosRecientes;

public class GetRegistrosRecientesQueryHandler
    : IRequestHandler<GetRegistrosRecientesQuery, IEnumerable<RegistroRecienteDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetRegistrosRecientesQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<IEnumerable<RegistroRecienteDto>> Handle(
        GetRegistrosRecientesQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _user.Id;
        if (string.IsNullOrWhiteSpace(userId))
            return [];

        var top = Math.Clamp(request.Top, 1, 10);

        // Load ordered by date desc; GroupBy in-memory preserves first-occurrence order,
        // so the first element of each group is the most recent record for that (Cliente, Proyecto).
        var records = await _context.RegistrosHoras
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.FechaRegistro)
            .ToListAsync(cancellationToken);

        return records
            .GroupBy(r => new { r.Cliente, r.Proyecto })
            .Take(top)
            .Select(g =>
            {
                var latest = g.First();
                return new RegistroRecienteDto(
                    latest.Cliente, latest.Proyecto,
                    latest.Modalidad, latest.Recurso,
                    latest.Descripcion, latest.Lugar);
            });
    }
}
