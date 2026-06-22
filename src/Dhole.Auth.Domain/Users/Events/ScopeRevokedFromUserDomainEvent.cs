using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Auth.Domain.Users.Events;

public sealed record ScopeRevokedFromUserDomainEvent(Guid userId, Guid scopeId, Guid? revokedBy)
    : DomainEvent;
