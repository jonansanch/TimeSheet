using KPG.Timesheet.Application.Common.Interfaces;

namespace KPG.Timesheet.Application.Features.Organizacion.SupervisoresPuesto;

public class GetSupervisoresPuestoQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSupervisoresPuestoQuery, IReadOnlyList<SupervisorPuestoDto>>
{
    public async Task<IReadOnlyList<SupervisorPuestoDto>> Handle(
        GetSupervisoresPuestoQuery request,
        CancellationToken cancellationToken)
    {
        // El nombre del supervisor no se resuelve aqui: AspNetUsers no esta en
        // IApplicationDbContext y la pantalla ya carga la lista de usuarios.
        var items = await (
            from s in context.SupervisoresPuesto
            join p in context.Empleados on s.PuestoId equals p.Id
            join c in context.Clientes on s.ClienteId equals c.Id into cs
            from c in cs.DefaultIfEmpty()
            orderby p.Nombre, c.Nombre
            select new SupervisorPuestoDto(
                s.Id,
                s.PuestoId,
                p.Nombre,
                s.ClienteId,
                c != null ? c.Nombre : null,
                s.SupervisorUserId,
                s.Activo)
        ).ToListAsync(cancellationToken);

        return items;
    }
}
