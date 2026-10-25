using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Auth.Domain.Users.Events;

public sealed record RoleAssignedToUserDomainEvent(Guid userId, Guid roleId, Guid? assignedBy)
    : DomainEvent;
