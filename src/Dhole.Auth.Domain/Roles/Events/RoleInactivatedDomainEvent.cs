using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Auth.Domain.Roles.Events;

public sealed record RoleInactivatedDomainEvent(Guid id, string name, Guid? InactivatedBy)
    : DomainEvent;
