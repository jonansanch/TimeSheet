using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public interface IRegistroHorasRepository
{
    Task<RegistroHorasResponse?> CreateAsync(CreateRegistroHorasRequest request, CancellationToken cancellationToken = default);
    Task<List<RegistroRecienteResponse>> GetRecientesAsync(int top = 5, CancellationToken cancellationToken = default);
    Task<List<HistorialRegistroResponse>> GetHistorialAsync(CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> UpdateDescripcionAsync(int id, string descripcion, CancellationToken cancellationToken = default);
}
