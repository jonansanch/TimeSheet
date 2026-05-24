using KPG.Timesheet.Application.Common.Models;
using KPG.Timesheet.Application.Features.Users.Queries.GetUsers;

namespace KPG.Timesheet.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<Dictionary<string, string>> GetUserEmailsAsync(IEnumerable<string> userIds, CancellationToken cancellationToken = default);

    Task<UsersPageDto> GetUsersAsync(
        int pageNumber,
        int pageSize,
        string? sortBy,
        bool sortDescending,
        CancellationToken cancellationToken = default);

    Task<(Result Result, UserAdminDto? User)> CreateUserAsync(string email, string password, string role, string? nombreCompleto = null);

    Task<Result> ActivateUserAsync(string userId);

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
}

public record UserCredentialsResult(string UserId, string Email, IReadOnlyList<string> Roles);
