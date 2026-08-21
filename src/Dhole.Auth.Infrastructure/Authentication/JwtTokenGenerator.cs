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
                Scopes = ExpandCompatibleScopes(scopes),
                TokenVersion = tokenVersion,
            }
        );
    }

    private static IReadOnlyCollection<string> ExpandCompatibleScopes(IReadOnlyCollection<string> scopes)
    {
        var expanded = new HashSet<string>(scopes, StringComparer.OrdinalIgnoreCase);

        // Config historically exposed config.catalog-selects.view while some clients/roles
        // use the shorter config.select. Emit both so old and new services authorize the
        // same permission without forcing users to log in with two separate scopes.
        if (expanded.Contains("config.select"))
            expanded.Add("config.catalog-selects.view");
        if (expanded.Contains("config.catalog-selects.view"))
            expanded.Add("config.select");

        return expanded.ToArray();
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
