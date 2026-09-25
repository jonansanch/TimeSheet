using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public interface ITerminoDescripcionRepository
{
    Task<List<TerminoDescripcionResponse>> GetAllAsync(CancellationToken ct = default);
    Task<(bool Ok, TerminoDescripcionResponse? Termino, string? Error)> CreateAsync(CreateTerminoDescripcionRequest request, CancellationToken ct = default);
    Task<(bool Ok, TerminoDescripcionResponse? Termino, string? Error)> UpdateAsync(int id, UpdateTerminoDescripcionRequest request, CancellationToken ct = default);
    Task<(bool Ok, TerminoDescripcionResponse? Termino, string? Error)> ToggleActivoAsync(int id, CancellationToken ct = default);

    Task<ParametrosDescripcionResponse?> GetParametrosAsync(CancellationToken ct = default);
    Task<(bool Ok, string? Error)> UpdateParametrosAsync(UpdateParametrosDescripcionRequest request, CancellationToken ct = default);

    Task<EvaluacionDescripcionResponse?> EvaluarAsync(string texto, int? proyectoId, CancellationToken ct = default);

    /// <summary>
    /// "Mejorar redacción" con IA. La IA sin configurar viene en la respuesta
    /// (Disponible=false); el límite diario alcanzado llega como error HTTP 400, igual que
    /// cualquier otra validación de un comando.
    /// </summary>
    Task<(MejorarDescripcionResultResponse? Resultado, string? Error)> MejorarAsync(
        string texto, int? proyectoId, CancellationToken ct = default);

    /// <summary>False si no hay IA configurada en este ambiente, o si la sesión/red falla.</summary>
    Task<bool> MejorarDisponibleAsync(CancellationToken ct = default);
}
