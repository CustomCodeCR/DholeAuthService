namespace Dhole.Auth.Domain.Shared;

public static class AuthConstants
{
    public const string ServiceName = "Auth";

    public static class SystemRoles
    {
        public const string Administrator = "Administrador";
        public const string SuperUser = "SuperUsuario";
    }

    public static class Scopes
    {
        // Users
        public const string UserCreate = "auth.users.create";
        public const string UserView = "auth.users.view";
        public const string UserUpdate = "auth.users.update";
        public const string UserDelete = "auth.users.delete";
        public const string UserSetActive = "auth.users.set-active";
        public const string UserSetLocked = "auth.users.set-locked";
        public const string UserChangePassword = "auth.users.change-password";

        // User roles
        public const string UserRoleAssign = "auth.users.roles.assign";
        public const string UserRoleRevoke = "auth.users.roles.revoke";

        // User scopes
        public const string UserScopeAssign = "auth.users.scopes.assign";
        public const string UserScopeRevoke = "auth.users.scopes.revoke";

        // Roles
        public const string RoleCreate = "auth.roles.create";
        public const string RoleView = "auth.roles.view";
        public const string RoleUpdate = "auth.roles.update";
        public const string RoleDelete = "auth.roles.delete";
        public const string RoleSetActive = "auth.roles.set-active";

        // Role scopes
        public const string RoleScopeAssign = "auth.roles.scopes.assign";
        public const string RoleScopeRevoke = "auth.roles.scopes.revoke";

        // Scopes
        public const string ScopeView = "auth.scopes.view";

        // Sessions
        public const string SessionView = "auth.sessions.view";
        public const string SessionRevoke = "auth.sessions.revoke";
        public const string SessionRevokeAll = "auth.sessions.revoke-all";

        // Auth actions
        public const string RefreshToken = "auth.refresh-token";
    }
}
