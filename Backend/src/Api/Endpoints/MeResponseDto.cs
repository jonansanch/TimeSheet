namespace KPG.Timesheet.Api.Endpoints;

public record MeResponseDto(string UserId, string Email, IReadOnlyList<string> Roles);
