using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Validation;

namespace KPG.Timesheet.Application.Features.RegistroHoras.Commands.UpdateDescripcionRegistroHoras;

/// <summary>
/// Calidad de la descripcion contra el catalogo de terminos. Va aparte de
/// <see cref="UpdateDescripcionRegistroHorasCommandValidator"/> porque necesita base de
/// datos: a diferencia de crear, aqui el comando no trae el proyecto, hay que resolverlo
/// desde el registro que se esta editando.
/// </summary>
public class UpdateDescripcionRegistroHorasDescripcionValidator : AbstractValidator<UpdateDescripcionRegistroHorasCommand>
{
    public UpdateDescripcionRegistroHorasDescripcionValidator(
        IApplicationDbContext context, IValidadorDescripcion validador)
    {
        RuleFor(x => x.Descripcion)
            .DescripcionValida(validador, (cmd, cancellationToken) =>
                context.RegistrosHoras
                    .Where(r => r.Id == cmd.RegistroId)
                    .Select(r => (int?)r.ProyectoId)
                    .FirstOrDefaultAsync(cancellationToken));
    }
}
