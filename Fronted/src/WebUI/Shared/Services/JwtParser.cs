using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace KPG.Timesheet.WebUI.Shared.Services;

public static class JwtParser
{
    public static ClaimsPrincipal ParseJwt(string token)
    {
        var parts = token.Split('.');
        if (parts.Length != 3)
            return new ClaimsPrincipal();

        var payload = parts[1];
        // Ajustar padding Base64url → Base64
        payload = payload.Replace('-', '+').Replace('_', '/');
        payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');

        var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
        var claims = new List<Claim>();

        using var doc = JsonDocument.Parse(json);
        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            // Los roles pueden venir como array o como valor único
            if (prop.Name == "role" || prop.Name == ClaimTypes.Role)
            {
                if (prop.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in prop.Value.EnumerateArray())
                        claims.Add(new Claim(ClaimTypes.Role, item.GetString()!));
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, prop.Value.GetString()!));
                }
            }
            else
            {
                var value = prop.Value.ValueKind == JsonValueKind.String
                    ? prop.Value.GetString()!
                    : prop.Value.GetRawText();
                claims.Add(new Claim(prop.Name, value));
            }
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
    }
}
