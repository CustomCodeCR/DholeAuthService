using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Auth.Domain.Sessions.Events;

public sealed record SessionRevokedDomainEvent(
    Guid SessionId,
    Guid UserId,
    Guid? RevokedBy,
    string? Reason
) : DomainEvent;
