using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public interface IReglaVentanaRepository
{
    Task<List<ReglaVentanaResponse>> GetAllAsync(CancellationToken ct = default);

    Task<(bool Ok, ReglaVentanaResponse? Item, string? Error)> CrearAsync(
        GuardarReglaVentanaRequest request, CancellationToken ct = default);

    Task<(bool Ok, ReglaVentanaResponse? Item, string? Error)> ActualizarAsync(
        int id, GuardarReglaVentanaRequest request, CancellationToken ct = default);

    Task<(bool Ok, ReglaVentanaResponse? Item)> ToggleAsync(int id, CancellationToken ct = default);
}
