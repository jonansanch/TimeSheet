using MediatR;

namespace KPG.Timesheet.Application.Features.Auth.Commands.Logout;

public record LogoutCommand(string RefreshToken) : IRequest;
