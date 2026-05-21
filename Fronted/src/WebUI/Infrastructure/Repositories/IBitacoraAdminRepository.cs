using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public interface IBitacoraAdminRepository
{
    Task<BitacoraResponse?> GetBitacoraAsync(
        DateOnly? desde = null,
        DateOnly? hasta = null,
        string? actorId = null,
        string? tipoEvento = null,
        CancellationToken ct = default);

    Task<(byte[] Contenido, string ContentType, string FileName)?> ExportarExcelAsync(
        DateOnly? desde = null,
        DateOnly? hasta = null,
        string? actorId = null,
        string? tipoEvento = null,
        CancellationToken ct = default);
}
