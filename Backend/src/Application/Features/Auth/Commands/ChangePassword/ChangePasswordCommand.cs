using KPG.Timesheet.Application.Common.Models;
using MediatR;

namespace KPG.Timesheet.Application.Features.Auth.Commands.ChangePassword;

public record ChangePasswordCommand(string CurrentPassword, string NewPassword) : IRequest<Result>;
