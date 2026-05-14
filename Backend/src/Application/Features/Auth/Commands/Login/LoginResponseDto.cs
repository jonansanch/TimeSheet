namespace KPG.Timesheet.Application.Features.Auth.Commands.Login;

public record LoginResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string UserId,
    string Email,
    IReadOnlyList<string> Roles);
