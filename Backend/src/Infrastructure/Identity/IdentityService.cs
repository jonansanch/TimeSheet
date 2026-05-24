using System.Data;
using Dapper;
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
    private readonly IDbConnection _db;

    public IdentityService(UserManager<ApplicationUser> userManager, IDbConnection db)
    {
        _userManager = userManager;
        _db = db;
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

        var orderBy = (sortBy?.ToLowerInvariant(), sortDescending) switch
        {
            ("estado" or "isactive", false) => "u.IsActive ASC,  u.Email ASC",
            ("estado" or "isactive", true)  => "u.IsActive DESC, u.Email ASC",
            ("rol"    or "role",     false) => "r.Name    ASC,  u.Email ASC",
            ("rol"    or "role",     true)  => "r.Name    DESC, u.Email ASC",
            (_,                      false) => "u.Email ASC",
            (_,                      true)  => "u.Email DESC",
        };

        var sql = $"""
            SELECT COUNT(*) FROM AspNetUsers;

            SELECT Id, Email, NombreCompleto, IsActive, Role, Created, DeactivatedAt
            FROM (
                SELECT ROW_NUMBER() OVER (ORDER BY {orderBy}) AS RowNum,
                       u.Id,
                       COALESCE(u.Email, '')  AS Email,
                       u.NombreCompleto,
                       u.IsActive,
                       COALESCE(r.Name, '')   AS Role,
                       u.Created,
                       u.DeactivatedAt
                FROM   AspNetUsers      u
                LEFT   JOIN AspNetUserRoles ur ON u.Id      = ur.UserId
                LEFT   JOIN AspNetRoles     r  ON ur.RoleId = r.Id
            ) AS Paged
            WHERE  RowNum > @Skip AND RowNum <= @Skip + @Take
            ORDER  BY RowNum;
            """;

        using var multi = await _db.QueryMultipleAsync(sql, new
        {
            Skip = (pageNumber - 1) * pageSize,
            Take = pageSize
        });

        var total = await multi.ReadSingleAsync<int>();
        var rows  = await multi.ReadAsync<UserAdminRow>();
        var items = rows.Select(r => new UserAdminDto(r.Id, r.Email, r.NombreCompleto, r.IsActive, r.Role, r.Created, r.DeactivatedAt)).ToList();

        return new UsersPageDto(items, total, pageNumber, pageSize);
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

    public async Task<Result> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Result.Failure(["Usuario no encontrado."]);

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return result.ToApplicationResult();
    }

    public async Task<(bool Found, string? Token, string? Email)> GeneratePasswordResetTokenAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null || !user.IsActive)
            return (false, null, null);

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        return (true, token, user.Email);
    }

    public async Task<Result> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null || !user.IsActive)
            return Result.Failure(["Usuario no encontrado."]);

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        return result.ToApplicationResult();
    }

    public async Task<Result> AdminResetPasswordAsync(string userId, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Result.Failure(["Usuario no encontrado."]);

        var removeResult = await _userManager.RemovePasswordAsync(user);
        if (!removeResult.Succeeded)
            return removeResult.ToApplicationResult();

        var addResult = await _userManager.AddPasswordAsync(user, newPassword);
        return addResult.ToApplicationResult();
    }

    private sealed class UserAdminRow
    {
        public string Id { get; set; } = "";
        public string Email { get; set; } = "";
        public string? NombreCompleto { get; set; }
        public bool IsActive { get; set; }
        public string Role { get; set; } = "";
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? DeactivatedAt { get; set; }
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
