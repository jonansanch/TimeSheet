using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public interface ISupervisorPuestoRepository
{
    Task<List<SupervisorPuestoResponse>> GetAllAsync(CancellationToken ct = default);

    Task<(bool Ok, SupervisorPuestoResponse? Item, string? Error)> CrearAsync(
        GuardarSupervisorPuestoRequest request, CancellationToken ct = default);

    Task<(bool Ok, SupervisorPuestoResponse? Item, string? Error)> ActualizarAsync(
        int id, GuardarSupervisorPuestoRequest request, CancellationToken ct = default);

    Task<(bool Ok, SupervisorPuestoResponse? Item)> ToggleAsync(int id, CancellationToken ct = default);
}
