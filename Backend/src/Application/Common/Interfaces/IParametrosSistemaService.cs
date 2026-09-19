namespace KPG.Timesheet.Application.Common.Interfaces;

/// <summary>
/// Lectura tipada de los parametros configurables del sistema, con valor por
/// defecto cuando la clave no existe o su valor no es parseable.
/// </summary>
public interface IParametrosSistemaService
{
    Task<int> GetIntAsync(string clave, int valorPorDefecto, CancellationToken cancellationToken = default);

    Task<string> GetTextoAsync(string clave, string valorPorDefecto, CancellationToken cancellationToken = default);

    /// <summary>Minutos minimos que debe sumar un dia para considerarse completo.</summary>
    Task<int> GetMinutosDiaCompletoAsync(CancellationToken cancellationToken = default);
}
