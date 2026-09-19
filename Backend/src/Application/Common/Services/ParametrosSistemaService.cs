using KPG.Timesheet.Application.Common.Interfaces;
using ParametrosSistemaKeys = KPG.Timesheet.Domain.Constants.ParametrosSistema;

namespace KPG.Timesheet.Application.Common.Services;

public class ParametrosSistemaService(IApplicationDbContext context) : IParametrosSistemaService
{
    /// <summary>Horas por defecto de una jornada completa si el parametro no esta configurado.</summary>
    private const int HorasDiaCompletoPorDefecto = 8;

    public async Task<int> GetIntAsync(
        string clave,
        int valorPorDefecto,
        CancellationToken cancellationToken = default)
    {
        var parametro = await context.ParametrosSistema
            .FirstOrDefaultAsync(p => p.Clave == clave, cancellationToken);

        return parametro is not null && int.TryParse(parametro.Valor, out var valor)
            ? valor
            : valorPorDefecto;
    }

    public async Task<string> GetTextoAsync(
        string clave,
        string valorPorDefecto,
        CancellationToken cancellationToken = default)
    {
        var parametro = await context.ParametrosSistema
            .FirstOrDefaultAsync(p => p.Clave == clave, cancellationToken);

        return string.IsNullOrWhiteSpace(parametro?.Valor) ? valorPorDefecto : parametro.Valor.Trim();
    }

    public async Task<int> GetMinutosDiaCompletoAsync(CancellationToken cancellationToken = default)
    {
        var horas = await GetIntAsync(
            ParametrosSistemaKeys.HorasDiaCompleto,
            HorasDiaCompletoPorDefecto,
            cancellationToken);

        // Un umbral no positivo dejaria todo dia con registro como "completo": se ignora.
        if (horas <= 0) horas = HorasDiaCompletoPorDefecto;

        return horas * 60;
    }
}
