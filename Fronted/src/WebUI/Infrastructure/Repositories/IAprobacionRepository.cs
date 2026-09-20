using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public interface IAprobacionRepository
{
    /// <summary>
    /// Solo los empleados a los que este revisor les aprueba algun nivel. Alimenta el
    /// filtro de la pantalla: la lista completa de usuarios no sirve ahi.
    /// </summary>
    Task<List<EmpleadoRevisableResponse>> GetEmpleadosRevisablesAsync(
        CancellationToken cancellationToken = default);

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

    /// <summary>
    /// El empleado reenvia su registro rechazado tras corregirlo. La cadena recomienza
    /// desde el primer nivel: los aprobadores anteriores revisan la correccion.
    /// </summary>
    Task<(bool Ok, EstadoRegistroResponse? Estado, string? Error)> ReenviarAsync(int id, CancellationToken ct = default);

    Task<(bool Ok, ImportacionResultadoResponse? Resultado, string? Error)> ImportarAsync(
        Stream archivo,
        string nombreArchivo,
        string userId,
        bool marcarAprobado,
        CancellationToken ct = default);
}
