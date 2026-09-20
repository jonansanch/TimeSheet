using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Domain.Enums;

namespace KPG.Timesheet.Application.Features.Voz;

/// <summary>
/// Interpreta lo dictado en el formulario de registro. El catalogo se arma aqui, contra la
/// base, en vez de confiar en el que mande el navegador: asi el modelo solo puede elegir
/// clientes y proyectos que existen y estan activos.
/// </summary>
[Authorize(Roles = $"{Roles.Empleado},{Roles.Supervisor},{Roles.Gerente},{Roles.Admin}")]
public record InterpretarVozCommand(string Transcripcion) : IRequest<InterpretacionVozDto>;

public class InterpretarVozCommandValidator : AbstractValidator<InterpretarVozCommand>
{
    /// <summary>Tope generoso para una frase dictada; evita mandar un texto enorme al modelo.</summary>
    public const int MaxCaracteres = 2000;

    public InterpretarVozCommandValidator()
    {
        RuleFor(x => x.Transcripcion)
            .NotEmpty().WithMessage("No se recibio ningun texto dictado.")
            .MaximumLength(MaxCaracteres)
            .WithMessage($"El dictado no puede superar {MaxCaracteres} caracteres.");
    }
}

public class InterpretarVozCommandHandler(
    IApplicationDbContext context,
    IInterpreteVoz interprete,
    IClock clock)
    : IRequestHandler<InterpretarVozCommand, InterpretacionVozDto>
{
    public async Task<InterpretacionVozDto> Handle(
        InterpretarVozCommand request,
        CancellationToken cancellationToken)
    {
        if (!interprete.Disponible)
            throw new Common.Exceptions.ServicioNoDisponibleException(
                "La interpretacion por IA no esta configurada en este ambiente.");

        var catalogo = await ArmarCatalogoAsync(cancellationToken);

        return await interprete.InterpretarAsync(
            request.Transcripcion, catalogo, clock.Today, cancellationToken);
    }

    private async Task<CatalogoVozDto> ArmarCatalogoAsync(CancellationToken cancellationToken)
    {
        var proyectos = await context.Proyectos
            .Where(p => p.Activo)
            .Join(context.Clientes.Where(c => c.Activo), p => p.ClienteId, c => c.Id,
                  (p, c) => new { Cliente = c.Nombre, Proyecto = p.Nombre })
            .ToListAsync(cancellationToken);

        var porCliente = proyectos
            .GroupBy(x => x.Cliente)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<string>)g.Select(x => x.Proyecto).Distinct().ToList());

        var modalidades = await context.Modalidades
            .Where(m => m.Activo).Select(m => m.Nombre).ToListAsync(cancellationToken);
        var recursos = await context.Empleados
            .Where(e => e.Activo).Select(e => e.Nombre).ToListAsync(cancellationToken);
        var lugares = await context.LugaresTrabajo
            .Where(l => l.Activo).Select(l => l.Nombre).ToListAsync(cancellationToken);

        return new CatalogoVozDto(
            porCliente.Keys.ToList(),
            porCliente,
            modalidades,
            recursos,
            lugares);
    }
}
