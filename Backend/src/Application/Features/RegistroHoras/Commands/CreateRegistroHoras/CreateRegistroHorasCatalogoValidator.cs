using KPG.Timesheet.Application.Common.Interfaces;

namespace KPG.Timesheet.Application.Features.RegistroHoras.Commands.CreateRegistroHoras;

/// <summary>
/// Verifica contra el catalogo que el proyecto pertenezca al cliente indicado.
/// Va aparte de <see cref="CreateRegistroHorasCommandValidator"/> —que solo valida la
/// forma del comando y no necesita base de datos— para que aquel siga siendo instanciable
/// sin dependencias. El pipeline ejecuta ambos validadores.
///
/// El registro guarda Cliente y Proyecto como texto suelto, no como ProyectoId, asi que
/// sin esta regla la API acepta cualquier combinacion aunque la UI ofrezca el desplegable
/// en cascada. Las parejas invalidas ademas romperian una futura migracion a ProyectoId.
/// </summary>
public class CreateRegistroHorasCatalogoValidator : AbstractValidator<CreateRegistroHorasCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateRegistroHorasCatalogoValidator(IApplicationDbContext context)
    {
        _context = context;

        // Solo corre si cliente y proyecto traen algo: los NotEmpty los cubre el otro validador.
        When(x => !string.IsNullOrWhiteSpace(x.Cliente) && !string.IsNullOrWhiteSpace(x.Proyecto), () =>
        {
            RuleFor(x => x.Cliente)
                .MustAsync(ExisteClienteActivoAsync)
                .WithMessage("El cliente seleccionado no existe o esta inactivo.");

            RuleFor(x => x.Proyecto)
                .MustAsync(PerteneceAlClienteAsync)
                .WithMessage((cmd, _) =>
                    $"El proyecto seleccionado no pertenece al cliente '{cmd.Cliente.Trim()}' o esta inactivo.");
        });
    }

    private Task<bool> ExisteClienteActivoAsync(string cliente, CancellationToken cancellationToken)
    {
        var nombre = cliente.Trim();
        return _context.Clientes.AnyAsync(c => c.Activo && c.Nombre == nombre, cancellationToken);
    }

    private async Task<bool> PerteneceAlClienteAsync(
        CreateRegistroHorasCommand command,
        string proyecto,
        CancellationToken cancellationToken)
    {
        // Si el cliente no existe, la regla anterior ya reporta el error: no se duplica.
        if (!await ExisteClienteActivoAsync(command.Cliente, cancellationToken))
            return true;

        var nombreCliente  = command.Cliente.Trim();
        var nombreProyecto = proyecto.Trim();

        return await _context.Proyectos.AnyAsync(
            p => p.Activo
              && p.Nombre == nombreProyecto
              && _context.Clientes.Any(c => c.Id == p.ClienteId && c.Activo && c.Nombre == nombreCliente),
            cancellationToken);
    }
}
