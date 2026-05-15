using KPG.Timesheet.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Application.Features.Catalogos.LugaresTrabajo.Queries.GetLugaresTrabajo;

public record LugarTrabajoDto(int Id, string Nombre, bool Activo);

public record GetLugaresTrabajosQuery(bool SoloActivos = false) : IRequest<List<LugarTrabajoDto>>;

public class GetLugaresTrabajosQueryHandler : IRequestHandler<GetLugaresTrabajosQuery, List<LugarTrabajoDto>>
{
    private readonly IApplicationDbContext _context;

    public GetLugaresTrabajosQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<LugarTrabajoDto>> Handle(GetLugaresTrabajosQuery request, CancellationToken cancellationToken)
    {
        var query = _context.LugaresTrabajo.AsQueryable();
        if (request.SoloActivos)
            query = query.Where(l => l.Activo);

        return await query
            .OrderBy(l => l.Nombre)
            .Select(l => new LugarTrabajoDto(l.Id, l.Nombre, l.Activo))
            .ToListAsync(cancellationToken);
    }
}
