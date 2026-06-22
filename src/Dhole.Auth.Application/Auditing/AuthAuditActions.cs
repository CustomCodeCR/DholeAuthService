namespace Dhole.Auth.Application.Auditing;

public static class AuthAuditActions
{
    public const string Created = "created";
    public const string Updated = "updated";
    public const string Deleted = "deleted";
    public const string Activated = "activated";
    public const string Inactivated = "inactivated";
    public const string Blocked = "blocked";
    public const string Unblocked = "unblocked";
    public const string PasswordChanged = "password_changed";
    public const string RoleAssigned = "role_assigned";
    public const string RoleRevoked = "role_revoked";
    public const string ScopeAssigned = "scope_assigned";
    public const string ScopeRevoked = "scope_revoked";
    public const string LoginSucceeded = "login_succeeded";
    public const string LoginFailed = "login_failed";
    public const string Refreshed = "refreshed";
    public const string Revoked = "revoked";
    public const string RevokedAll = "revoked_all";
}
