using CustomCodeFramework.Core.Domain.Entities;
using Dhole.Auth.Domain.Roles.Events;

namespace Dhole.Auth.Domain.Roles.Entities;

public sealed class Role : SoftDeletableAggregateRoot<Guid>
{
    private readonly List<RoleScope> _scopes = [];

    private Role() { }

    private Role(Guid id, string name, string? description, bool isSystemRole, Guid? createdBy)
        : base(id)
    {
        Name = name;
        Description = description;
        IsSystemRole = isSystemRole;
        IsActive = true;

        MarkAsCreated(DateTime.UtcNow, createdBy?.ToString());
    }

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public bool IsSystemRole { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<RoleScope> Scopes => _scopes;

    public static Role Create(
        string name,
        string? description,
        bool isSystemRole = false,
        Guid? createdBy = null
    )
    {
        var role = new Role(
            Guid.NewGuid(),
            name.Trim(),
            description?.Trim(),
            isSystemRole,
            createdBy
        );

        role.AddDomainEvent(new RoleCreatedDomainEvent(role.Id, role.Name, createdBy));

        return role;
    }

    public void Update(string name, string? description, Guid? updatedBy = null)
    {
        Name = name.Trim();
        Description = description?.Trim();

        MarkAsUpdated(DateTime.UtcNow, updatedBy?.ToString());

        AddDomainEvent(new RoleUpdatedDomainEvent(Id, Name, updatedBy));
    }

    public void Delete(Guid? deletedBy = null)
    {
        MarkAsDeleted(DateTime.UtcNow, deletedBy?.ToString());

        AddDomainEvent(new RoleDeletedDomainEvent(Id, Name, deletedBy));
    }

    public void AssignScope(Guid scopeId, Guid? assignedBy = null)
    {
        if (_scopes.Any(x => x.ScopeId == scopeId))
            return;

        _scopes.Add(RoleScope.Create(Id, scopeId, assignedBy));

        MarkAsUpdated(DateTime.UtcNow, assignedBy?.ToString());

        AddDomainEvent(new ScopeAssignedToRoleDomainEvent(Id, scopeId, assignedBy));
    }

    public void RevokeScope(Guid scopeId, Guid? revokedBy = null)
    {
        var roleScope = _scopes.FirstOrDefault(x => x.ScopeId == scopeId);

        if (roleScope is null)
            return;

        _scopes.Remove(roleScope);

        MarkAsUpdated(DateTime.UtcNow, revokedBy?.ToString());

        AddDomainEvent(new ScopeRevokedFromRoleDomainEvent(Id, scopeId, revokedBy));
    }

    public void SetActive(bool isActive, Guid? updatedBy = null)
    {
        if (IsActive == isActive)
            return;

        IsActive = isActive;

        MarkAsUpdated(DateTime.UtcNow, updatedBy?.ToString());

        if (IsActive)
        {
            AddDomainEvent(new RoleActivatedDomainEvent(Id, Name, updatedBy));
            return;
        }

        AddDomainEvent(new RoleInactivatedDomainEvent(Id, Name, updatedBy));
    }
}
