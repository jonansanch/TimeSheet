using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public interface IRestriccionDiaRepository
{
    Task<List<RestriccionDiaResponse>> GetAllAsync(CancellationToken ct = default);

    Task<(bool Ok, RestriccionDiaResponse? Item, string? Error)> CrearAsync(
        GuardarRestriccionDiaRequest request, CancellationToken ct = default);

    Task<(bool Ok, RestriccionDiaResponse? Item)> ToggleAsync(int id, CancellationToken ct = default);
}
