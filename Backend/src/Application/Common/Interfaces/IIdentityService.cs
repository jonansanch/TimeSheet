using KPG.Timesheet.Application.Common.Models;

namespace KPG.Timesheet.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password);

    Task<Result> DeleteUserAsync(string userId);

    Task<UserCredentialsResult?> ValidateCredentialsAsync(string email, string password);

    Task<UserCredentialsResult?> ValidateCredentialsByIdAsync(string userId);
}

public record UserCredentialsResult(string UserId, string Email, IReadOnlyList<string> Roles);
