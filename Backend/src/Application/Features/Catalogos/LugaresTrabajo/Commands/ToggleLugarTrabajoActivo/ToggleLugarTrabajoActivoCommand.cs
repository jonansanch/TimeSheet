using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Catalogos.LugaresTrabajo.Queries.GetLugaresTrabajo;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NotFoundException = KPG.Timesheet.Application.Common.Exceptions.NotFoundException;

namespace KPG.Timesheet.Application.Features.Catalogos.LugaresTrabajo.Commands.ToggleLugarTrabajoActivo;

public record ToggleLugarTrabajoActivoCommand(int Id) : IRequest<LugarTrabajoDto>;

public class ToggleLugarTrabajoActivoCommandHandler : IRequestHandler<ToggleLugarTrabajoActivoCommand, LugarTrabajoDto>
{
    private readonly IApplicationDbContext _context;

    public ToggleLugarTrabajoActivoCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<LugarTrabajoDto> Handle(ToggleLugarTrabajoActivoCommand request, CancellationToken cancellationToken)
    {
        var lugar = await _context.LugaresTrabajo
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException($"Lugar de trabajo con id '{request.Id}' no fue encontrado.");

        if (lugar.Activo)
            lugar.Desactivar();
        else
            lugar.Activar();

        await _context.SaveChangesAsync(cancellationToken);

        return new LugarTrabajoDto(lugar.Id, lugar.Nombre, lugar.Activo);
    }
}
