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

        // Query 1: GROUP BY en SQL — devuelve a lo sumo `top` filas.
        // El índice (UserId, FechaRegistro, Cliente, Proyecto) cubre esta query.
        var groups = await _context.RegistrosHoras
            .Where(r => r.UserId == userId)
            .GroupBy(r => new { r.Cliente, r.Proyecto })
            .Select(g => new
            {
                g.Key.Cliente,
                g.Key.Proyecto,
                UltimaFecha = g.Max(r => r.FechaRegistro)
            })
            .OrderByDescending(g => g.UltimaFecha)
            .Take(top)
            .ToListAsync(cancellationToken);

        if (groups.Count == 0)
            return [];

        // Query 2: fetch de datos completos para esos registros en una sola consulta.
        // Máximo `top` filas — nunca carga toda la tabla del usuario.
        var clientes  = groups.Select(g => g.Cliente).ToList();
        var proyectos = groups.Select(g => g.Proyecto).ToList();
        var fechas    = groups.Select(g => g.UltimaFecha).ToList();

        var records = await _context.RegistrosHoras
            .Where(r => r.UserId   == userId
                     && clientes.Contains(r.Cliente)
                     && proyectos.Contains(r.Proyecto)
                     && fechas.Contains(r.FechaRegistro))
            .Select(r => new
            {
                r.Cliente, r.Proyecto, r.FechaRegistro,
                r.Modalidad, r.Recurso, r.Descripcion, r.Lugar
            })
            .ToListAsync(cancellationToken);

        // Join en memoria preservando el orden más-reciente-primero (a lo sumo `top` items).
        return groups.Select(g =>
        {
            var r = records.First(r => r.Cliente      == g.Cliente
                                    && r.Proyecto     == g.Proyecto
                                    && r.FechaRegistro == g.UltimaFecha);
            return new RegistroRecienteDto(
                r.Cliente, r.Proyecto, r.Modalidad, r.Recurso, r.Descripcion, r.Lugar);
        });
    }
}
