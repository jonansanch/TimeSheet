namespace KPG.Timesheet.Application.Common.Interfaces;

/// <summary>
/// Resuelve cuantos dias habiles hacia atras puede registrar una persona.
/// Precedencia: regla de la persona → regla de su rol → parametro global.
/// </summary>
public interface IVentanaRetroactividadService
{
    Task<int> GetDiasAsync(
        string? userId,
        IEnumerable<string>? roles,
        CancellationToken cancellationToken = default);

    /// <summary>Valor global, sin aplicar excepciones. Lo usa la pantalla de parametros.</summary>
    Task<int> GetDiasGlobalAsync(CancellationToken cancellationToken = default);
}
