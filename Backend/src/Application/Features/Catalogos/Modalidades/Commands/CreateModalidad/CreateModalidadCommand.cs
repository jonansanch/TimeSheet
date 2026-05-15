using FluentValidation;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Catalogos.Modalidades.Queries.GetModalidades;
using KPG.Timesheet.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Application.Features.Catalogos.Modalidades.Commands.CreateModalidad;

public record CreateModalidadCommand(string Nombre) : IRequest<ModalidadDto>;

public class CreateModalidadCommandValidator : AbstractValidator<CreateModalidadCommand>
{
    public CreateModalidadCommandValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(100);
    }
}

public class CreateModalidadCommandHandler : IRequestHandler<CreateModalidadCommand, ModalidadDto>
{
    private readonly IApplicationDbContext _context;

    public CreateModalidadCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<ModalidadDto> Handle(CreateModalidadCommand request, CancellationToken cancellationToken)
    {
        var modalidad = new Modalidad(request.Nombre);
        _context.Modalidades.Add(modalidad);

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
