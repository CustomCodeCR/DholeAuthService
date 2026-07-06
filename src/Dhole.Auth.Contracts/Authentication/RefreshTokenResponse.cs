namespace Dhole.Auth.Contracts.Authentication;

public sealed record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    Guid SessionId,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt,
    string DisplayName,
    string UserName,
    string Email
);
