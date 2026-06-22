using CustomCodeFramework.Redis.Abstractions;
using CustomCodeFramework.Redis.Caching;
using Dhole.Auth.Application.Abstractions.Permissions;

namespace Dhole.Auth.Infrastructure.Permissions;

public sealed class RedisEffectivePermissionCache(ICacheService cache) : IEffectivePermissionCache
{
    public Task<EffectivePermissions?> GetAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return cache.GetAsync<EffectivePermissions?>(BuildKey(userId), cancellationToken);
    }

    public Task SetAsync(
        EffectivePermissions permissions,
        TimeSpan expiration,
        CancellationToken cancellationToken = default
    )
    {
        return cache.SetAsync(
            BuildKey(permissions.UserId),
            permissions,
            CacheEntryOptions.Default(expiration),
            cancellationToken
        );
    }

    public Task RemoveAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return cache.RemoveAsync(BuildKey(userId), cancellationToken);
    }

    private static string BuildKey(Guid userId)
    {
        return $"auth:permissions:effective:{userId}";
    }
}
