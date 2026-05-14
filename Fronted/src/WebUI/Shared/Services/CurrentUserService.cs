using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace KPG.Timesheet.WebUI.Shared.Services;

public class CurrentUserService
{
    private readonly AuthenticationStateProvider _authStateProvider;

    public CurrentUserService(AuthenticationStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
    }

    public async Task<ClaimsPrincipal> GetUserAsync()
    {
        var state = await _authStateProvider.GetAuthenticationStateAsync();
        return state.User;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var user = await GetUserAsync();
        return user.Identity?.IsAuthenticated ?? false;
    }

    public async Task<bool> IsInRoleAsync(string role)
    {
        var user = await GetUserAsync();
        return user.IsInRole(role);
    }

    public async Task<string?> GetEmailAsync()
    {
        var user = await GetUserAsync();
        return user.FindFirst(ClaimTypes.Email)?.Value
            ?? user.FindFirst("email")?.Value;
    }

    public async Task<IReadOnlyList<string>> GetRolesAsync()
    {
        var user = await GetUserAsync();
        return user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList().AsReadOnly();
    }
}
