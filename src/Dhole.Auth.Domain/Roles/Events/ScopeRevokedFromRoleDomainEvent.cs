using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Auth.Domain.Roles.Events;

public sealed record ScopeRevokedFromRoleDomainEvent(Guid roleId, Guid scopeId, Guid? revokedBy)
    : DomainEvent;
