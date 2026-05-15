namespace KPG.Timesheet.Application.Features.Users.Queries.GetUsers;

public record UsersPageDto(
    IReadOnlyList<UserAdminDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);
