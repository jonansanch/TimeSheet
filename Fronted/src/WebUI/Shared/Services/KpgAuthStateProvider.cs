using System.Security.Claims;
using KPG.Timesheet.WebUI.Infrastructure.Repositories;
using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace KPG.Timesheet.WebUI.Shared.Services;

public class KpgAuthStateProvider : AuthenticationStateProvider
{
    private const string RefreshTokenKey = "kpg_rt";

    private readonly IAuthRepository _authRepository;
    private readonly AuthStateService _authState;
    private readonly SessionTimeoutService _sessionTimeout;
    private readonly IJSRuntime _js;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    private readonly AuthenticationState _anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public KpgAuthStateProvider(
        IAuthRepository authRepository,
        AuthStateService authState,
        SessionTimeoutService sessionTimeout,
        IJSRuntime js)
    {
        _authRepository = authRepository;
        _authState = authState;
        _sessionTimeout = sessionTimeout;
        _js = js;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_authState.IsAuthenticated)
            return BuildState(_authState.AccessToken!);

        await _refreshLock.WaitAsync();
        try
        {
            if (_authState.IsAuthenticated)
                return BuildState(_authState.AccessToken!);

            return await TryRestoreSessionAsync();
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    public async Task LoginAsync(string email, string password)
    {
        var response = await _authRepository.LoginAsync(new LoginRequest(email, password));

        if (response is null)
            throw new UnauthorizedAccessException("Credenciales invalidas.");

        _authState.SetToken(response.AccessToken);
        await SetRefreshTokenInStorageAsync(response.RefreshToken);
        _sessionTimeout.StartTracking(response.ExpiresAt, response.WarningMinutes);

        NotifyAuthenticationStateChanged(Task.FromResult(BuildState(response.AccessToken)));
    }

    public async Task LogoutAsync()
    {
        var refreshToken = await GetRefreshTokenFromStorageAsync();

        if (!string.IsNullOrEmpty(refreshToken) && _authState.IsAuthenticated)
            await _authRepository.LogoutAsync(refreshToken, _authState.AccessToken!);

        _sessionTimeout.StopTracking();
        _authState.ClearToken();
        await RemoveRefreshTokenFromStorageAsync();

        NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
    }

    public async Task<bool> RefreshSessionAsync()
    {
        var refreshToken = await GetRefreshTokenFromStorageAsync();
        if (string.IsNullOrEmpty(refreshToken)) return false;

        var response = await _authRepository.RefreshAsync(refreshToken);
        if (response is null) return false;

        _authState.SetToken(response.AccessToken);
        await SetRefreshTokenInStorageAsync(response.RefreshToken);
        _sessionTimeout.StartTracking(response.ExpiresAt, response.WarningMinutes);

        NotifyAuthenticationStateChanged(Task.FromResult(BuildState(response.AccessToken)));
        return true;
    }

    private async Task<AuthenticationState> TryRestoreSessionAsync()
    {
        var refreshToken = await GetRefreshTokenFromStorageAsync();
        if (string.IsNullOrEmpty(refreshToken))
            return _anonymous;

        var response = await _authRepository.RefreshAsync(refreshToken);
        if (response is null)
        {
            await RemoveRefreshTokenFromStorageAsync();
            return _anonymous;
        }

        _authState.SetToken(response.AccessToken);
        await SetRefreshTokenInStorageAsync(response.RefreshToken);
        _sessionTimeout.StartTracking(response.ExpiresAt, response.WarningMinutes);

        return BuildState(response.AccessToken);
    }

    private static AuthenticationState BuildState(string accessToken)
    {
        var principal = JwtParser.ParseJwt(accessToken);
        return new AuthenticationState(principal);
    }

    private async Task<string?> GetRefreshTokenFromStorageAsync()
    {
        try { return await _js.InvokeAsync<string?>("sessionStorage.getItem", RefreshTokenKey); }
        catch { return null; }
    }

    private async Task SetRefreshTokenInStorageAsync(string token)
    {
        try { await _js.InvokeVoidAsync("sessionStorage.setItem", RefreshTokenKey, token); }
        catch { }
    }

    private async Task RemoveRefreshTokenFromStorageAsync()
    {
        try { await _js.InvokeVoidAsync("sessionStorage.removeItem", RefreshTokenKey); }
        catch { }
    }
}
