using KPG.Timesheet.Application.Common.Interfaces;
using ParametrosSistemaKeys = KPG.Timesheet.Domain.Constants.ParametrosSistema;

namespace KPG.Timesheet.Application.Common.Services;

public class VentanaRetroactividadService(
    IApplicationDbContext context,
    IParametrosSistemaService parametros)
    : IVentanaRetroactividadService
{
    private const int DiasPorDefecto = 3;

    public Task<int> GetDiasGlobalAsync(CancellationToken cancellationToken = default) =>
        parametros.GetIntAsync(ParametrosSistemaKeys.VentanaRetroactividad, DiasPorDefecto, cancellationToken);

    public async Task<int> GetDiasAsync(
        string? userId,
        IEnumerable<string>? roles,
        CancellationToken cancellationToken = default)
    {
        var global = await GetDiasGlobalAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(userId))
            return global;

        var listaRoles = roles?.ToList() ?? [];

        var reglas = await context.ReglasVentanaRetroactividad
            .Where(r => r.Activo
                     && (r.UserId == userId || (r.Rol != null && listaRoles.Contains(r.Rol))))
            .Select(r => new { r.UserId, r.Dias })
            .ToListAsync(cancellationToken);

        if (reglas.Count == 0)
            return global;

        // La regla de la persona manda, sin importar lo que digan las de sus roles.
        var deUsuario = reglas.FirstOrDefault(r => r.UserId != null);
        if (deUsuario is not null)
            return deUsuario.Dias;

        // Con varios roles se toma la ventana mas amplia: es predecible y no castiga
        // a alguien por acumular roles.
        return reglas.Max(r => r.Dias);
    }
}
