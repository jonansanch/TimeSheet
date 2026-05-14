using KPG.Timesheet.WebUI.Infrastructure.Repositories.Models;

namespace KPG.Timesheet.WebUI.Infrastructure.Repositories;

public interface IAuthRepository
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<RefreshResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task LogoutAsync(string refreshToken, string accessToken, CancellationToken cancellationToken = default);
}
