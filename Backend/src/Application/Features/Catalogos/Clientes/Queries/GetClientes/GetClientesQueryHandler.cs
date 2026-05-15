using KPG.Timesheet.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Application.Features.Catalogos.Clientes.Queries.GetClientes;

public class GetClientesQueryHandler : IRequestHandler<GetClientesQuery, List<ClienteDto>>
{
    private readonly IApplicationDbContext _context;

    public GetClientesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ClienteDto>> Handle(GetClientesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Clientes.AsQueryable();

        if (request.SoloActivos)
            query = query.Where(c => c.Activo);

        return await query
            .OrderBy(c => c.Nombre)
            .Select(c => new ClienteDto(c.Id, c.Nombre, c.Activo))
            .ToListAsync(cancellationToken);
    }
}
