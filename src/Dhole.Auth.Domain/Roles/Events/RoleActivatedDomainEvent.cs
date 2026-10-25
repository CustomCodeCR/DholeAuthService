using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Auth.Domain.Roles.Events;

public sealed record RoleActivatedDomainEvent(Guid id, string name, Guid? ActivatedBy)
    : DomainEvent;
