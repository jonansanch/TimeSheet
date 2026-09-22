using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public interface IReporteUsuarioRepository
{
    Task<List<ReporteUsuarioResponse>> GetMisReportesAsync(CancellationToken ct = default);

    Task<List<ReporteUsuarioResponse>> GetTodosAsync(CancellationToken ct = default);

    Task<(bool Ok, ReporteUsuarioResponse? Item, string? Error)> CrearAsync(
        CrearReporteUsuarioRequest request, CancellationToken ct = default);

    Task<(bool Ok, ReporteUsuarioResponse? Item, string? Error)> CambiarEstadoAsync(
        int id, CambiarEstadoReporteRequest request, CancellationToken ct = default);
}
