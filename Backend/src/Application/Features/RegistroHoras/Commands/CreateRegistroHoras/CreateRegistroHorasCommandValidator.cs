using System.Linq.Expressions;

namespace KPG.Timesheet.Application.Features.RegistroHoras.Commands.CreateRegistroHoras;

public class CreateRegistroHorasCommandValidator : AbstractValidator<CreateRegistroHorasCommand>
{
    public CreateRegistroHorasCommandValidator()
    {
        RuleFor(x => x.FechaRegistro)
            .NotEmpty().WithMessage("La fecha del registro es requerida.");

        RuleFor(x => x)
            .Must(x => x.HoraEntrada1.HasValue || x.HoraEntrada2.HasValue || x.HoraEntrada3.HasValue)
            .WithMessage("Debe registrar al menos un horario.");

        // Cada bloque: si se provee uno de los dos extremos, ambos son obligatorios y salida > entrada
        ReglasBloque(1, x => x.HoraEntrada1, x => x.HoraSalida1);
        ReglasBloque(2, x => x.HoraEntrada2, x => x.HoraSalida2);
        ReglasBloque(3, x => x.HoraEntrada3, x => x.HoraSalida3);

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

    private void ReglasBloque(
        int numero,
        Expression<Func<CreateRegistroHorasCommand, TimeOnly?>> entrada,
        Expression<Func<CreateRegistroHorasCommand, TimeOnly?>> salida)
    {
        var getEntrada = entrada.Compile();
        var getSalida  = salida.Compile();

        When(x => getEntrada(x).HasValue || getSalida(x).HasValue, () =>
        {
            RuleFor(entrada)
                .NotNull()
                .WithMessage($"La hora de entrada del horario {numero} es requerida cuando se registra ese horario.");

            RuleFor(salida)
                .NotNull()
                .WithMessage($"La hora de salida del horario {numero} es requerida cuando se registra ese horario.")
                .Must((cmd, valor) => !getEntrada(cmd).HasValue || valor > getEntrada(cmd))
                .WithMessage($"La hora de salida del horario {numero} debe ser mayor que la hora de entrada.");
        });
    }
}
