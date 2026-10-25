using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Auth.Domain.Users.Events;

public sealed record RoleRevokedFromUserDomainEvent(Guid userId, Guid roleId, Guid? revokedBy)
    : DomainEvent;
