namespace Dhole.Auth.Contracts.Users;

public sealed record UserScopeDto(Guid UserId, Guid ScopeId, string ScopeCode, string ScopeName);
