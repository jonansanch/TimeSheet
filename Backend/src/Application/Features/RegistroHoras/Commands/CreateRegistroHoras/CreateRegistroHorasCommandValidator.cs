using KPG.Timesheet.Domain.Enums;

namespace KPG.Timesheet.Application.Features.RegistroHoras.Commands.CreateRegistroHoras;

public class CreateRegistroHorasCommandValidator : AbstractValidator<CreateRegistroHorasCommand>
{
    public CreateRegistroHorasCommandValidator()
    {
        RuleFor(x => x.FechaRegistro)
            .NotEmpty().WithMessage("La fecha del registro es requerida.");

        RuleFor(x => x.Turno)
            .IsInEnum().WithMessage("El turno debe ser AM o PM.")
            .Must(turno => turno is TurnoRegistro.AM or TurnoRegistro.PM)
            .WithMessage("El turno debe ser AM o PM.");

        RuleFor(x => x.HoraEntrada)
            .NotEmpty().WithMessage("La hora de entrada es requerida.");

        RuleFor(x => x.HoraSalida)
            .NotEmpty().WithMessage("La hora de salida es requerida.")
            .GreaterThan(x => x.HoraEntrada)
            .WithMessage("La hora de salida debe ser mayor que la hora de entrada.");

        RuleFor(x => x.Cliente)
            .NotEmpty().WithMessage("El cliente es requerido.")
            .MaximumLength(200).WithMessage("El cliente no puede superar 200 caracteres.");

        RuleFor(x => x.Proyecto)
            .NotEmpty().WithMessage("El proyecto es requerido.")
            .MaximumLength(200).WithMessage("El proyecto no puede superar 200 caracteres.");

        RuleFor(x => x.Modalidad)
            .NotEmpty().WithMessage("La modalidad es requerida.")
            .MaximumLength(100).WithMessage("La modalidad no puede superar 100 caracteres.");

        RuleFor(x => x.Recurso)
            .NotEmpty().WithMessage("El recurso es requerido.")
            .MaximumLength(100).WithMessage("El recurso no puede superar 100 caracteres.");

        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripcion es requerida.")
            .MaximumLength(1000).WithMessage("La descripcion no puede superar 1000 caracteres.");

        RuleFor(x => x.Lugar)
            .NotEmpty().WithMessage("El lugar es requerido.")
            .MaximumLength(200).WithMessage("El lugar no puede superar 200 caracteres.");
    }
}
