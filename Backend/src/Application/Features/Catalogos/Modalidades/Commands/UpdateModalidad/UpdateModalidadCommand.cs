using FluentValidation;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Catalogos.Modalidades.Queries.GetModalidades;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NotFoundException = KPG.Timesheet.Application.Common.Exceptions.NotFoundException;

namespace KPG.Timesheet.Application.Features.Catalogos.Modalidades.Commands.UpdateModalidad;

public record UpdateModalidadCommand(int Id, string Nombre) : IRequest<ModalidadDto>;

public class UpdateModalidadCommandValidator : AbstractValidator<UpdateModalidadCommand>
{
    public UpdateModalidadCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(100);
    }
}

public class UpdateModalidadCommandHandler : IRequestHandler<UpdateModalidadCommand, ModalidadDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateModalidadCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<ModalidadDto> Handle(UpdateModalidadCommand request, CancellationToken cancellationToken)
    {
        var modalidad = await _context.Modalidades
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException($"Modalidad con id '{request.Id}' no fue encontrada.");

        modalidad.ActualizarNombre(request.Nombre);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (EsDuplicado(ex))
        {
            throw new ValidationException([new FluentValidation.Results.ValidationFailure(
                nameof(request.Nombre), "Ya existe una modalidad con ese nombre.")]);
        }

        return new ModalidadDto(modalidad.Id, modalidad.Nombre, modalidad.Activo);
    }

    private static bool EsDuplicado(DbUpdateException ex)
    {
        var msg = ex.InnerException?.Message ?? string.Empty;
        return msg.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("IX_Modalidades_Nombre", StringComparison.OrdinalIgnoreCase);
    }
}
