using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Constants;

namespace KPG.Timesheet.Application.Features.Users.Queries.GetUsers;

[Authorize(Roles = Roles.Admin)]
public record GetUsersQuery(
    int PageNumber = 1,
    int PageSize = 25,
    string? SortBy = "email",
    bool SortDescending = false) : IRequest<UsersPageDto>;
