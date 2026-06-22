namespace Dhole.Auth.Contracts.Authentication;

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    Guid SessionId,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt
);
