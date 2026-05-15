using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public interface IEmpleadoRepository
{
    Task<List<EmpleadoResponse>> GetAllAsync(CancellationToken ct = default);
    Task<List<EmpleadoResponse>> GetActivosAsync(CancellationToken ct = default);
    Task<(bool Ok, EmpleadoResponse? Empleado, string? Error)> CreateAsync(CreateEmpleadoRequest request, CancellationToken ct = default);
    Task<(bool Ok, EmpleadoResponse? Empleado, string? Error)> UpdateAsync(int id, UpdateEmpleadoRequest request, CancellationToken ct = default);
    Task<(bool Ok, EmpleadoResponse? Empleado, string? Error)> ToggleActivoAsync(int id, CancellationToken ct = default);
}
