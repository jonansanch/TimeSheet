using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public interface IUserAdminRepository
{
    Task<UsersPageResponse> GetUsersAsync(
        int pageNumber,
        int pageSize,
        string sortBy,
        bool sortDescending,
        CancellationToken ct = default);

    Task<(bool Ok, UserAdminResponse? User, string? Error)> CreateAsync(CreateUserRequest request, CancellationToken ct = default);

    Task<(bool Ok, UserAdminResponse? User)> ActivateAsync(string id, CancellationToken ct = default);

    Task<(bool Ok, UserAdminResponse? User)> DeactivateAsync(string id, CancellationToken ct = default);

    Task<(bool Ok, UserAdminResponse? User, string? Error)> ChangeRoleAsync(
        string id,
        ChangeUserRoleRequest request,
        CancellationToken ct = default);

    Task<(bool Ok, DeleteUserResponse? Result)> DeleteAsync(string id, CancellationToken ct = default);
}
