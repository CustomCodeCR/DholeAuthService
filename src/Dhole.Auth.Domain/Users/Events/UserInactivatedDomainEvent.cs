using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Auth.Domain.Users.Events;

public sealed record UserInactivatedDomainEvent(
    Guid id,
    string userName,
    string email,
    Guid? inactivatedBy
) : DomainEvent;
