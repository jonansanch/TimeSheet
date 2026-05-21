using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public interface IBitacoraRepository
{
    Task<BitacoraResponse?> GetMiAlcanceAsync(
        DateOnly? desde = null,
        DateOnly? hasta = null,
        string? actorId = null,
        string? tipoEvento = null,
        CancellationToken ct = default);
}
