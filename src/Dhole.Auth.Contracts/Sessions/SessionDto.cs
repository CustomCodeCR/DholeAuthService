namespace Dhole.Auth.Contracts.Sessions;

public sealed record SessionDto(
    Guid Id,
    Guid UserId,
    string? IpAddress,
    string? UserAgent,
    DateTime CreatedAt,
    DateTime LastUsedAt,
    DateTime ExpiresAt,
    bool IsRevoked,
    DateTime? RevokedAt,
    string? RevocationReason
);
