using System.Security.Cryptography;
using System.Text;
using KPG.Timesheet.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IClock _clock;

    public LogoutCommandHandler(IApplicationDbContext context, IClock clock)
    {
        _context = context;
        _clock = clock;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return;

        var tokenHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(request.RefreshToken)));

        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (token is null || token.IsRevoked)
            return;

        token.Revoke(_clock.UtcNow.UtcDateTime);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
