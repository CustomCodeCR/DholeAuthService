namespace Dhole.Auth.Contracts.Users;

public sealed record UserPermissionsDto(
    Guid UserId,
    IReadOnlyCollection<UserRoleDto> Roles,
    IReadOnlyCollection<UserScopeDto> DirectScopes,
    IReadOnlyCollection<UserScopeDto> EffectiveScopes
);
