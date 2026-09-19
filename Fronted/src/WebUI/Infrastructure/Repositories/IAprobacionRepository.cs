using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public interface IAprobacionRepository
{
    Task<PendientesAprobacionResponse> GetPendientesAsync(
        DateOnly desde,
        DateOnly hasta,
        string? userId = null,
        int? clienteId = null,
        int? proyectoId = null,
        bool incluirRevisados = false,
        CancellationToken ct = default);

    Task<(bool Ok, EstadoRegistroResponse? Estado, string? Error)> AprobarAsync(int id, CancellationToken ct = default);

    Task<(bool Ok, EstadoRegistroResponse? Estado, string? Error)> RechazarAsync(
        int id, string comentario, CancellationToken ct = default);

    Task<(bool Ok, EstadoRegistroResponse? Estado, string? Error)> RevertirAprobacionAsync(int id, CancellationToken ct = default);

    Task<(bool Ok, EstadoRegistroResponse? Estado, string? Error)> RevertirRechazoAsync(int id, CancellationToken ct = default);

    Task<(bool Ok, ImportacionResultadoResponse? Resultado, string? Error)> ImportarAsync(
        Stream archivo,
        string nombreArchivo,
        string userId,
        bool marcarAprobado,
        CancellationToken ct = default);
}
