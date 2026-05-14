using MediatR;

namespace KPG.Timesheet.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<LoginResponseDto>;
