using CustomCodeFramework.Core.Domain.Entities;
using Dhole.Auth.Domain.Users.Enums;
using Dhole.Auth.Domain.Users.Events;

namespace Dhole.Auth.Domain.Users.Entities;

public sealed class User : SoftDeletableAggregateRoot<Guid>
{
    private readonly List<UserRole> _roles = [];
    private readonly List<UserScope> _scopes = [];

    private User() { }

    private User(
        Guid id,
        string userName,
        string email,
        string displayName,
        UserType userType,
        string passwordHash,
        Guid? createdBy
    )
        : base(id)
    {
        UserName = userName;
        Email = email;
        DisplayName = displayName;
        UserType = userType;
        PasswordHash = passwordHash;

        IsActive = true;
        IsLocked = false;
        FailedLoginAttempts = 0;
        TokenVersion = 0;

        MarkAsCreated(DateTime.UtcNow, createdBy?.ToString());
    }

    public string UserName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public UserType UserType { get; private set; }
    public string PasswordHash { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }
    public bool IsLocked { get; private set; }

    public DateTime? LockedAt { get; private set; }
    public string? LockedReason { get; private set; }

    public int FailedLoginAttempts { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public DateTime? LastFailedLoginAt { get; private set; }

    public int TokenVersion { get; private set; }

    public IReadOnlyCollection<UserRole> Roles => _roles;
    public IReadOnlyCollection<UserScope> Scopes => _scopes;

    public static User Create(
        string userName,
        string email,
        string displayName,
        UserType userType,
        string passwordHash,
        Guid? createdBy
    )
    {
        var user = new User(
            Guid.NewGuid(),
            userName.Trim(),
            email.Trim().ToLowerInvariant(),
            displayName.Trim(),
            userType,
            passwordHash,
            createdBy
        );

        user.AddDomainEvent(
            new UserCreatedDomainEvent(
                user.Id,
                user.UserName,
                user.Email,
                user.DisplayName,
                createdBy
            )
        );

        return user;
    }

    public void UpdateProfile(
        string userName,
        string email,
        string displayName,
        Guid? updatedBy = null
    )
    {
        UserName = userName.Trim();
        Email = email.Trim().ToLowerInvariant();
        DisplayName = displayName.Trim();

        IncreaseTokenVersion();

        MarkAsUpdated(DateTime.UtcNow, updatedBy?.ToString());

        AddDomainEvent(new UserUpdatedDomainEvent(Id, UserName, Email, DisplayName, updatedBy));
    }

    public void ChangePassword(string passwordHash, Guid? updatedBy = null)
    {
        PasswordHash = passwordHash;

        IncreaseTokenVersion();

        MarkAsUpdated(DateTime.UtcNow, updatedBy?.ToString());

        AddDomainEvent(new UserPasswordChangedDomainEvent(Id, UserName, updatedBy));
    }

    public void Delete(Guid? deletedBy = null)
    {
        IncreaseTokenVersion();

        MarkAsDeleted(DateTime.UtcNow, deletedBy?.ToString());

        AddDomainEvent(new UserDeletedDomainEvent(Id, UserName, Email, DisplayName, deletedBy));
    }

    public void SetActive(bool isActive, Guid? updatedBy = null)
    {
        if (IsActive == isActive)
        {
            return;
        }

        IsActive = isActive;

        IncreaseTokenVersion();

        MarkAsUpdated(DateTime.UtcNow, updatedBy?.ToString());

        if (IsActive)
        {
            AddDomainEvent(new UserActivatedDomainEvent(Id, UserName, Email, updatedBy));
            return;
        }

        AddDomainEvent(new UserInactivatedDomainEvent(Id, UserName, Email, updatedBy));
    }

    public void AssignScope(Guid scopeId, Guid? assignedBy = null)
    {
        if (_scopes.Any(x => x.ScopeId == scopeId))
        {
            return;
        }

        _scopes.Add(UserScope.Create(Id, scopeId, assignedBy));

        IncreaseTokenVersion();

        MarkAsUpdated(DateTime.UtcNow, assignedBy?.ToString());

        AddDomainEvent(new ScopeAssignedToUserDomainEvent(Id, scopeId, assignedBy));
    }

    public void RevokeScope(Guid scopeId, Guid? revokedBy = null)
    {
        var userScope = _scopes.FirstOrDefault(x => x.ScopeId == scopeId);

        if (userScope is null)
        {
            return;
        }

        _scopes.Remove(userScope);

        IncreaseTokenVersion();

        MarkAsUpdated(DateTime.UtcNow, revokedBy?.ToString());

        AddDomainEvent(new ScopeRevokedFromUserDomainEvent(Id, scopeId, revokedBy));
    }

    public void AssignRole(Guid roleId, Guid? assignedBy = null)
    {
        if (_roles.Any(x => x.RoleId == roleId))
        {
            return;
        }

        _roles.Add(UserRole.Create(Id, roleId, assignedBy));

        IncreaseTokenVersion();

        MarkAsUpdated(DateTime.UtcNow, assignedBy?.ToString());

        AddDomainEvent(new RoleAssignedToUserDomainEvent(Id, roleId, assignedBy));
    }

    public void RevokeRole(Guid roleId, Guid? revokedBy = null)
    {
        var userRole = _roles.FirstOrDefault(x => x.RoleId == roleId);

        if (userRole is null)
        {
            return;
        }

        _roles.Remove(userRole);

        IncreaseTokenVersion();

        MarkAsUpdated(DateTime.UtcNow, revokedBy?.ToString());

        AddDomainEvent(new RoleRevokedFromUserDomainEvent(Id, roleId, revokedBy));
    }

    public void SetLocked(bool isLocked, string? reason = null, Guid? updatedBy = null)
    {
        if (IsLocked == isLocked)
        {
            return;
        }

        IsLocked = isLocked;

        IncreaseTokenVersion();

        if (IsLocked)
        {
            LockedAt = DateTime.UtcNow;
            LockedReason = reason?.Trim();

            MarkAsUpdated(DateTime.UtcNow, updatedBy?.ToString());

            AddDomainEvent(
                new UserBlockedDomainEvent(Id, UserName, Email, LockedReason!, updatedBy)
            );

            return;
        }

        LockedAt = null;
        LockedReason = null;

        MarkAsUpdated(DateTime.UtcNow, updatedBy?.ToString());

        AddDomainEvent(new UserUnblockedDomainEvent(Id, UserName, Email, updatedBy));
    }

    private void IncreaseTokenVersion()
    {
        TokenVersion++;
    }
}
