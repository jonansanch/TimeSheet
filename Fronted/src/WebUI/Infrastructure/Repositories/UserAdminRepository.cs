using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;
using KPG.Timesheet.WebUI.Shared.Services;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public class UserAdminRepository : IUserAdminRepository
{
    private readonly HttpClient _http;
    private readonly AuthStateService _authState;

    public UserAdminRepository(HttpClient http, AuthStateService authState)
    {
        _http = http;
        _authState = authState;
    }

    public async Task<UsersPageResponse> GetUsersAsync(
        int pageNumber,
        int pageSize,
        string sortBy,
        bool sortDescending,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return Empty(pageNumber, pageSize);

        using var message = CreateMessage(HttpMethod.Get,
            $"api/users?pageNumber={pageNumber}&pageSize={pageSize}&sortBy={Uri.EscapeDataString(sortBy)}&sortDescending={sortDescending}");

        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return Empty(pageNumber, pageSize);

        return await response.Content.ReadFromJsonAsync<UsersPageResponse>(cancellationToken: ct)
            ?? Empty(pageNumber, pageSize);
    }

    public async Task<(bool Ok, UserAdminResponse? User, string? Error)> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return (false, null, "Sesion no disponible.");

        using var message = CreateMessage(HttpMethod.Post, "api/users");
        message.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return (false, null, await ReadErrorAsync(response, ct));

        var user = await response.Content.ReadFromJsonAsync<UserAdminResponse>(cancellationToken: ct);
        return (user is not null, user, null);
    }

    public async Task<(bool Ok, UserAdminResponse? User)> ActivateAsync(string id, CancellationToken ct = default) =>
        await SendUserActionAsync(HttpMethod.Post, $"api/users/{id}/activate", ct);

    public async Task<(bool Ok, UserAdminResponse? User)> DeactivateAsync(string id, CancellationToken ct = default) =>
        await SendUserActionAsync(HttpMethod.Post, $"api/users/{id}/deactivate", ct);

    public async Task<(bool Ok, UserAdminResponse? User, string? Error)> ChangeRoleAsync(
        string id,
        ChangeUserRoleRequest request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return (false, null, "Sesion no disponible.");

        using var message = CreateMessage(HttpMethod.Put, $"api/users/{id}/role");
        message.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return (false, null, await ReadErrorAsync(response, ct));

        var user = await response.Content.ReadFromJsonAsync<UserAdminResponse>(cancellationToken: ct);
        return (user is not null, user, null);
    }

    public async Task<(bool Ok, DeleteUserResponse? Result)> DeleteAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return (false, null);

        using var message = CreateMessage(HttpMethod.Delete, $"api/users/{id}");
        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return (false, null);

        var result = await response.Content.ReadFromJsonAsync<DeleteUserResponse>(cancellationToken: ct);
        return (result is not null, result);
    }

    private async Task<(bool Ok, UserAdminResponse? User)> SendUserActionAsync(HttpMethod method, string url, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_authState.AccessToken))
            return (false, null);

        using var message = CreateMessage(method, url);
        var response = await _http.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return (false, null);

        var user = await response.Content.ReadFromJsonAsync<UserAdminResponse>(cancellationToken: ct);
        return (user is not null, user);
    }

    private HttpRequestMessage CreateMessage(HttpMethod method, string url)
    {
        var message = new HttpRequestMessage(method, url);
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.AccessToken);
        return message;
    }

    private static UsersPageResponse Empty(int pageNumber, int pageSize) => new([], 0, pageNumber, pageSize);

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var json = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
            var root = json.RootElement;

            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                var messages = new List<string>();
                foreach (var property in errors.EnumerateObject())
                {
                    if (property.Value.ValueKind != JsonValueKind.Array)
                        continue;

                    foreach (var item in property.Value.EnumerateArray())
                    {
                        var message = item.GetString();
                        if (!string.IsNullOrWhiteSpace(message))
                            messages.Add(message);
                    }
                }

                if (messages.Count > 0)
                    return string.Join(" ", messages);
            }

            if (root.TryGetProperty("detail", out var detail) && !string.IsNullOrWhiteSpace(detail.GetString()))
                return detail.GetString()!;

            if (root.TryGetProperty("title", out var title) && !string.IsNullOrWhiteSpace(title.GetString()))
                return title.GetString()!;
        }
        catch
        {
            // Best-effort error message for UI.
        }

        return "No fue posible completar la operacion.";
    }
}
