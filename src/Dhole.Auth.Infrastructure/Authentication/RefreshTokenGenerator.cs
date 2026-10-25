using System.Security.Cryptography;
using System.Text;
using Dhole.Auth.Application.Abstractions.Authentication;

namespace Dhole.Auth.Infrastructure.Authentication;

public sealed class RefreshTokenGenerator : IRefreshTokenGenerator
{
    public string Generate()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public string Hash(string refreshToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));

        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
