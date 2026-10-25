namespace Dhole.Auth.Contracts.Users;

public sealed record IssuedUserCredentialsDto(
    Guid UserId,
    string UserName,
    string Email,
    string DisplayName,
    string TemporaryPassword
);
