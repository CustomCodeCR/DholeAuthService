using CustomCodeFramework.Core.Domain.Entities;
using Dhole.Auth.Domain.Sessions.Events;

namespace Dhole.Auth.Domain.Sessions.Entities;

public sealed class Session : AggregateRoot<Guid>
{
    private Session() { }

    private Session(
        Guid id,
        Guid userId,
        string refreshTokenHash,
        DateTime expiresAt,
        string? ipAddress,
        string? userAgent
    )
        : base(id)
    {
        UserId = userId;
        RefreshTokenHash = refreshTokenHash;
        ExpiresAt = expiresAt;
        IpAddress = ipAddress;
        UserAgent = userAgent;

        CreatedAt = DateTime.UtcNow;
        LastUsedAt = DateTime.UtcNow;

        IsRevoked = false;
    }

    public Guid UserId { get; private set; }
    public string RefreshTokenHash { get; private set; } = string.Empty;

    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime LastUsedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public Guid? RevokedBy { get; private set; }
    public string? RevocationReason { get; private set; }

    public static Session Create(
        Guid userId,
        string refreshTokenHash,
        DateTime expiresAt,
        string? ipAddress,
        string? userAgent
    )
    {
        var session = new Session(
            Guid.NewGuid(),
            userId,
            refreshTokenHash,
            expiresAt,
            ipAddress,
            userAgent
        );

        session.AddDomainEvent(
            new SessionCreatedDomainEvent(
                session.Id,
                session.UserId,
                session.IpAddress,
                session.UserAgent
            )
        );

        return session;
    }

    public void Refresh(
        string newRefreshTokenHash,
        DateTime newExpiresAt,
        string? ipAddress,
        string? userAgent
    )
    {
        if (IsRevoked)
        {
            return;
        }

        RefreshTokenHash = newRefreshTokenHash;
        ExpiresAt = newExpiresAt;
        LastUsedAt = DateTime.UtcNow;
        IpAddress = ipAddress;
        UserAgent = userAgent;

        AddDomainEvent(new SessionRefreshedDomainEvent(Id, UserId, IpAddress, UserAgent));
    }

    public void Revoke(Guid? revokedBy = null, string? reason = null)
    {
        if (IsRevoked)
        {
            return;
        }

        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        RevokedBy = revokedBy;
        RevocationReason = reason?.Trim();

        AddDomainEvent(new SessionRevokedDomainEvent(Id, UserId, RevokedBy, RevocationReason));
    }

    public void Logout(Guid? loggedOutBy = null)
    {
        if (IsRevoked)
        {
            return;
        }

        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        RevokedBy = loggedOutBy;
        RevocationReason = "Logout";

        AddDomainEvent(new SessionLoggedOutDomainEvent(Id, UserId, loggedOutBy));
    }

    public bool IsExpired(DateTime utcNow)
    {
        return ExpiresAt <= utcNow;
    }

    public bool CanBeUsed(DateTime utcNow)
    {
        return !IsRevoked && !IsExpired(utcNow);
    }
}
