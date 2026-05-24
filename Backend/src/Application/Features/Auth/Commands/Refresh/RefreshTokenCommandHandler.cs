using System.Security.Cryptography;
using System.Text;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Models;
using KPG.Timesheet.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KPG.Timesheet.Application.Features.Auth.Commands.Refresh;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly JwtSettings _jwtSettings;
    private readonly IClock _clock;

    public RefreshTokenCommandHandler(
        IApplicationDbContext context,
        IIdentityService identityService,
        IJwtTokenService jwtTokenService,
        IOptions<JwtSettings> jwtSettings,
        IClock clock)
    {
        _context = context;
        _identityService = identityService;
        _jwtTokenService = jwtTokenService;
        _jwtSettings = jwtSettings.Value;
        _clock = clock;
    }

    public async Task<RefreshTokenResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(request.RefreshToken)));

        var existingToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (existingToken is null || !existingToken.IsActive)
            throw new UnauthorizedAccessException("Refresh token inválido o expirado.");

        var credentials = await _identityService.ValidateCredentialsByIdAsync(existingToken.UserId);
        if (credentials is null)
            throw new UnauthorizedAccessException("Usuario inactivo.");

        var now = _clock.UtcNow;
        var accessToken = _jwtTokenService.GenerateAccessToken(
            credentials.UserId, credentials.Email, credentials.Roles);
        var expiresAt = now.AddMinutes(_jwtSettings.ExpirationMinutes).UtcDateTime;

        // Rotación: revocar el token anterior y emitir uno nuevo
        existingToken.Revoke(now.UtcDateTime);

        var rawNewToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var newTokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawNewToken)));

        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = existingToken.UserId,
            TokenHash = newTokenHash,
            ExpiresAt = now.AddHours(_jwtSettings.RefreshExpirationHours).UtcDateTime
        });

        await _context.SaveChangesAsync(cancellationToken);

        return new RefreshTokenResponseDto(accessToken, rawNewToken, expiresAt, _jwtSettings.SessionWarningMinutes);
    }
}
