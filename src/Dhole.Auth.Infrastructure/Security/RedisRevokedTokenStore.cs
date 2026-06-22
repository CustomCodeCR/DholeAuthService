using CustomCodeFramework.Redis.Abstractions;
using CustomCodeFramework.Redis.Caching;
using Dhole.Auth.Application.Abstractions.Security;

namespace Dhole.Auth.Infrastructure.Security;

public sealed class RedisRevokedTokenStore(ICacheService cache) : IRevokedTokenStore
{
    public Task RevokeSessionAsync(
        Guid sessionId,
        TimeSpan expiration,
        CancellationToken cancellationToken = default
    )
    {
        return cache.SetAsync(
            BuildKey(sessionId),
            true,
            CacheEntryOptions.Default(expiration),
            cancellationToken
        );
    }

    public Task<bool> IsSessionRevokedAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default
    )
    {
        return cache.ExistsAsync(BuildKey(sessionId), cancellationToken);
    }

    private static string BuildKey(Guid sessionId)
    {
        return $"auth:revoked:sessions:{sessionId}";
    }
}
