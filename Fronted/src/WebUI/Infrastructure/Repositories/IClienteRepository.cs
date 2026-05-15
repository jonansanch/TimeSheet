using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public interface IClienteRepository
{
    Task<List<ClienteResponse>> GetAllAsync(CancellationToken ct = default);
    Task<List<ClienteConProyectosResponse>> GetCatalogoAsync(CancellationToken ct = default);
    Task<(bool Ok, ClienteResponse? Cliente, string? Error)> CreateAsync(CreateClienteRequest request, CancellationToken ct = default);
    Task<(bool Ok, ClienteResponse? Cliente, string? Error)> UpdateAsync(int id, UpdateClienteRequest request, CancellationToken ct = default);
    Task<(bool Ok, ClienteResponse? Cliente, string? Error)> ToggleActivoAsync(int id, CancellationToken ct = default);
    Task<List<ProyectoResponse>> GetProyectosAsync(int clienteId, CancellationToken ct = default);
    Task<(bool Ok, ProyectoResponse? Proyecto, string? Error)> CreateProyectoAsync(int clienteId, CreateProyectoRequest request, CancellationToken ct = default);
    Task<(bool Ok, ProyectoResponse? Proyecto, string? Error)> UpdateProyectoAsync(int id, UpdateProyectoRequest request, CancellationToken ct = default);
    Task<(bool Ok, ProyectoResponse? Proyecto, string? Error)> ToggleProyectoActivoAsync(int id, CancellationToken ct = default);
}
