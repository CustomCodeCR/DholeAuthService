namespace Dhole.Auth.Contracts.Authentication;

public sealed record ImpersonationResponse(
    string AccessToken,
    Guid SessionId,
    DateTime AccessTokenExpiresAt,
    string DisplayName,
    string UserName,
    string Email,
    Guid ImpersonatorUserId,
    string ImpersonatorUserName
);
