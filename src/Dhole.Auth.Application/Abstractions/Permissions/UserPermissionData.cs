namespace Dhole.Auth.Application.Abstractions.Permissions;

public sealed record UserPermissionData(
    Guid UserId,
    bool IsActive,
    bool IsLocked,
    IReadOnlyCollection<string> ActiveRoles,
    IReadOnlyCollection<string> DirectScopes,
    IReadOnlyCollection<string> RoleScopes
);
