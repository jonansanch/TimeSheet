namespace KPG.Timesheet.Application.Common.Interfaces;

/// <summary>
/// Verifica si una persona puede registrar horas en una fecha dada, segun las
/// restricciones de dia de la semana configuradas por rol o por persona.
/// </summary>
public interface IRestriccionDiaService
{
    /// <summary>
    /// Retorna si puede registrar y, si no, el motivo para mostrar al usuario.
    /// Una restriccion de persona o de rol que coincida con el dia bloquea el registro.
    /// </summary>
    Task<(bool PuedeRegistrar, string? Motivo)> PuedeRegistrarEnDiaAsync(
        string userId,
        IEnumerable<string>? roles,
        DateOnly fecha,
        CancellationToken cancellationToken = default);
}
