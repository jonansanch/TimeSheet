using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Catalogos.Empleados.Queries.GetEmpleados;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NotFoundException = KPG.Timesheet.Application.Common.Exceptions.NotFoundException;

namespace KPG.Timesheet.Application.Features.Catalogos.Empleados.Commands.ToggleEmpleadoActivo;

public class ToggleEmpleadoActivoCommandHandler : IRequestHandler<ToggleEmpleadoActivoCommand, EmpleadoDto>
{
    private readonly IApplicationDbContext _context;

    public ToggleEmpleadoActivoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmpleadoDto> Handle(ToggleEmpleadoActivoCommand request, CancellationToken cancellationToken)
    {
        var empleado = await _context.Empleados
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException($"Empleado con id '{request.Id}' no fue encontrado.");

        if (empleado.Activo)
            empleado.Desactivar();
        else
            empleado.Activar();

        await _context.SaveChangesAsync(cancellationToken);

        return new EmpleadoDto(empleado.Id, empleado.Nombre, empleado.Activo);
    }
}
