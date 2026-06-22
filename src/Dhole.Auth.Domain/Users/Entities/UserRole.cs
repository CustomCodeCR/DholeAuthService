using CustomCodeFramework.Core.Domain.Entities;

namespace Dhole.Auth.Domain.Users.Entities;

public sealed class UserRole : Entity<Guid>
{
    private UserRole() { }

    private UserRole(Guid id, Guid userId, Guid roleId, Guid? assignedBy)
        : base(id)
    {
        UserId = userId;
        RoleId = roleId;
        AssignedAt = DateTime.UtcNow;
        AssignedBy = assignedBy;
    }

    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public DateTime AssignedAt { get; private set; }
    public Guid? AssignedBy { get; private set; }

    public static UserRole Create(Guid userId, Guid roleId, Guid? assignedBy = null)
    {
        return new UserRole(Guid.NewGuid(), userId, roleId, assignedBy);
    }
}
