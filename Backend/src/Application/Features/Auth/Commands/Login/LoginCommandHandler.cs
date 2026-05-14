using System.Security.Cryptography;
using System.Text;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Models;
using KPG.Timesheet.Domain.Entities;
using MediatR;

namespace KPG.Timesheet.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IApplicationDbContext _context;
    private readonly JwtSettings _jwtSettings;

    public LoginCommandHandler(
        IIdentityService identityService,
        IJwtTokenService jwtTokenService,
        IApplicationDbContext context,
        Microsoft.Extensions.Options.IOptions<JwtSettings> jwtSettings)
    {
        _identityService = identityService;
        _jwtTokenService = jwtTokenService;
        _context = context;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var credentials = await _identityService.ValidateCredentialsAsync(request.Email, request.Password);

        if (credentials is null)
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        var accessToken = _jwtTokenService.GenerateAccessToken(credentials.UserId, credentials.Email, credentials.Roles);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

        var rawRefreshToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawRefreshToken)));

        var refreshToken = new RefreshToken
        {
            UserId = credentials.UserId,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddHours(_jwtSettings.RefreshExpirationHours)
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new LoginResponseDto(
            AccessToken: accessToken,
            RefreshToken: rawRefreshToken,
            ExpiresAt: expiresAt,
            UserId: credentials.UserId,
            Email: credentials.Email,
            Roles: credentials.Roles);
    }
}
