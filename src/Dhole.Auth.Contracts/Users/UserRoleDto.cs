namespace Dhole.Auth.Contracts.Users;

public sealed record UserRoleDto(Guid UserId, Guid RoleId, string RoleName);
