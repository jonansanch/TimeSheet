using FluentValidation.Results;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Catalogos.Proyectos.Queries.GetProyectosPorCliente;
using KPG.Timesheet.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NotFoundException = KPG.Timesheet.Application.Common.Exceptions.NotFoundException;
using ValidationException = KPG.Timesheet.Application.Common.Exceptions.ValidationException;

namespace KPG.Timesheet.Application.Features.Catalogos.Proyectos.Commands.CreateProyecto;

public class CreateProyectoCommandHandler : IRequestHandler<CreateProyectoCommand, ProyectoDto>
{
    private readonly IApplicationDbContext _context;

    public CreateProyectoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProyectoDto> Handle(CreateProyectoCommand request, CancellationToken cancellationToken)
    {
        var clienteExiste = await _context.Clientes
            .AnyAsync(c => c.Id == request.ClienteId, cancellationToken);

        if (!clienteExiste)
            throw new NotFoundException($"Cliente con id '{request.ClienteId}' no fue encontrado.");

        var proyecto = new Proyecto(request.ClienteId, request.Nombre);
        _context.Proyectos.Add(proyecto);

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
