using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Application.Features.Users.Queries.GetUsers;
using KPG.Timesheet.Domain.Constants;

namespace KPG.Timesheet.Application.Features.Users.Commands.CreateUser;

[Authorize(Roles = Roles.Admin)]
public record CreateUserCommand(string Email, string Password, string Role) : IRequest<UserAdminDto>;
