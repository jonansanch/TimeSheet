namespace KPG.Timesheet.Application.Features.Auth.Commands.Refresh;

public record RefreshTokenResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    int WarningMinutes);
