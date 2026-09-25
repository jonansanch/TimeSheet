using KPG.Timesheet.Application.Common.Models;
using KPG.Timesheet.Application.Features.Users.Queries.GetOrganigrama;
using KPG.Timesheet.Application.Features.Users.Queries.GetUsers;

namespace KPG.Timesheet.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<Dictionary<string, string>> GetUserEmailsAsync(IEnumerable<string> userIds, CancellationToken cancellationToken = default);

    /// <summary>Nombre completo de cada usuario, con el correo como respaldo.</summary>
    Task<Dictionary<string, string>> GetUserNamesAsync(IEnumerable<string> userIds, CancellationToken cancellationToken = default);

    Task<UsersPageDto> GetUsersAsync(
        int pageNumber,
        int pageSize,
        string? sortBy,
        bool sortDescending,
        CancellationToken cancellationToken = default);

    Task<(Result Result, UserAdminDto? User)> CreateUserAsync(
        string email, string password, string role, string? nombreCompleto = null, string? codigoPais = null);

    Task<Result> ActivateUserAsync(string userId);

    /// <summary>
    /// Asigna jefe directo y puesto. Rechaza auto-referencias y ciclos en el organigrama:
    /// una jerarquia circular colgaria tanto el arbol como la cadena de aprobacion.
    /// </summary>
    Task<(Result Result, UserAdminDto? User)> AsignarEstructuraAsync(
        string userId,
        string? supervisorUserId,
        int? puestoId,
        string? codigoPais = null,
        bool actualizarCodigoPais = false,
        CancellationToken cancellationToken = default);

    /// <summary>Usuarios activos en forma de arbol para el organigrama.</summary>
    Task<IReadOnlyList<OrganigramaNodoDto>> GetOrganigramaAsync(CancellationToken cancellationToken = default);

    Task<Result> DeactivateUserAsync(string userId, string? deactivatedBy);

    Task<(Result Result, UserAdminDto? User)> ChangeUserRoleAsync(
        string userId,
        string role,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteUserAsync(string userId);

    Task<UserCredentialsResult?> ValidateCredentialsAsync(string email, string password);

    Task<UserCredentialsResult?> ValidateCredentialsByIdAsync(string userId);

    Task<Result> ChangePasswordAsync(string userId, string currentPassword, string newPassword);

    Task<(bool Found, string? Token, string? Email)> GeneratePasswordResetTokenAsync(string email);

    Task<Result> ResetPasswordAsync(string email, string token, string newPassword);

    Task<Result> AdminResetPasswordAsync(string userId, string newPassword);
}

public record UserCredentialsResult(
    string UserId,
    string Email,
    IReadOnlyList<string> Roles,
    string? NombreCompleto,
    string? SupervisorNombre,
    string? PuestoNombre = null);
