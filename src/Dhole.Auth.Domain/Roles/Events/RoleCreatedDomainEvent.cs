using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Auth.Domain.Roles.Events;

public sealed record RoleCreatedDomainEvent(Guid id, string name, Guid? createdBy) : DomainEvent;
