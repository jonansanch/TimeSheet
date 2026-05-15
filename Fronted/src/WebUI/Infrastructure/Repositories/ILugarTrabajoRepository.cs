using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public interface ILugarTrabajoRepository
{
    Task<List<LugarTrabajoResponse>> GetAllAsync(CancellationToken ct = default);
    Task<List<LugarTrabajoResponse>> GetActivosAsync(CancellationToken ct = default);
    Task<(bool Ok, LugarTrabajoResponse? Lugar, string? Error)> CreateAsync(CreateLugarTrabajoRequest request, CancellationToken ct = default);
    Task<(bool Ok, LugarTrabajoResponse? Lugar, string? Error)> UpdateAsync(int id, UpdateLugarTrabajoRequest request, CancellationToken ct = default);
    Task<(bool Ok, LugarTrabajoResponse? Lugar, string? Error)> ToggleActivoAsync(int id, CancellationToken ct = default);
}
