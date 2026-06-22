using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Auth.Domain.Sessions.Events;

public sealed record SessionLoggedOutDomainEvent(Guid SessionId, Guid UserId, Guid? LoggedOutBy)
    : DomainEvent;
