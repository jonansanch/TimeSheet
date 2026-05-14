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

        return await _context.RegistrosHoras
            .Where(r => r.UserId == userId)
            .GroupBy(r => new { r.Cliente, r.Proyecto })
            .Select(g => new
            {
                g.Key.Cliente,
                g.Key.Proyecto,
                UltimaFecha = g.Max(r => r.FechaRegistro)
            })
            .OrderByDescending(x => x.UltimaFecha)
            .Take(top)
            .Select(x => new RegistroRecienteDto(x.Cliente, x.Proyecto))
            .ToListAsync(cancellationToken);
    }
}
