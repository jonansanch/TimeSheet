using KPG.Timesheet.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Application.Features.Catalogos.Clientes.Queries.GetCatalogoClientesConProyectos;

public class GetCatalogoClientesConProyectosQueryHandler
    : IRequestHandler<GetCatalogoClientesConProyectosQuery, List<ClienteConProyectosDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCatalogoClientesConProyectosQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ClienteConProyectosDto>> Handle(
        GetCatalogoClientesConProyectosQuery request, CancellationToken cancellationToken)
    {
        var flat = await (
            from c in _context.Clientes
            where c.Activo
            join p in _context.Proyectos.Where(p => p.Activo)
                on c.Id equals p.ClienteId into ps
            from p in ps.DefaultIfEmpty()
            select new { ClienteId = c.Id, ClienteNombre = c.Nombre, ProyectoNombre = (string?)p.Nombre }
        ).OrderBy(x => x.ClienteNombre).ThenBy(x => x.ProyectoNombre)
         .ToListAsync(cancellationToken);

        return flat
            .GroupBy(x => new { x.ClienteId, x.ClienteNombre })
            .Select(g => new ClienteConProyectosDto(
                g.Key.ClienteId,
                g.Key.ClienteNombre,
                g.Where(x => x.ProyectoNombre != null).Select(x => x.ProyectoNombre!).ToList()))
            .ToList();
    }
}
