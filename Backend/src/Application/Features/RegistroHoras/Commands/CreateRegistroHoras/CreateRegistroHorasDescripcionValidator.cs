using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Validation;

namespace KPG.Timesheet.Application.Features.RegistroHoras.Commands.CreateRegistroHoras;

/// <summary>
/// Calidad de la descripcion contra el catalogo de terminos (ver
/// Docs/plan-calidad-descripciones.md). Va aparte de
/// <see cref="CreateRegistroHorasCommandValidator"/> por la misma razon que
/// <see cref="CreateRegistroHorasCatalogoValidator"/>: necesita base de datos, y el
/// validador de forma tiene que seguir siendo instanciable sin dependencias.
/// </summary>
public class CreateRegistroHorasDescripcionValidator : AbstractValidator<CreateRegistroHorasCommand>
{
    public CreateRegistroHorasDescripcionValidator(IValidadorDescripcion validador)
    {
        RuleFor(x => x.Descripcion)
            .DescripcionValida(validador, (cmd, _) => Task.FromResult<int?>(cmd.ProyectoId));
    }
}
