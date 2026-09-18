namespace KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

/// <summary>
/// Regla que decide quien aprueba en primer nivel los registros de un puesto.
/// <c>ClienteId</c> nulo = regla general, aplica a todos los clientes.
/// </summary>
public record SupervisorPuestoResponse(
    int Id,
    int PuestoId,
    string PuestoNombre,
    int? ClienteId,
    string? ClienteNombre,
    string SupervisorUserId,
    bool Activo);

public record GuardarSupervisorPuestoRequest(int PuestoId, int? ClienteId, string SupervisorUserId);
