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

        // Se muestran los nombres guardados en el registro (foto del momento), no los
        // actuales del catalogo: el historial debe reflejar como se llamaban entonces.
        var items = await baseQuery
            .OrderByDescending(r => r.FechaRegistro)
            .Skip(skip)
            .Take(pageSize)
            .Select(r => new MisRegistrosItemDto(
                r.Id,
                r.FechaRegistro,
                r.HoraEntrada1,
                r.HoraSalida1,
                r.HoraEntrada2,
                r.HoraSalida2,
                r.HoraEntrada3,
                r.HoraSalida3,
                r.ProyectoId,
                r.ClienteNombre,
                r.ProyectoNombre,
                r.Modalidad,
                r.Recurso,
                r.Descripcion))
            .ToListAsync(cancellationToken);

        return new MisRegistrosPaginadosResponse(totalCount, items);
    }
}
