using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public interface IRegistroHorasRepository
{
    /// <summary>
    /// Devuelve tambien el motivo del rechazo: el backend explica por que no pudo guardar
    /// (p. ej. "el horario 1 ya fue registrado") y ese texto le sirve al usuario.
    /// </summary>
    Task<(RegistroHorasResponse? Registro, string? Error)> CreateAsync(
        CreateRegistroHorasRequest request, CancellationToken cancellationToken = default);
    Task<List<RegistroRecienteResponse>> GetRecientesAsync(int top = 5, CancellationToken cancellationToken = default);
    Task<HistorialPaginadoResponse> GetHistorialAsync(int page = 1, int pageSize = 20, DateOnly? desde = null, DateOnly? hasta = null, CancellationToken cancellationToken = default);
    Task<ResumenMensualResponse> GetResumenMensualAsync(int mes, int anio, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> UpdateDescripcionAsync(
        int id,
        string descripcion,
        string? modalidad = null,
        string? recurso = null,
        string? lugar = null,
        CancellationToken cancellationToken = default);
}
