using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.Endpoints;

[Collection("EndpointTests")]
public class AuthEndpointsTests : IClassFixture<KpgWebApplicationFactory>, IAsyncLifetime
{
    private readonly KpgWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthEndpointsTests(KpgWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask;

    // ── Login ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_WithValidCredentials_Returns200WithToken()
    {
        var response = await _client.PostAsJsonAsync("api/auth/login", new
        {
            email    = KpgWebApplicationFactory.AdminEmail,
            password = KpgWebApplicationFactory.TestPassword
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await ParseJsonAsync(response);
        body.RootElement.GetProperty("accessToken").GetString().Should().NotBeNullOrWhiteSpace();
        body.RootElement.GetProperty("refreshToken").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var response = await _client.PostAsJsonAsync("api/auth/login", new
        {
            email    = KpgWebApplicationFactory.AdminEmail,
            password = "WrongPassword!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_Returns401()
    {
        var response = await _client.PostAsJsonAsync("api/auth/login", new
        {
            email    = "noexiste@test.com",
            password = KpgWebApplicationFactory.TestPassword
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithInvalidEmailFormat_Returns400()
    {
        var response = await _client.PostAsJsonAsync("api/auth/login", new
        {
            email    = "no-es-un-email",
            password = KpgWebApplicationFactory.TestPassword
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Me ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Me_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("api/auth/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_WithValidToken_Returns200WithEmailAndRoles()
    {
        var token = await GetTokenAsync(KpgWebApplicationFactory.AdminEmail);

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/auth/me");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await ParseJsonAsync(response);
        body.RootElement.GetProperty("email").GetString()
            .Should().Be(KpgWebApplicationFactory.AdminEmail);
        body.RootElement.GetProperty("roles").EnumerateArray()
            .Should().Contain(e => e.GetString() == "Admin");
    }

    // ── Refresh ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Refresh_WithInvalidToken_Returns401()
    {
        var response = await _client.PostAsJsonAsync("api/auth/refresh", new
        {
            refreshToken = "token-invalido"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithValidRefreshToken_Returns200WithNewTokens()
    {
        var loginResponse = await _client.PostAsJsonAsync("api/auth/login", new
        {
            email    = KpgWebApplicationFactory.AdminEmail,
            password = KpgWebApplicationFactory.TestPassword
        });
        var loginBody    = await ParseJsonAsync(loginResponse);
        var refreshToken = loginBody.RootElement.GetProperty("refreshToken").GetString()!;

        var refreshResponse = await _client.PostAsJsonAsync("api/auth/refresh", new
        {
            refreshToken
        });

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshBody = await ParseJsonAsync(refreshResponse);
        refreshBody.RootElement.GetProperty("accessToken").GetString().Should().NotBeNullOrWhiteSpace();
    }

    // ── ForgotPassword ─────────────────────────────────────────────────────

    [Fact]
    public async Task ForgotPassword_WithExistingEmail_Returns204()
    {
        var response = await _client.PostAsJsonAsync("api/auth/forgot-password", new
        {
            email = KpgWebApplicationFactory.AdminEmail
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ForgotPassword_WithNonExistentEmail_Returns204()
    {
        // Siempre 204 para no revelar si el email existe
        var response = await _client.PostAsJsonAsync("api/auth/forgot-password", new
        {
            email = "noexiste@test.com"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ForgotPassword_WithInvalidEmail_Returns400()
    {
        var response = await _client.PostAsJsonAsync("api/auth/forgot-password", new
        {
            email = "no-es-email"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private async Task<string> GetTokenAsync(string email)
    {
        var response = await _client.PostAsJsonAsync("api/auth/login", new
        {
            email,
            password = KpgWebApplicationFactory.TestPassword
        });
        var body = await ParseJsonAsync(response);
        return body.RootElement.GetProperty("accessToken").GetString()!;
    }

    private static async Task<JsonDocument> ParseJsonAsync(HttpResponseMessage response)
        => JsonDocument.Parse(await response.Content.ReadAsStringAsync());
}
