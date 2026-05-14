using KPG.Timesheet.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Application.Features.RegistroHoras.Queries.GetMisRegistros;

public class GetMisRegistrosQueryHandler : IRequestHandler<GetMisRegistrosQuery, IEnumerable<MisRegistrosItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetMisRegistrosQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<IEnumerable<MisRegistrosItemDto>> Handle(
        GetMisRegistrosQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _user.Id;
        if (string.IsNullOrWhiteSpace(userId))
            return [];

        return await _context.RegistrosHoras
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.FechaRegistro)
            .ThenBy(r => r.Turno)
            .Select(r => new MisRegistrosItemDto(
                r.Id,
                r.FechaRegistro,
                r.Turno,
                r.HoraEntrada,
                r.HoraSalida,
                r.Cliente,
                r.Proyecto,
                r.Modalidad,
                r.Descripcion))
            .ToListAsync(cancellationToken);
    }
}
