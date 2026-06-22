using Dhole.Auth.Domain.Users.Enums;

namespace Dhole.Auth.Contracts.Users;

public sealed record UserDto(
    Guid Id,
    string UserName,
    string Email,
    string DisplayName,
    UserType UserType,
    bool IsActive,
    bool IsLocked,
    DateTime? LastLoginAt
);
