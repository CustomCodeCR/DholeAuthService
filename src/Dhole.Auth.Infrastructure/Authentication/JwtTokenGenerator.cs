using CustomCodeFramework.Auth.Abstractions;
using Dhole.Auth.Application.Abstractions.Authentication;

namespace Dhole.Auth.Infrastructure.Authentication;

internal sealed class JwtTokenGenerator(ITokenService tokenService) : IJwtTokenGenerator
{
    public string Generate(
        Guid userId,
        Guid sessionId,
        string userType,
        string email,
        string userName,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> scopes,
        int tokenVersion,
        DateTime expiresAt
    )
    {
        return tokenService.CreateToken(
            new TokenRequest
            {
                UserId = userId.ToString(),
                SessionId = sessionId.ToString(),
                UserType = userType,
                Email = email,
                UserName = userName,
                Roles = roles,
                Scopes = scopes,
                TokenVersion = tokenVersion,
            }
        );
    }
}
