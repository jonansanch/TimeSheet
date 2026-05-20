using KPG.Timesheet.Application.Common.Models;
using KPG.Timesheet.Application.Features.Users.Queries.GetUsers;

namespace KPG.Timesheet.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password);

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

    Task<Result> DeleteUserHardAsync(string userId);

    Task<UserCredentialsResult?> ValidateCredentialsAsync(string email, string password);

    Task<UserCredentialsResult?> ValidateCredentialsByIdAsync(string userId);
}

public record UserCredentialsResult(string UserId, string Email, IReadOnlyList<string> Roles);
