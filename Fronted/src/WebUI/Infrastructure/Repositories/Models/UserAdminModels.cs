namespace KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

public record UserAdminResponse(
    string Id,
    string Email,
    bool IsActive,
    string Role,
    DateTimeOffset Created,
    DateTimeOffset? DeactivatedAt);

public record UsersPageResponse(
    List<UserAdminResponse> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);

public record CreateUserRequest(string Email, string Password, string Role);

public record ChangeUserRoleRequest(string Role);

public record DeleteUserResponse(bool WasHardDeleted);
