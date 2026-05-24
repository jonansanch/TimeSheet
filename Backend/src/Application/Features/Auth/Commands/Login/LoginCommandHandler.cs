using System.Security.Cryptography;
using System.Text;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Models;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Domain.Entities;
using MediatR;

namespace KPG.Timesheet.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IApplicationDbContext _context;
    private readonly JwtSettings _jwtSettings;
    private readonly IBitacoraService _bitacora;
    private readonly IClock _clock;

    public LoginCommandHandler(
        IIdentityService identityService,
        IJwtTokenService jwtTokenService,
        IApplicationDbContext context,
        Microsoft.Extensions.Options.IOptions<JwtSettings> jwtSettings,
        IBitacoraService bitacora,
        IClock clock)
    {
        _identityService = identityService;
        _jwtTokenService = jwtTokenService;
        _context = context;
        _jwtSettings = jwtSettings.Value;
        _bitacora = bitacora;
        _clock = clock;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var credentials = await _identityService.ValidateCredentialsAsync(request.Email, request.Password);

        if (credentials is null)
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        var now = _clock.UtcNow;
        var nowUtc = now.UtcDateTime;
        var accessToken = _jwtTokenService.GenerateAccessToken(credentials.UserId, credentials.Email, credentials.Roles);
        var expiresAt = now.AddMinutes(_jwtSettings.ExpirationMinutes).UtcDateTime;

        await _context.RefreshTokens
            .Where(t => t.UserId == credentials.UserId
                     && (t.RevokedAt != null || t.ExpiresAt <= nowUtc))
            .ExecuteDeleteAsync(cancellationToken);

        var rawRefreshToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawRefreshToken)));

        var refreshToken = new RefreshToken
        {
            UserId = credentials.UserId,
            TokenHash = tokenHash,
            ExpiresAt = now.AddHours(_jwtSettings.RefreshExpirationHours).UtcDateTime
        };

        _context.RefreshTokens.Add(refreshToken);
        await _bitacora.RegistrarAsync(
            TipoEventoBitacora.LoginExitoso,
            credentials.UserId, credentials.Email,
            "AspNetUsers", credentials.UserId,
            new { credentials.Email },
            cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new LoginResponseDto(
            AccessToken: accessToken,
            RefreshToken: rawRefreshToken,
            ExpiresAt: expiresAt,
            UserId: credentials.UserId,
            Email: credentials.Email,
            Roles: credentials.Roles,
            WarningMinutes: _jwtSettings.SessionWarningMinutes);
    }
}
