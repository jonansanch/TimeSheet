using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public interface ISolicitudExcepcionRepository
{
    Task<SolicitudExcepcionResponse?> CreateAsync(CreateSolicitudExcepcionRequest request, CancellationToken cancellationToken = default);
    Task<List<DateOnly>> GetMisAprobadasAsync(CancellationToken cancellationToken = default);
    Task<List<MiSolicitudExcepcionResponse>> GetMisSolicitudesAsync(CancellationToken cancellationToken = default);
}
