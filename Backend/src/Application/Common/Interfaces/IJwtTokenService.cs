namespace KPG.Timesheet.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(string userId, string email, IEnumerable<string> roles);
}
