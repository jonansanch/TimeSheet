using KPG.Timesheet.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Application.Features.Catalogos.Modalidades.Queries.GetModalidades;

public record ModalidadDto(int Id, string Nombre, bool Activo);

public record GetModalidadesQuery(bool SoloActivas = false) : IRequest<List<ModalidadDto>>;

public class GetModalidadesQueryHandler : IRequestHandler<GetModalidadesQuery, List<ModalidadDto>>
{
    private readonly IApplicationDbContext _context;

    public GetModalidadesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<ModalidadDto>> Handle(GetModalidadesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Modalidades.AsQueryable();
        if (request.SoloActivas)
            query = query.Where(m => m.Activo);

        return await query
            .OrderBy(m => m.Nombre)
            .Select(m => new ModalidadDto(m.Id, m.Nombre, m.Activo))
            .ToListAsync(cancellationToken);
    }
}
