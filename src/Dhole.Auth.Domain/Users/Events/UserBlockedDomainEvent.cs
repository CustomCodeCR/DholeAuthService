using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Auth.Domain.Users.Events;

public sealed record UserBlockedDomainEvent(
    Guid id,
    string userName,
    string email,
    string reason,
    Guid? blockedBy
) : DomainEvent;
