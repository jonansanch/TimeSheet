namespace KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

public record UserAdminResponse(
    string Id,
    string Email,
    string? NombreCompleto,
    bool IsActive,
    string Role,
    DateTimeOffset Created,
    DateTimeOffset? DeactivatedAt,
    string? SupervisorUserId,
    string? SupervisorNombre,
    int? PuestoId,
    string? PuestoNombre,
    string? CodigoPais);

public record UsersPageResponse(
    List<UserAdminResponse> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);

public record CreateUserRequest(
    string Email, string Password, string Role, string? NombreCompleto = null, string? CodigoPais = null);

public record ChangeUserRoleRequest(string Role);

public record AsignarEstructuraRequest(
    string? SupervisorUserId,
    int? PuestoId,
    string? CodigoPais = null,
    bool ActualizarCodigoPais = false);

/// <summary>Nodo plano del organigrama; el arbol se arma enlazando por SupervisorUserId.</summary>
public record OrganigramaNodoResponse(
    string UserId,
    string Nombre,
    string Email,
    string Rol,
    string? PuestoNombre,
    string? SupervisorUserId,
    string? CodigoPais);

public record DeleteUserResponse(bool WasHardDeleted);
public record AdminResetPasswordRequest(string NewPassword);
