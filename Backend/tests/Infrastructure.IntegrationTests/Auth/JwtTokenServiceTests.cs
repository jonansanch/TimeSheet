using System.IdentityModel.Tokens.Jwt;
using KPG.Timesheet.Application.Common.Models;
using KPG.Timesheet.Infrastructure.Identity;
using Microsoft.Extensions.Options;

namespace KPG.Timesheet.Infrastructure.IntegrationTests.Auth;

public class JwtTokenServiceTests
{
    private static readonly string[] RolesEmpleado = ["Empleado"];

    [Fact]
    public void GenerateAccessToken_ShouldIncludeNombreCompletoAsNameClaim()
    {
        var token = Leer(CreateService().GenerateAccessToken(
            "user-1", "juan@kpg.com", RolesEmpleado, "Juan Pérez"));

        Claim(token, JwtRegisteredClaimNames.Name).Should().Be("Juan Pérez");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GenerateAccessToken_WhenNombreCompletoIsMissing_ShouldFallBackToEmail(string? nombre)
    {
        // Las cuentas creadas antes de que el nombre fuera obligatorio no tienen uno:
        // la UI debe seguir mostrando algo identificable.
        var token = Leer(CreateService().GenerateAccessToken(
            "user-1", "juan@kpg.com", RolesEmpleado, nombre));

        Claim(token, JwtRegisteredClaimNames.Name).Should().Be("juan@kpg.com");
    }

    [Fact]
    public void GenerateAccessToken_ShouldIncludeLiderClaim()
    {
        var token = Leer(CreateService().GenerateAccessToken(
            "user-1", "juan@kpg.com", RolesEmpleado, "Juan Pérez", "Miguel Torres"));

        Claim(token, JwtTokenService.ClaimLider).Should().Be("Miguel Torres");
    }

    [Fact]
    public void GenerateAccessToken_WhenUserHasNoManager_ShouldOmitLiderClaim()
    {
        // Sin claim la UI no pinta la linea del lider, en lugar de mostrarla vacia.
        var token = Leer(CreateService().GenerateAccessToken(
            "user-1", "juan@kpg.com", RolesEmpleado, "Juan Pérez", null));

        Claim(token, JwtTokenService.ClaimLider).Should().BeNull();
    }

    [Fact]
    public void GenerateAccessToken_ShouldKeepSubEmailAndRoles()
    {
        var token = Leer(CreateService().GenerateAccessToken(
            "user-1", "juan@kpg.com", ["Empleado", "Supervisor"], "Juan Pérez"));

        Claim(token, JwtRegisteredClaimNames.Sub).Should().Be("user-1");
        Claim(token, JwtRegisteredClaimNames.Email).Should().Be("juan@kpg.com");
        token.Claims.Where(c => c.Type == "role" || c.Type.EndsWith("/role"))
             .Select(c => c.Value)
             .Should().BeEquivalentTo("Empleado", "Supervisor");
    }

    [Fact]
    public void GenerateAccessToken_ConPuesto_ShouldIncluirElClaim()
    {
        // El formulario de registro precarga el Recurso desde este claim.
        var token = Leer(CreateService().GenerateAccessToken(
            "user-1", "juan@kpg.com", ["Empleado"], "Juan Pérez", "Ana Jefa", "Consultor SAP"));

        Claim(token, JwtTokenService.ClaimPuesto).Should().Be("Consultor SAP");
    }

    [Fact]
    public void GenerateAccessToken_SinPuesto_ShouldOmitirElClaim()
    {
        // Hoy casi nadie tiene puesto asignado: el claim se omite y el formulario
        // sigue dejando elegir el recurso a mano, en vez de quedar bloqueado.
        var token = Leer(CreateService().GenerateAccessToken(
            "user-1", "juan@kpg.com", ["Empleado"], "Juan Pérez"));

        token.Claims.Should().NotContain(c => c.Type == JwtTokenService.ClaimPuesto);
    }

    private static JwtTokenService CreateService() =>
        new(Options.Create(new JwtSettings
        {
            Key = "KPG-Timesheet-Test-Secret-Key-MinLength32Chars!!",
            Issuer = "KPG.Timesheet.Api",
            Audience = "KPG.Timesheet.WebUI",
            ExpirationMinutes = 60
        }), TimeProvider.System);

    private static JwtSecurityToken Leer(string token) =>
        new JwtSecurityTokenHandler().ReadJwtToken(token);

    private static string? Claim(JwtSecurityToken token, string tipo) =>
        token.Claims.FirstOrDefault(c => c.Type == tipo)?.Value;
}
