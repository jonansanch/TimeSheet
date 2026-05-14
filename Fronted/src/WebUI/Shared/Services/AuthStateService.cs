namespace KPG.Timesheet.WebUI.Shared.Services;

public class AuthStateService
{
    private string? _accessToken;

    public string? AccessToken => _accessToken;
    public bool IsAuthenticated => _accessToken is not null;

    public event Action? OnAuthStateChanged;

    public void SetToken(string accessToken)
    {
        _accessToken = accessToken;
        OnAuthStateChanged?.Invoke();
    }

    public void ClearToken()
    {
        _accessToken = null;
        OnAuthStateChanged?.Invoke();
    }
}
