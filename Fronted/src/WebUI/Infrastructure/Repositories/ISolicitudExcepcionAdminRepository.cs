using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public interface ISolicitudExcepcionAdminRepository
{
    Task<List<SolicitudExcepcionAdminResponse>> GetAllAsync(CancellationToken ct = default);
    Task<bool> AprobarAsync(int id, CancellationToken ct = default);
    Task<bool> RechazarAsync(int id, CancellationToken ct = default);
}
