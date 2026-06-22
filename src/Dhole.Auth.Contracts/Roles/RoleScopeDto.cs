namespace Dhole.Auth.Contracts.Roles;

public sealed record RoleScopeDto(Guid RoleId, Guid ScopeId, string ScopeCode, string ScopeName);
