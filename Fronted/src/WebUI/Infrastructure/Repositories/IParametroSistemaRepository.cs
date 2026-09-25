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

    /// <summary>Logo actual de los reportes, como data URI, o cadena vacia si no hay ninguno.</summary>
    Task<string> GetLogoReportesAsync(CancellationToken ct = default);
    Task<(bool Ok, string? Error)> UpdateLogoReportesAsync(string? imagenDataUri, CancellationToken ct = default);

    /// <summary>Estado de la API key de Gemini (dictado de voz y "mejorar redaccion"), sin exponer el valor completo.</summary>
    Task<(bool Configurada, string? Mascara)> GetGeminiApiKeyEstadoAsync(CancellationToken ct = default);
    /// <summary>Vacio o null quita la key configurada.</summary>
    Task<(bool Ok, string? Error)> UpdateGeminiApiKeyAsync(string? apiKey, CancellationToken ct = default);
}
