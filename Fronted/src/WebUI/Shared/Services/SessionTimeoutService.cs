namespace KPG.Timesheet.WebUI.Shared.Services;

public sealed class SessionTimeoutService : IDisposable
{
    public event Func<Task>? OnWarning;
    public event Func<Task>? OnExpired;

    public DateTime? ExpiresAt { get; private set; }
    public int WarningMinutes { get; private set; }

    private CancellationTokenSource? _cts;

    public void StartTracking(DateTime expiresAtUtc, int warningMinutes)
    {
        StopTracking();

        ExpiresAt = expiresAtUtc;
        WarningMinutes = warningMinutes;

        _cts = new CancellationTokenSource();
        _ = RunAsync(_cts.Token);
    }

    public void StopTracking()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        ExpiresAt = null;
    }

    private async Task RunAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var warningAt = ExpiresAt!.Value.AddMinutes(-WarningMinutes);
        var warningDelay = warningAt - now;
        var expireDelay = ExpiresAt.Value - now;

        try
        {
            if (warningDelay > TimeSpan.Zero)
                await Task.Delay(warningDelay, ct);

            if (!ct.IsCancellationRequested && OnWarning is not null)
                await OnWarning.Invoke();

            var remaining = ExpiresAt.Value - DateTime.UtcNow;
            if (remaining > TimeSpan.Zero)
                await Task.Delay(remaining, ct);

            if (!ct.IsCancellationRequested && OnExpired is not null)
                await OnExpired.Invoke();
        }
        catch (OperationCanceledException) { }
    }

    public void Dispose() => StopTracking();
}
