using CustomCodeFramework.Core.Domain.Events;
using Dhole.Auth.Domain.Roles.Events;
using Dhole.Auth.Domain.Sessions.Events;
using Dhole.Auth.Domain.Users.Events;

namespace Dhole.Auth.Persistence.Messaging;

internal static class DomainEventOutboxMapper
{
    public static string GetEventName(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            // Users
            UserCreatedDomainEvent => "auth.user.created",
            UserUpdatedDomainEvent => "auth.user.updated",
            UserPasswordChangedDomainEvent => "auth.user.password_changed",
            UserDeletedDomainEvent => "auth.user.deleted",
            UserActivatedDomainEvent => "auth.user.activated",
            UserInactivatedDomainEvent => "auth.user.inactivated",
            UserBlockedDomainEvent => "auth.user.blocked",
            UserUnblockedDomainEvent => "auth.user.unblocked",

            RoleAssignedToUserDomainEvent => "auth.user.role_assigned",
            RoleRevokedFromUserDomainEvent => "auth.user.role_revoked",
            ScopeAssignedToUserDomainEvent => "auth.user.scope_assigned",
            ScopeRevokedFromUserDomainEvent => "auth.user.scope_revoked",

            // Roles
            RoleCreatedDomainEvent => "auth.role.created",
            RoleUpdatedDomainEvent => "auth.role.updated",
            RoleDeletedDomainEvent => "auth.role.deleted",
            RoleActivatedDomainEvent => "auth.role.activated",
            RoleInactivatedDomainEvent => "auth.role.inactivated",
            ScopeAssignedToRoleDomainEvent => "auth.role.scope_assigned",
            ScopeRevokedFromRoleDomainEvent => "auth.role.scope_revoked",

            // Sessions
            SessionCreatedDomainEvent => "auth.session.created",
            SessionRefreshedDomainEvent => "auth.session.refreshed",
            SessionRevokedDomainEvent => "auth.session.revoked",
            SessionLoggedOutDomainEvent => "auth.session.logged_out",

            _ => $"auth.{domainEvent.GetType().Name}",
        };
    }

    public static string GetEventType(IDomainEvent domainEvent)
    {
        return domainEvent.GetType().FullName ?? domainEvent.GetType().Name;
    }
}
