namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public interface IParametroSistemaRepository
{
    /// <summary>Ventana efectiva del usuario autenticado, ya con excepciones aplicadas.</summary>
    Task<int> GetVentanaRetroactividadAsync(CancellationToken cancellationToken = default);

    /// <summary>Valor global del sistema, sin excepciones. Solo Admin.</summary>
    Task<int> GetVentanaRetroactividadGlobalAsync(CancellationToken cancellationToken = default);
    Task<(bool Ok, string? Error)> UpdateVentanaRetroactividadAsync(int dias, CancellationToken ct = default);
    /// <summary>Corte de revision: "Semanal" o "Quincenal".</summary>
    Task<string> GetPeriodoAprobacionAsync(CancellationToken ct = default);

    Task<int> GetUmbralNotificacionAsync(CancellationToken ct = default);
    Task<(bool Ok, string? Error)> UpdateUmbralNotificacionAsync(int dias, CancellationToken ct = default);
}
