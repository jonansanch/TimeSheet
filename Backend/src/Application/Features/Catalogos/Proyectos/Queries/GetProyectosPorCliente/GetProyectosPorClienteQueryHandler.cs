using KPG.Timesheet.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Application.Features.Catalogos.Proyectos.Queries.GetProyectosPorCliente;

public class GetProyectosPorClienteQueryHandler : IRequestHandler<GetProyectosPorClienteQuery, List<ProyectoDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProyectosPorClienteQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProyectoDto>> Handle(GetProyectosPorClienteQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Proyectos
            .Where(p => p.ClienteId == request.ClienteId);

        if (request.SoloActivos)
            query = query.Where(p => p.Activo);

        return await query
            .OrderBy(p => p.Nombre)
            .Select(p => new ProyectoDto(p.Id, p.Nombre, p.ClienteId, p.Activo))
            .ToListAsync(cancellationToken);
    }
}
