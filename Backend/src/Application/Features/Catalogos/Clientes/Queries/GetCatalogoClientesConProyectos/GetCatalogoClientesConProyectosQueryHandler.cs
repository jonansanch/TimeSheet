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
        var clientes = await _context.Clientes
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .ToListAsync(cancellationToken);

        var clienteIds = clientes.Select(c => c.Id).ToList();

        var proyectos = await _context.Proyectos
            .Where(p => clienteIds.Contains(p.ClienteId) && p.Activo)
            .OrderBy(p => p.Nombre)
            .ToListAsync(cancellationToken);

        return clientes.Select(c => new ClienteConProyectosDto(
            c.Id,
            c.Nombre,
            proyectos
                .Where(p => p.ClienteId == c.Id)
                .Select(p => p.Nombre)
                .ToList()
        )).ToList();
    }
}
