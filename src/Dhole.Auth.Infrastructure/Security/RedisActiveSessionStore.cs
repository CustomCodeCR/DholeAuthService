using CustomCodeFramework.Redis.Abstractions;
using CustomCodeFramework.Redis.Caching;
using Dhole.Auth.Application.Abstractions.Security;

namespace Dhole.Auth.Infrastructure.Security;

public sealed class RedisActiveSessionStore(ICacheService cache) : IActiveSessionStore
{
    public Task SetActiveAsync(
        Guid sessionId,
        Guid userId,
        TimeSpan expiration,
        CancellationToken cancellationToken = default
    )
    {
        return cache.SetAsync(
            BuildKey(sessionId),
            userId,
            CacheEntryOptions.Default(expiration),
            cancellationToken
        );
    }

    public Task RemoveAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return cache.RemoveAsync(BuildKey(sessionId), cancellationToken);
    }

    public Task<bool> IsActiveAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return cache.ExistsAsync(BuildKey(sessionId), cancellationToken);
    }

    private static string BuildKey(Guid sessionId)
    {
        return $"auth:sessions:active:{sessionId}";
    }
}
