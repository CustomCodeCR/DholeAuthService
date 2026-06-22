namespace Dhole.Auth.Api.Authorization;

internal static class AuthScopeNames
{
    public const string UsersCreate = "auth.users.create";
    public const string UsersView = "auth.users.view";
    public const string UsersUpdate = "auth.users.update";
    public const string UsersDelete = "auth.users.delete";
    public const string UsersSetActive = "auth.users.set-active";
    public const string UsersSetLocked = "auth.users.set-locked";
    public const string UsersChangePassword = "auth.users.change-password";
    public const string UsersRolesAssign = "auth.users.roles.assign";
    public const string UsersRolesRevoke = "auth.users.roles.revoke";
    public const string UsersScopesAssign = "auth.users.scopes.assign";
    public const string UsersScopesRevoke = "auth.users.scopes.revoke";

    public const string RolesCreate = "auth.roles.create";
    public const string RolesView = "auth.roles.view";
    public const string RolesUpdate = "auth.roles.update";
    public const string RolesDelete = "auth.roles.delete";
    public const string RolesSetActive = "auth.roles.set-active";
    public const string RolesScopesAssign = "auth.roles.scopes.assign";
    public const string RolesScopesRevoke = "auth.roles.scopes.revoke";

    public const string ScopesView = "auth.scopes.view";
    public const string ScopesSetActive = "auth.scopes.set-active";

    public const string SessionsView = "auth.sessions.view";
    public const string SessionsRevoke = "auth.sessions.revoke";
    public const string SessionsRevokeAll = "auth.sessions.revoke-all";
}
