using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Models;
using KPG.Timesheet.Application.Features.Users.Queries.GetUsers;
using KPG.Timesheet.Domain.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
    }

    public async Task<Dictionary<string, string>> GetUserEmailsAsync(IEnumerable<string> userIds, CancellationToken cancellationToken = default)
    {
        var ids = userIds.ToList();
        return await _userManager.Users
            .Where(u => ids.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Email ?? u.Id, cancellationToken);
    }

    public async Task<UsersPageDto> GetUsersAsync(
        int pageNumber,
        int pageSize,
        string? sortBy,
        bool sortDescending,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        // 1 query: all users
        var users = await _userManager.Users.ToListAsync(cancellationToken);

        // 4 queries (one per role) — O(roles) instead of O(2N+1)
        var allRoles = new[] { Roles.Admin, Roles.Gerente, Roles.Supervisor, Roles.Empleado };
        var userRoleMap = new Dictionary<string, string>(users.Count);
        foreach (var roleName in allRoles)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
            foreach (var u in usersInRole)
                userRoleMap.TryAdd(u.Id, roleName);
        }

        var items = users.Select(u => new UserAdminDto(
            u.Id,
            u.Email ?? string.Empty,
            u.NombreCompleto,
            u.IsActive,
            userRoleMap.GetValueOrDefault(u.Id, string.Empty),
            u.Created,
            u.DeactivatedAt)).ToList();

        var ordered = (sortBy?.ToLowerInvariant()) switch
        {
            "estado" or "isactive" => sortDescending
                ? items.OrderByDescending(u => u.IsActive).ThenBy(u => u.Email)
                : items.OrderBy(u => u.IsActive).ThenBy(u => u.Email),
            "rol" or "role" => sortDescending
                ? items.OrderByDescending(u => u.Role).ThenBy(u => u.Email)
                : items.OrderBy(u => u.Role).ThenBy(u => u.Email),
            _ => sortDescending
                ? items.OrderByDescending(u => u.Email)
                : items.OrderBy(u => u.Email)
        };

        var total = items.Count;
        var pageItems = ordered
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new UsersPageDto(pageItems, total, pageNumber, pageSize);
    }

    public async Task<(Result Result, UserAdminDto? User)> CreateUserAsync(string email, string password, string role, string? nombreCompleto = null)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            NombreCompleto = string.IsNullOrWhiteSpace(nombreCompleto) ? null : nombreCompleto.Trim(),
            IsActive = true,
            Created = DateTimeOffset.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
            return (createResult.ToApplicationResult(), null);

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return (roleResult.ToApplicationResult(), null);
        }

        return (Result.Success(), new UserAdminDto(user.Id, user.Email ?? email, user.NombreCompleto, user.IsActive, role, user.Created, user.DeactivatedAt));
    }

    public async Task<Result> ActivateUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Result.Failure(["Usuario no encontrado."]);

        user.IsActive = true;
        user.DeactivatedAt = null;
        user.DeactivatedBy = null;

        return (await _userManager.UpdateAsync(user)).ToApplicationResult();
    }

    public async Task<Result> DeactivateUserAsync(string userId, string? deactivatedBy)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Result.Failure(["Usuario no encontrado."]);

        user.IsActive = false;
        user.DeactivatedAt = DateTimeOffset.UtcNow;
        user.DeactivatedBy = deactivatedBy;

        return (await _userManager.UpdateAsync(user)).ToApplicationResult();
    }

    public async Task<(Result Result, UserAdminDto? User)> ChangeUserRoleAsync(
        string userId,
        string role,
        CancellationToken cancellationToken = default)
    {
        if (!IsValidRole(role))
            return (Result.Failure(["El rol seleccionado no es valido."]), null);

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return (Result.Failure(["Usuario no encontrado."]), null);

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Count == 1 && currentRoles[0] == role)
            return (Result.Success(), ToUserAdminDto(user, role));

        if (currentRoles.Contains(Roles.Admin) && role != Roles.Admin && !await HasAnotherActiveAdminAsync(user.Id, cancellationToken))
            return (Result.Failure(["Debe existir al menos un administrador activo."]), null);

        if (currentRoles.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
                return (removeResult.ToApplicationResult(), null);
        }

        var addResult = await _userManager.AddToRoleAsync(user, role);
        if (!addResult.Succeeded)
            return (addResult.ToApplicationResult(), null);

        return (Result.Success(), ToUserAdminDto(user, role));
    }

    public async Task<Result> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null ? await DeleteUserAsync(user) : Result.Success();
    }

    public async Task<Result> DeleteUserAsync(ApplicationUser user)
    {
        var result = await _userManager.DeleteAsync(user);

        return result.ToApplicationResult();
    }

    public async Task<UserCredentialsResult?> ValidateCredentialsAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || !user.IsActive || !await _userManager.CheckPasswordAsync(user, password))
            return null;

        var roles = await _userManager.GetRolesAsync(user);
        return new UserCredentialsResult(user.Id, user.Email!, roles.ToList().AsReadOnly());
    }

    public async Task<UserCredentialsResult?> ValidateCredentialsByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || !user.IsActive)
            return null;

        var roles = await _userManager.GetRolesAsync(user);
        return new UserCredentialsResult(user.Id, user.Email!, roles.ToList().AsReadOnly());
    }

    private static bool IsValidRole(string role) =>
        role is Roles.Admin or Roles.Gerente or Roles.Supervisor or Roles.Empleado;

    private static UserAdminDto ToUserAdminDto(ApplicationUser user, string role) =>
        new(user.Id, user.Email ?? string.Empty, user.NombreCompleto, user.IsActive, role, user.Created, user.DeactivatedAt);

    private async Task<bool> HasAnotherActiveAdminAsync(string userId, CancellationToken cancellationToken)
    {
        var admins = await _userManager.GetUsersInRoleAsync(Roles.Admin);
        return admins.Any(u => u.IsActive && u.Id != userId);
    }
}
