using Dhole.Auth.Domain.Roles.Entities;
using Dhole.Auth.Domain.Scopes.Entities;
using Dhole.Auth.Domain.Sessions.Entities;
using Dhole.Auth.Domain.Users.Entities;

namespace Dhole.Auth.Application.Auditing;

public sealed record UserAuditSnapshot(
    Guid Id,
    string UserName,
    string Email,
    string DisplayName,
    string UserType,
    bool IsActive,
    bool IsLocked,
    bool IsDeleted,
    string? LockedReason,
    int FailedLoginAttempts,
    DateTime? LastLoginAt,
    DateTime? LastFailedLoginAt,
    int TokenVersion,
    IReadOnlyCollection<Guid> RoleIds,
    IReadOnlyCollection<Guid> DirectScopeIds
)
{
    public static UserAuditSnapshot From(User user)
    {
        return new UserAuditSnapshot(
            user.Id,
            user.UserName,
            user.Email,
            user.DisplayName,
            user.UserType.ToString(),
            user.IsActive,
            user.IsLocked,
            user.IsDeleted,
            user.LockedReason,
            user.FailedLoginAttempts,
            user.LastLoginAt,
            user.LastFailedLoginAt,
            user.TokenVersion,
            user.Roles.Select(x => x.RoleId).OrderBy(x => x).ToArray(),
            user.Scopes.Select(x => x.ScopeId).OrderBy(x => x).ToArray()
        );
    }
}

public sealed record RoleAuditSnapshot(
    Guid Id,
    string Name,
    string? Description,
    bool IsSystemRole,
    bool IsActive,
    bool IsDeleted,
    IReadOnlyCollection<Guid> ScopeIds
)
{
    public static RoleAuditSnapshot From(Role role)
    {
        return new RoleAuditSnapshot(
            role.Id,
            role.Name,
            role.Description,
            role.IsSystemRole,
            role.IsActive,
            role.IsDeleted,
            role.Scopes.Select(x => x.ScopeId).OrderBy(x => x).ToArray()
        );
    }
}

public sealed record ScopeAuditSnapshot(Guid Id, string Code, string Name, string? Description, bool IsActive)
{
    public static ScopeAuditSnapshot From(Scope scope)
    {
        return new ScopeAuditSnapshot(
            scope.Id,
            scope.Code,
            scope.Name,
            scope.Description,
            scope.IsActive
        );
    }
}

public sealed record SessionAuditSnapshot(
    Guid Id,
    Guid UserId,
    string? IpAddress,
    string? UserAgent,
    DateTime CreatedAt,
    DateTime LastUsedAt,
    DateTime ExpiresAt,
    bool IsRevoked,
    DateTime? RevokedAt,
    Guid? RevokedBy,
    string? RevocationReason
)
{
    public static SessionAuditSnapshot From(Session session)
    {
        return new SessionAuditSnapshot(
            session.Id,
            session.UserId,
            session.IpAddress,
            session.UserAgent,
            session.CreatedAt,
            session.LastUsedAt,
            session.ExpiresAt,
            session.IsRevoked,
            session.RevokedAt,
            session.RevokedBy,
            session.RevocationReason
        );
    }
}
