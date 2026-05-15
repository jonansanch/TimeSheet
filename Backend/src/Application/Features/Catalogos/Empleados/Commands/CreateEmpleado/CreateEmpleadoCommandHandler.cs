using FluentValidation.Results;
using KPG.Timesheet.Application.Common.Exceptions;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Catalogos.Empleados.Queries.GetEmpleados;
using KPG.Timesheet.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ValidationException = KPG.Timesheet.Application.Common.Exceptions.ValidationException;

namespace KPG.Timesheet.Application.Features.Catalogos.Empleados.Commands.CreateEmpleado;

public class CreateEmpleadoCommandHandler : IRequestHandler<CreateEmpleadoCommand, EmpleadoDto>
{
    private readonly IApplicationDbContext _context;

    public CreateEmpleadoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmpleadoDto> Handle(CreateEmpleadoCommand request, CancellationToken cancellationToken)
    {
        var empleado = new Empleado(request.Nombre);
        _context.Empleados.Add(empleado);

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
