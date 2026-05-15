using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public interface IModalidadRepository
{
    Task<List<ModalidadResponse>> GetAllAsync(CancellationToken ct = default);
    Task<List<ModalidadResponse>> GetActivasAsync(CancellationToken ct = default);
    Task<(bool Ok, ModalidadResponse? Modalidad, string? Error)> CreateAsync(CreateModalidadRequest request, CancellationToken ct = default);
    Task<(bool Ok, ModalidadResponse? Modalidad, string? Error)> UpdateAsync(int id, UpdateModalidadRequest request, CancellationToken ct = default);
    Task<(bool Ok, ModalidadResponse? Modalidad, string? Error)> ToggleActivaAsync(int id, CancellationToken ct = default);
}
