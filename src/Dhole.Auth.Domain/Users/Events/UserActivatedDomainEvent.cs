using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Auth.Domain.Users.Events;

public sealed record UserActivatedDomainEvent(
    Guid id,
    string userName,
    string email,
    Guid? activatedBy
) : DomainEvent;
