using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Application.Features.Users.Queries.GetUsers;
using KPG.Timesheet.Domain.Constants;

namespace KPG.Timesheet.Application.Features.Users.Commands.ActivateUser;

[Authorize(Roles = Roles.Admin)]
public record ActivateUserCommand(string Id) : IRequest<UserAdminDto>;
