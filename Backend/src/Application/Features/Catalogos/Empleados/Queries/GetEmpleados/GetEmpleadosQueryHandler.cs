using KPG.Timesheet.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Application.Features.Catalogos.Empleados.Queries.GetEmpleados;

public class GetEmpleadosQueryHandler : IRequestHandler<GetEmpleadosQuery, List<EmpleadoDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEmpleadosQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmpleadoDto>> Handle(GetEmpleadosQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Empleados.AsQueryable();

        if (request.SoloActivos)
            query = query.Where(e => e.Activo);

        return await query
            .OrderBy(e => e.Nombre)
            .Select(e => new EmpleadoDto(e.Id, e.Nombre, e.Activo))
            .ToListAsync(cancellationToken);
    }
}
