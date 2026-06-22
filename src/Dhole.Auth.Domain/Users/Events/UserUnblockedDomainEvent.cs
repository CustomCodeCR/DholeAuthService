using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Auth.Domain.Users.Events;

public sealed record UserUnblockedDomainEvent(
    Guid id,
    string userName,
    string email,
    Guid? UnblockedBy
) : DomainEvent;
