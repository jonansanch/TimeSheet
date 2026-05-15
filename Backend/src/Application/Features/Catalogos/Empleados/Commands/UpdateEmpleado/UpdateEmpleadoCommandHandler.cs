using FluentValidation.Results;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Catalogos.Empleados.Queries.GetEmpleados;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NotFoundException = KPG.Timesheet.Application.Common.Exceptions.NotFoundException;
using ValidationException = KPG.Timesheet.Application.Common.Exceptions.ValidationException;

namespace KPG.Timesheet.Application.Features.Catalogos.Empleados.Commands.UpdateEmpleado;

public class UpdateEmpleadoCommandHandler : IRequestHandler<UpdateEmpleadoCommand, EmpleadoDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateEmpleadoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmpleadoDto> Handle(UpdateEmpleadoCommand request, CancellationToken cancellationToken)
    {
        var empleado = await _context.Empleados
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException($"Empleado con id '{request.Id}' no fue encontrado.");

        empleado.ActualizarNombre(request.Nombre);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (EsDuplicado(ex))
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(request.Nombre), "Ya existe un empleado con ese nombre.")
            ]);
        }

        return new EmpleadoDto(empleado.Id, empleado.Nombre, empleado.Activo);
    }

    private static bool EsDuplicado(DbUpdateException ex)
    {
        var mensaje = ex.InnerException?.Message ?? string.Empty;
        return mensaje.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
            || mensaje.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
            || mensaje.Contains("IX_Empleados_Nombre", StringComparison.OrdinalIgnoreCase);
    }
}
