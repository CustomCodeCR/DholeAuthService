using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Auth.Domain.Users.Events;

public sealed record ScopeAssignedToUserDomainEvent(Guid userId, Guid scopeId, Guid? assignedBy)
    : DomainEvent;
