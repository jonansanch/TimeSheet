using KPG.Timesheet.Application.Common.Interfaces;

namespace KPG.Timesheet.Application.Features.RegistroHoras.Commands.CreateRegistroHoras;

/// <summary>
/// Verifica contra el catalogo que el proyecto exista y que tanto el como su cliente
/// esten activos.
///
/// <para>
/// Desde la migracion a <c>ProyectoId</c>, la pertenencia al cliente la garantiza la FK:
/// ya no hay forma de enviar una pareja incoherente. Lo que sigue haciendo falta es
/// impedir que se imputen horas a un proyecto o cliente dados de baja.
/// </para>
/// <para>
/// Va aparte de <see cref="CreateRegistroHorasCommandValidator"/> —que solo valida la
/// forma del comando y no necesita base de datos— para que aquel siga siendo instanciable
/// sin dependencias. El pipeline ejecuta ambos.
/// </para>
/// </summary>
public class CreateRegistroHorasCatalogoValidator : AbstractValidator<CreateRegistroHorasCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateRegistroHorasCatalogoValidator(IApplicationDbContext context)
    {
        _context = context;

        // El ProyectoId <= 0 lo reporta el validador de forma: aqui no se duplica.
        When(x => x.ProyectoId > 0, () =>
        {
            RuleFor(x => x.ProyectoId)
                .MustAsync(ExisteYEstaActivoAsync)
                .WithMessage("El proyecto seleccionado no existe, esta inactivo o su cliente esta inactivo.");
        });
    }

    private Task<bool> ExisteYEstaActivoAsync(int proyectoId, CancellationToken cancellationToken) =>
        _context.Proyectos.AnyAsync(
            p => p.Id == proyectoId
              && p.Activo
              && _context.Clientes.Any(c => c.Id == p.ClienteId && c.Activo),
            cancellationToken);
}
