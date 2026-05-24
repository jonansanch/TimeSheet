using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using KPG.Timesheet.Domain.Constants;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.Endpoints;

[Collection("EndpointTests")]
public class SecuredEndpointsTests : IClassFixture<KpgWebApplicationFactory>, IAsyncLifetime
{
    private readonly KpgWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public SecuredEndpointsTests(KpgWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask;

    // ── 401 sin token ──────────────────────────────────────────────────────

    [Theory]
    [InlineData("GET",    "api/registros-horas")]
    [InlineData("GET",    "api/registros-horas/recientes")]
    [InlineData("GET",    "api/modalidades")]
    [InlineData("GET",    "api/lugares-trabajo")]
    [InlineData("GET",    "api/empleados")]
    [InlineData("GET",    "api/dashboard/estado-equipo")]
    [InlineData("GET",    "api/sistema/ventana-retroactividad")]
    public async Task ProtectedEndpoint_WithoutToken_Returns401(string method, string path)
    {
        var request  = new HttpRequestMessage(new HttpMethod(method), path);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Catálogos públicos ─────────────────────────────────────────────────

    [Theory]
    [InlineData("api/modalidades/activas")]
    [InlineData("api/lugares-trabajo/activos")]
    public async Task CatalogoActivos_WithToken_Returns200(string path)
    {
        var token = await GetTokenAsync(KpgWebApplicationFactory.EmpleadoEmail);

        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── 403 por rol insuficiente ───────────────────────────────────────────

    [Theory]
    [InlineData("GET",  "api/users")]
    [InlineData("GET",  "api/empleados")]
    [InlineData("GET",  "api/dashboard/admin")]
    public async Task AdminOnlyEndpoint_WithEmpleadoToken_Returns403(string method, string path)
    {
        var token = await GetTokenAsync(KpgWebApplicationFactory.EmpleadoEmail);

        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── ChangePassword ─────────────────────────────────────────────────────

    [Fact]
    public async Task ChangePassword_WithoutToken_Returns401()
    {
        var response = await _client.PostAsJsonAsync("api/auth/change-password", new
        {
            currentPassword = "any",
            newPassword     = "any"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_WithWrongCurrentPassword_Returns400()
    {
        var token = await GetTokenAsync(KpgWebApplicationFactory.AdminEmail);

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/change-password");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new
        {
            currentPassword = "PasswordIncorrecto123!",
            newPassword     = "NuevaPassword123!"
        });

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Logout ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Logout_WithoutToken_Returns401()
    {
        var response = await _client.PostAsJsonAsync("api/auth/logout", new
        {
            refreshToken = "cualquier-token"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── ResetPassword ──────────────────────────────────────────────────────

    [Fact]
    public async Task ResetPassword_WithInvalidToken_Returns400()
    {
        var response = await _client.PostAsJsonAsync("api/auth/reset-password", new
        {
            email       = KpgWebApplicationFactory.AdminEmail,
            token       = "token-invalido",
            newPassword = "NuevaPassword123!"
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
        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("accessToken").GetString()!;
    }
}
