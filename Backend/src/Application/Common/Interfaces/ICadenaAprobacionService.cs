namespace KPG.Timesheet.Application.Common.Interfaces;

/// <summary>
/// Resuelve quienes deben aprobar un registro de horas, en orden.
/// Lo consume el modulo de aprobaciones; aqui solo se determina la cadena.
/// </summary>
public interface ICadenaAprobacionService
{
    Task<CadenaAprobacionDto> ResolverAsync(
        string userId,
        int proyectoId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Los tres niveles de aprobacion, en el orden en que deben ocurrir.
/// Un nivel nulo significa que no hay supervisor asignado: ese paso se salta.
/// </summary>
public record CadenaAprobacionDto(
    string? SupervisorPuestoUserId,
    string? SupervisorUsuarioUserId,
    string? SupervisorProyectoUserId)
{
    public static readonly CadenaAprobacionDto Vacia = new(null, null, null);

    /// <summary>Niveles efectivos, sin huecos y sin repetir a la misma persona dos veces seguidas.</summary>
    public IReadOnlyList<string> Niveles =>
        new[] { SupervisorPuestoUserId, SupervisorUsuarioUserId, SupervisorProyectoUserId }
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id!)
            .Aggregate(new List<string>(), (acc, id) =>
            {
                // Si la misma persona ocupa dos niveles consecutivos, aprueba una sola vez.
                if (acc.Count == 0 || !string.Equals(acc[^1], id, StringComparison.Ordinal))
                    acc.Add(id);
                return acc;
            });
}
