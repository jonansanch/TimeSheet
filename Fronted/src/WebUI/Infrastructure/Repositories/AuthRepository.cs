using System.Net.Http.Headers;
using System.Net.Http.Json;
using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly HttpClient _http;

    public AuthRepository(HttpClient http)
    {
        _http = http;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await _http.PostAsJsonAsync("api/auth/login", request, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken);
    }

    public async Task<RefreshResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await _http.PostAsJsonAsync(
                "api/auth/refresh",
                new RefreshRequest(refreshToken),
                cancellationToken);
        }
        catch (HttpRequestException)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<RefreshResponse>(cancellationToken: cancellationToken);
    }

    public async Task LogoutAsync(string refreshToken, string accessToken, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new LogoutRequest(refreshToken));

        await _http.SendAsync(request, cancellationToken);
    }

    public async Task<(bool Ok, string? Error)> ChangePasswordAsync(string currentPassword, string newPassword, string accessToken, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/change-password");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new ChangePasswordRequest(currentPassword, newPassword));

        HttpResponseMessage response;
        try { response = await _http.SendAsync(request, cancellationToken); }
        catch (HttpRequestException) { return (false, null); }

        if (response.IsSuccessStatusCode)
            return (true, null);

        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetail>(cancellationToken: cancellationToken);
            return (false, problem?.Detail);
        }
        catch { return (false, null); }
    }

    public async Task<bool> ForgotPasswordAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/forgot-password", new ForgotPasswordRequest(email), cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<(bool Ok, string? Error)> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/reset-password", new ResetPasswordRequest(email, token, newPassword), cancellationToken);
            if (response.IsSuccessStatusCode)
                return (true, null);

            var problem = await response.Content.ReadFromJsonAsync<ProblemDetail>(cancellationToken: cancellationToken);
            return (false, problem?.Detail);
        }
        catch { return (false, null); }
    }

    private sealed record ProblemDetail(string? Detail);
}
