namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public interface IParametroSistemaRepository
{
    Task<int> GetVentanaRetroactividadAsync(CancellationToken cancellationToken = default);
    Task<(bool Ok, string? Error)> UpdateVentanaRetroactividadAsync(int dias, CancellationToken ct = default);
    Task<int> GetUmbralNotificacionAsync(CancellationToken ct = default);
    Task<(bool Ok, string? Error)> UpdateUmbralNotificacionAsync(int dias, CancellationToken ct = default);
}
