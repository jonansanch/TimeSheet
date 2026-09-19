using KPG.Timesheet.Application.Features.Aprobaciones;

namespace KPG.Timesheet.Application.Common.Interfaces;

/// <summary>
/// Vive fuera de <see cref="IApplicationDbContext"/> porque resolver quien aprueba cada
/// nivel exige leer AspNetUsers, que Application no expone.
/// </summary>
public interface IAprobacionesRepository
{
    Task<PendientesAprobacionResponse> GetPendientesAsync(
        string revisorUserId,
        GetPendientesAprobacionQuery filtros,
        CancellationToken cancellationToken = default);
}
