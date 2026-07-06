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
        string displayName,
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
                ExtraClaims = BuildDisplayNameClaims(displayName),
                Roles = roles,
                Scopes = scopes,
                TokenVersion = tokenVersion,
            }
        );
    }

    private static Dictionary<string, string> BuildDisplayNameClaims(string displayName)
    {
        var normalizedDisplayName = string.IsNullOrWhiteSpace(displayName)
            ? string.Empty
            : displayName.Trim();

        return new Dictionary<string, string>
        {
            ["displayName"] = normalizedDisplayName,
            ["display_name"] = normalizedDisplayName,
            ["fullName"] = normalizedDisplayName,
            ["full_name"] = normalizedDisplayName,
        };
    }
}
