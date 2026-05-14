namespace KPG.Timesheet.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string UserId { get; init; } = string.Empty;

    // SHA-256 hash del token; el token raw se envía al cliente y nunca se persiste
    public string TokenHash { get; init; } = string.Empty;

    public DateTime ExpiresAt { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; private set; }

    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    public void Revoke()
    {
        RevokedAt = DateTime.UtcNow;
    }
}
