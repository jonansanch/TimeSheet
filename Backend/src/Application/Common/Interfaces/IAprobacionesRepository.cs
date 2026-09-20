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

    /// <summary>
    /// Empleados a los que este revisor les aprueba algun nivel. Alimenta el filtro de la
    /// pantalla de revision: mostrar ahi la lista completa de usuarios confunde (se puede
    /// filtrar por gente que nunca va a aparecer) y de paso le expone a un supervisor los
    /// nombres de todo el personal.
    /// </summary>
    Task<IReadOnlyList<EmpleadoRevisableDto>> GetEmpleadosRevisablesAsync(
        string revisorUserId,
        CancellationToken cancellationToken = default);
}
