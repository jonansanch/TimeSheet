using KPG.Timesheet.Application.Features.Bitacora.Queries.GetBitacora;

namespace KPG.Timesheet.Application.Common.Interfaces;

public interface IBitacoraQueryRepository
{
    Task<BitacoraResponse> GetAsync(DateOnly? desde, DateOnly? hasta, string? actorId, string? tipoEvento, CancellationToken cancellationToken = default);
    Task<BitacoraResponse> GetAlcanceAsync(DateOnly? desde, DateOnly? hasta, string? actorId, string? tipoEvento, bool soloEquipo, CancellationToken cancellationToken = default);
}
