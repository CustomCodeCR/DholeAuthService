using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Auth.Domain.Roles.Events;

public sealed record RoleDeletedDomainEvent(Guid id, string name, Guid? deletedBy) : DomainEvent;
