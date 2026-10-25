using CustomCodeFramework.Core.Domain.Entities;

namespace Dhole.Auth.Domain.Users.Entities;

public sealed class UserScope : Entity<Guid>
{
    private UserScope() { }

    private UserScope(Guid id, Guid userId, Guid scopeId, Guid? assignedBy)
        : base(id)
    {
        UserId = userId;
        ScopeId = scopeId;
        AssignedAt = DateTime.UtcNow;
        AssignedBy = assignedBy;
    }

    public Guid UserId { get; private set; }
    public Guid ScopeId { get; private set; }
    public DateTime AssignedAt { get; private set; }
    public Guid? AssignedBy { get; private set; }

    public static UserScope Create(Guid userId, Guid scopeId, Guid? assignedBy = null)
    {
        return new UserScope(Guid.NewGuid(), userId, scopeId, assignedBy);
    }
}
