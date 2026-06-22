namespace Dhole.Auth.Contracts.Scopes;

public sealed record ScopeDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    bool IsActive
);
