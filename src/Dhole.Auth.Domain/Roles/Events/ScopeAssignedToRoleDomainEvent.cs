using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Auth.Domain.Roles.Events;

public sealed record ScopeAssignedToRoleDomainEvent(Guid roleId, Guid scopeId, Guid? assignedBy)
    : DomainEvent;
