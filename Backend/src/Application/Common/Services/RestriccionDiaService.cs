using KPG.Timesheet.Application.Common.Interfaces;

namespace KPG.Timesheet.Application.Common.Services;

public class RestriccionDiaService(IApplicationDbContext context) : IRestriccionDiaService
{
    public async Task<(bool PuedeRegistrar, string? Motivo)> PuedeRegistrarEnDiaAsync(
        string userId,
        IEnumerable<string>? roles,
        DateOnly fecha,
        CancellationToken cancellationToken = default)
    {
        var dia = fecha.DayOfWeek;
        var listaRoles = roles?.ToList() ?? [];

        var restriccionUsuario = await context.ParametrosRestriccionDia
            .AnyAsync(r => r.Activo && r.DiaDelaSemana == dia && r.UserId == userId, cancellationToken);

        if (restriccionUsuario)
            return (false, $"No puede registrar el {NombreDia(dia)}: tiene una restriccion para ese dia.");

        var rolRestringido = await context.ParametrosRestriccionDia
            .Where(r => r.Activo && r.DiaDelaSemana == dia && r.Rol != null && listaRoles.Contains(r.Rol!))
            .Select(r => r.Rol)
            .FirstOrDefaultAsync(cancellationToken);

        if (rolRestringido is not null)
            return (false, $"No puede registrar el {NombreDia(dia)}: el rol '{rolRestringido}' tiene ese dia restringido.");

        return (true, null);
    }

    private static string NombreDia(DayOfWeek dia) => dia switch
    {
        DayOfWeek.Sunday    => "domingo",
        DayOfWeek.Monday    => "lunes",
        DayOfWeek.Tuesday   => "martes",
        DayOfWeek.Wednesday => "miercoles",
        DayOfWeek.Thursday  => "jueves",
        DayOfWeek.Friday    => "viernes",
        DayOfWeek.Saturday  => "sabado",
        _                    => dia.ToString()
    };
}
