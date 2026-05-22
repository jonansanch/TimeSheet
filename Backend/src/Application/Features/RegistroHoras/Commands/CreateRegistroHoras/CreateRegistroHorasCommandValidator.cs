namespace KPG.Timesheet.Application.Features.RegistroHoras.Commands.CreateRegistroHoras;

public class CreateRegistroHorasCommandValidator : AbstractValidator<CreateRegistroHorasCommand>
{
    public CreateRegistroHorasCommandValidator()
    {
        RuleFor(x => x.FechaRegistro)
            .NotEmpty().WithMessage("La fecha del registro es requerida.");

        RuleFor(x => x)
            .Must(x => x.HoraEntradaAM.HasValue || x.HoraEntradaPM.HasValue)
            .WithMessage("Debe registrar al menos un turno (AM o PM).");

        // Bloque AM: si se provee uno, ambos son obligatorios y salida > entrada
        When(x => x.HoraEntradaAM.HasValue || x.HoraSalidaAM.HasValue, () =>
        {
            RuleFor(x => x.HoraEntradaAM)
                .NotNull().WithMessage("La hora de entrada AM es requerida cuando se registra el turno AM.");
            RuleFor(x => x.HoraSalidaAM)
                .NotNull().WithMessage("La hora de salida AM es requerida cuando se registra el turno AM.")
                .Must((cmd, salida) => !cmd.HoraEntradaAM.HasValue || salida > cmd.HoraEntradaAM)
                .WithMessage("La hora de salida AM debe ser mayor que la hora de entrada.");
        });

        // Bloque PM: si se provee uno, ambos son obligatorios y salida > entrada
        When(x => x.HoraEntradaPM.HasValue || x.HoraSalidaPM.HasValue, () =>
        {
            RuleFor(x => x.HoraEntradaPM)
                .NotNull().WithMessage("La hora de entrada PM es requerida cuando se registra el turno PM.");
            RuleFor(x => x.HoraSalidaPM)
                .NotNull().WithMessage("La hora de salida PM es requerida cuando se registra el turno PM.")
                .Must((cmd, salida) => !cmd.HoraEntradaPM.HasValue || salida > cmd.HoraEntradaPM)
                .WithMessage("La hora de salida PM debe ser mayor que la hora de entrada.");
        });

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
