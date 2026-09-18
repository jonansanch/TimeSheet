using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace KPG.Timesheet.Infrastructure.Identity;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;
    private readonly TimeProvider _timeProvider;

    public JwtTokenService(IOptions<JwtSettings> settings, TimeProvider timeProvider)
    {
        _settings = settings.Value;
        _timeProvider = timeProvider;
    }

    /// <summary>Claim propio de KPG: nombre del jefe directo.</summary>
    public const string ClaimLider = "lider";

    public string GenerateAccessToken(string userId, string email, IEnumerable<string> roles, string? nombreCompleto = null, string? supervisorNombre = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            // La UI saluda por nombre; el email queda como respaldo para cuentas antiguas sin nombre.
            new(JwtRegisteredClaimNames.Name, string.IsNullOrWhiteSpace(nombreCompleto) ? email : nombreCompleto),
        };

        // Quien lidera al usuario, para mostrarlo al iniciar sesion sin una llamada extra.
        if (!string.IsNullOrWhiteSpace(supervisorNombre))
            claims.Add(new Claim(ClaimLider, supervisorNombre));

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: _timeProvider.GetUtcNow().AddMinutes(_settings.ExpirationMinutes).UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
