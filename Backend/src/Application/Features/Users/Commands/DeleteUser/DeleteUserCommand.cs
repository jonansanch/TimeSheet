using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Constants;

namespace KPG.Timesheet.Application.Features.Users.Commands.DeleteUser;

[Authorize(Roles = Roles.Admin)]
public record DeleteUserCommand(string Id) : IRequest<DeleteUserDto>;
