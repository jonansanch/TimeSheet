using FluentValidation.Results;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Catalogos.Proyectos.Queries.GetProyectosPorCliente;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NotFoundException = KPG.Timesheet.Application.Common.Exceptions.NotFoundException;
using ValidationException = KPG.Timesheet.Application.Common.Exceptions.ValidationException;

namespace KPG.Timesheet.Application.Features.Catalogos.Proyectos.Commands.UpdateProyecto;

public class UpdateProyectoCommandHandler : IRequestHandler<UpdateProyectoCommand, ProyectoDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateProyectoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProyectoDto> Handle(UpdateProyectoCommand request, CancellationToken cancellationToken)
    {
        var proyecto = await _context.Proyectos
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException($"Proyecto con id '{request.Id}' no fue encontrado.");

        proyecto.ActualizarNombre(request.Nombre);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (EsDuplicado(ex))
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(request.Nombre), "Ya existe un proyecto con ese nombre para este cliente.")
            ]);
        }

        return new ProyectoDto(proyecto.Id, proyecto.Nombre, proyecto.ClienteId, proyecto.Activo);
    }

    private static bool EsDuplicado(DbUpdateException ex)
    {
        var mensaje = ex.InnerException?.Message ?? string.Empty;
        return mensaje.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
            || mensaje.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
            || mensaje.Contains("IX_Proyectos_ClienteId_Nombre", StringComparison.OrdinalIgnoreCase);
    }
}
