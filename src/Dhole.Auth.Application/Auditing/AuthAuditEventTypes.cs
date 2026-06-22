namespace Dhole.Auth.Application.Auditing;

public static class AuthAuditEventTypes
{
    public const string UserCreated = "auth.user.created";
    public const string UserUpdated = "auth.user.updated";
    public const string UserDeleted = "auth.user.deleted";
    public const string UserActivated = "auth.user.activated";
    public const string UserInactivated = "auth.user.inactivated";
    public const string UserBlocked = "auth.user.blocked";
    public const string UserUnblocked = "auth.user.unblocked";
    public const string UserPasswordChanged = "auth.user.password_changed";
    public const string UserRoleAssigned = "auth.user.role.assigned";
    public const string UserRoleRevoked = "auth.user.role.revoked";
    public const string UserScopeAssigned = "auth.user.scope.assigned";
    public const string UserScopeRevoked = "auth.user.scope.revoked";

    public const string RoleCreated = "auth.role.created";
    public const string RoleUpdated = "auth.role.updated";
    public const string RoleDeleted = "auth.role.deleted";
    public const string RoleActivated = "auth.role.activated";
    public const string RoleInactivated = "auth.role.inactivated";
    public const string RoleScopeAssigned = "auth.role.scope.assigned";
    public const string RoleScopeRevoked = "auth.role.scope.revoked";

    public const string LoginSucceeded = "auth.login.succeeded";
    public const string LoginFailed = "auth.login.failed";
    public const string SessionCreated = "auth.session.created";
    public const string SessionRefreshed = "auth.session.refreshed";
    public const string SessionRevoked = "auth.session.revoked";
    public const string UserSessionsRevoked = "auth.user.sessions.revoked";
}
