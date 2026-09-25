namespace KPG.Timesheet.Application.Features.Users.Queries.GetUsers;

public record UserAdminDto(
    string Id,
    string Email,
    string? NombreCompleto,
    bool IsActive,
    string Role,
    DateTimeOffset Created,
    DateTimeOffset? DeactivatedAt,
    // ── Estructura organizacional ──────────────────────────────────────────
    string? SupervisorUserId,
    string? SupervisorNombre,
    int? PuestoId,
    string? PuestoNombre,
    string? CodigoPais);
