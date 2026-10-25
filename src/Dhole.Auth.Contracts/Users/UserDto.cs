using Dhole.Auth.Domain.Users.Enums;

namespace Dhole.Auth.Contracts.Users;

public sealed record UserDto(
    Guid Id,
    string UserName,
    string Email,
    string DisplayName,
    UserType UserType,
    string UserTypeName,
    bool IsActive,
    bool IsLocked,
    DateTime? LastLoginAt,
    bool IsProtected
);
