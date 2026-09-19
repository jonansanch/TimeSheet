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

        // Con ProyectoId el agrupamiento es por una sola clave, asi que basta una query:
        // antes hacian falta dos y un join en memoria para casar (Cliente, Proyecto, Fecha).
        // El indice (UserId, FechaRegistro, ProyectoId) cubre este GROUP BY.
        var ultimos = await _context.RegistrosHoras
            .Where(r => r.UserId == userId)
            .GroupBy(r => r.ProyectoId)
            .Select(g => new
            {
                ProyectoId  = g.Key,
                UltimaFecha = g.Max(r => r.FechaRegistro)
            })
            .OrderByDescending(g => g.UltimaFecha)
            .Take(top)
            .ToListAsync(cancellationToken);

        if (ultimos.Count == 0)
            return [];

        var proyectoIds = ultimos.Select(u => u.ProyectoId).ToList();
        var fechas      = ultimos.Select(u => u.UltimaFecha).ToList();

        var registros = await _context.RegistrosHoras
            .Where(r => r.UserId == userId
                     && proyectoIds.Contains(r.ProyectoId)
                     && fechas.Contains(r.FechaRegistro))
            .Select(r => new
            {
                r.ProyectoId,
                r.FechaRegistro,
                r.ClienteNombre,
                r.ProyectoNombre,
                r.Modalidad,
                r.Recurso,
                r.Descripcion,
                r.Lugar
            })
            .ToListAsync(cancellationToken);

        // Join en memoria preservando el orden más-reciente-primero (a lo sumo `top` items).
        return ultimos
            .Select(u => registros.FirstOrDefault(
                r => r.ProyectoId == u.ProyectoId && r.FechaRegistro == u.UltimaFecha))
            .Where(r => r is not null)
            .Select(r => new RegistroRecienteDto(
                r!.ProyectoId, r.ClienteNombre, r.ProyectoNombre,
                r.Modalidad, r.Recurso, r.Descripcion, r.Lugar))
            .ToList();
    }
}
