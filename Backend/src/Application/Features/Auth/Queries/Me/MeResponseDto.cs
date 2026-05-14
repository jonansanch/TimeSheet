namespace KPG.Timesheet.Application.Features.Auth.Queries.Me;

public record MeResponseDto(string UserId, string Email, IReadOnlyList<string> Roles);
