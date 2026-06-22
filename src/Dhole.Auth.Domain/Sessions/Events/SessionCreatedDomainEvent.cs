using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Auth.Domain.Sessions.Events;

public sealed record SessionCreatedDomainEvent(
    Guid SessionId,
    Guid UserId,
    string? IpAddress,
    string? UserAgent
) : DomainEvent;
