using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Application.Features.Catalogos.TerminosDescripcion.Queries.GetTerminosDescripcion;

public record TerminoDescripcionDto(
    int Id,
    string Termino,
    TipoTermino Tipo,
    ReglaTermino Regla,
    SeveridadTermino Severidad,
    string? Sugerencia,
    string Motivo,
    bool Activo);

public record GetTerminosDescripcionQuery(bool SoloActivos = false) : IRequest<List<TerminoDescripcionDto>>;

public class GetTerminosDescripcionQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetTerminosDescripcionQuery, List<TerminoDescripcionDto>>
{
    public async Task<List<TerminoDescripcionDto>> Handle(
        GetTerminosDescripcionQuery request, CancellationToken cancellationToken)
    {
        var query = context.TerminosDescripcion.AsQueryable();
        if (request.SoloActivos)
            query = query.Where(t => t.Activo);

        return await query
            .OrderBy(t => t.Termino)
            .Select(t => new TerminoDescripcionDto(
                t.Id, t.Termino, t.Tipo, t.Regla, t.Severidad, t.Sugerencia, t.Motivo, t.Activo))
            .ToListAsync(cancellationToken);
    }
}
