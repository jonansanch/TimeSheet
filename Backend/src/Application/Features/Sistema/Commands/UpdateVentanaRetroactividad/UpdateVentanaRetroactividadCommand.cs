using FluentValidation;
using KPG.Timesheet.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Application.Features.Sistema.Commands.UpdateVentanaRetroactividad;

public record UpdateVentanaRetroactividadCommand(int Dias) : IRequest;

public class UpdateVentanaRetroactividadCommandValidator : AbstractValidator<UpdateVentanaRetroactividadCommand>
{
    public UpdateVentanaRetroactividadCommandValidator()
    {
        RuleFor(x => x.Dias)
            .GreaterThanOrEqualTo(1).WithMessage("El valor debe ser al menos 1 día.")
            .LessThanOrEqualTo(30).WithMessage("El valor no puede superar 30 días.");
    }
}

public class UpdateVentanaRetroactividadCommandHandler : IRequestHandler<UpdateVentanaRetroactividadCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateVentanaRetroactividadCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(UpdateVentanaRetroactividadCommand request, CancellationToken cancellationToken)
    {
        var param = await _context.ParametrosSistema
            .FirstOrDefaultAsync(p => p.Clave == Domain.Constants.ParametrosSistema.VentanaRetroactividad, cancellationToken);

        if (param is not null)
        {
            param.Valor = request.Dias.ToString();
        }
        else
        {
            _context.ParametrosSistema.Add(new Domain.Entities.ParametroSistema
            {
                Clave = Domain.Constants.ParametrosSistema.VentanaRetroactividad,
                Valor = request.Dias.ToString()
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
