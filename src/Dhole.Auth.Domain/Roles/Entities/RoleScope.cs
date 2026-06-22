using CustomCodeFramework.Core.Domain.Entities;

namespace Dhole.Auth.Domain.Roles.Entities;

public sealed class RoleScope : Entity<Guid>
{
    private RoleScope() { }

    private RoleScope(Guid id, Guid roleId, Guid scopeId, Guid? assignedBy)
        : base(id)
    {
        RoleId = roleId;
        ScopeId = scopeId;
        AssignedAt = DateTime.UtcNow;
        AssignedBy = assignedBy;
    }

    public Guid RoleId { get; private set; }
    public Guid ScopeId { get; private set; }
    public DateTime AssignedAt { get; private set; }
    public Guid? AssignedBy { get; private set; }

    public static RoleScope Create(Guid roleId, Guid scopeId, Guid? assignedBy = null)
    {
        return new RoleScope(Guid.NewGuid(), roleId, scopeId, assignedBy);
    }
}
