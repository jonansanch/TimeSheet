using FluentValidation;
using KPG.Timesheet.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Application.Features.Sistema.Commands.UpdateUmbralNotificacion;

public record UpdateUmbralNotificacionCommand(int Dias) : IRequest;

public class UpdateUmbralNotificacionCommandValidator : AbstractValidator<UpdateUmbralNotificacionCommand>
{
    public UpdateUmbralNotificacionCommandValidator()
    {
        RuleFor(x => x.Dias)
            .GreaterThanOrEqualTo(1).WithMessage("El valor debe ser al menos 1 día.")
            .LessThanOrEqualTo(30).WithMessage("El valor no puede superar 30 días.");
    }
}

public class UpdateUmbralNotificacionCommandHandler : IRequestHandler<UpdateUmbralNotificacionCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateUmbralNotificacionCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(UpdateUmbralNotificacionCommand request, CancellationToken cancellationToken)
    {
        var param = await _context.ParametrosSistema
            .FirstOrDefaultAsync(p => p.Clave == Domain.Constants.ParametrosSistema.DiasUmbralNotificacion, cancellationToken);

        if (param is not null)
        {
            param.Valor = request.Dias.ToString();
        }
        else
        {
            _context.ParametrosSistema.Add(new Domain.Entities.ParametroSistema
            {
                Clave = Domain.Constants.ParametrosSistema.DiasUmbralNotificacion,
                Valor = request.Dias.ToString()
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
