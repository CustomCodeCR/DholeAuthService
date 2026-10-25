using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Auth.Domain.Users.Events;

public sealed record UserCreatedDomainEvent(
    Guid id,
    string userName,
    string email,
    string displayName,
    Guid? createdBy
) : DomainEvent;
