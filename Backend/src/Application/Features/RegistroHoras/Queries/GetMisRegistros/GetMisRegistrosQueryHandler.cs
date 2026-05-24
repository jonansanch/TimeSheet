using KPG.Timesheet.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Application.Features.RegistroHoras.Queries.GetMisRegistros;

public class GetMisRegistrosQueryHandler : IRequestHandler<GetMisRegistrosQuery, MisRegistrosPaginadosResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetMisRegistrosQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user    = user;
    }

    public async Task<MisRegistrosPaginadosResponse> Handle(
        GetMisRegistrosQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _user.Id;
        if (string.IsNullOrWhiteSpace(userId))
            return new MisRegistrosPaginadosResponse(0, []);

        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var skip     = (Math.Max(1, request.Page) - 1) * pageSize;

        var baseQuery = _context.RegistrosHoras
            .Where(r => r.UserId == userId
                && (request.Desde == null || r.FechaRegistro >= request.Desde)
                && (request.Hasta == null || r.FechaRegistro <= request.Hasta));

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .OrderByDescending(r => r.FechaRegistro)
            .Skip(skip)
            .Take(pageSize)
            .Select(r => new MisRegistrosItemDto(
                r.Id,
                r.FechaRegistro,
                r.HoraEntradaAM,
                r.HoraSalidaAM,
                r.HoraEntradaPM,
                r.HoraSalidaPM,
                r.Cliente,
                r.Proyecto,
                r.Modalidad,
                r.Descripcion))
            .ToListAsync(cancellationToken);

        return new MisRegistrosPaginadosResponse(totalCount, items);
    }
}
