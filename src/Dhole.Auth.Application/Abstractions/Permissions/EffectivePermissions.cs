namespace Dhole.Auth.Application.Abstractions.Permissions;

public sealed record EffectivePermissions(
    Guid UserId,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> DirectScopes,
    IReadOnlyCollection<string> RoleScopes,
    IReadOnlyCollection<string> EffectiveScopes
);
