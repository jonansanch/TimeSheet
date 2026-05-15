using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Catalogos.Proyectos.Queries.GetProyectosPorCliente;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NotFoundException = KPG.Timesheet.Application.Common.Exceptions.NotFoundException;

namespace KPG.Timesheet.Application.Features.Catalogos.Proyectos.Commands.ToggleProyectoActivo;

public class ToggleProyectoActivoCommandHandler : IRequestHandler<ToggleProyectoActivoCommand, ProyectoDto>
{
    private readonly IApplicationDbContext _context;

    public ToggleProyectoActivoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProyectoDto> Handle(ToggleProyectoActivoCommand request, CancellationToken cancellationToken)
    {
        var proyecto = await _context.Proyectos
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException($"Proyecto con id '{request.Id}' no fue encontrado.");

        if (proyecto.Activo)
            proyecto.Desactivar();
        else
            proyecto.Activar();

        await _context.SaveChangesAsync(cancellationToken);

        return new ProyectoDto(proyecto.Id, proyecto.Nombre, proyecto.ClienteId, proyecto.Activo);
    }
}
