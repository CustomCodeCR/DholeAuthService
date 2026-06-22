using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Auth.Domain.Roles.Events;

public sealed record RoleUpdatedDomainEvent(Guid id, string name, Guid? updatedBy) : DomainEvent;
