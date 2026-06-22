using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Auth.Domain.Users.Events;

public sealed record UserPasswordChangedDomainEvent(Guid id, string userName, Guid? updatedBy)
    : DomainEvent;
