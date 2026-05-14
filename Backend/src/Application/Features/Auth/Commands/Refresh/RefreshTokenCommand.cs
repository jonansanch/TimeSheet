using MediatR;

namespace KPG.Timesheet.Application.Features.Auth.Commands.Refresh;

public record RefreshTokenCommand(string RefreshToken) : IRequest<RefreshTokenResponseDto>;
