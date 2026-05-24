namespace KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

public record LoginRequest(string Email, string Password);

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string UserId,
    string Email,
    List<string> Roles,
    int WarningMinutes);

public record RefreshRequest(string RefreshToken);

public record RefreshResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    int WarningMinutes);

public record LogoutRequest(string RefreshToken);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public record ForgotPasswordRequest(string Email);

public record ResetPasswordRequest(string Email, string Token, string NewPassword);
