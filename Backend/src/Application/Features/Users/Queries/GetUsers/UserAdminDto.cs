namespace KPG.Timesheet.Application.Features.Users.Queries.GetUsers;

public record UserAdminDto(
    string Id,
    string Email,
    string? NombreCompleto,
    bool IsActive,
    string Role,
    DateTimeOffset Created,
    DateTimeOffset? DeactivatedAt);
