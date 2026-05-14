namespace KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

public record LoginRequest(string Email, string Password);

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string UserId,
    string Email,
    List<string> Roles);

public record RefreshRequest(string RefreshToken);

public record RefreshResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt);

public record LogoutRequest(string RefreshToken);
