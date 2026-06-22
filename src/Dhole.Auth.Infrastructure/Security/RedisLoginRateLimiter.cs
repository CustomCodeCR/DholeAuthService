using CustomCodeFramework.Redis.Abstractions;
using CustomCodeFramework.Redis.Caching;
using Dhole.Auth.Application.Abstractions.Security;
using Microsoft.Extensions.Options;

namespace Dhole.Auth.Infrastructure.Security;

public sealed class RedisLoginRateLimiter(
    ICacheService cache,
    IOptions<LoginRateLimitOptions> options
) : ILoginRateLimiter
{
    private readonly LoginRateLimitOptions _options = options.Value;

    public Task<bool> IsBlockedAsync(
        string email,
        string? ipAddress,
        CancellationToken cancellationToken = default
    )
    {
        return cache.ExistsAsync(BuildBlockedKey(email, ipAddress), cancellationToken);
    }

    public async Task RegisterFailedAttemptAsync(
        string email,
        string? ipAddress,
        CancellationToken cancellationToken = default
    )
    {
        var attemptsKey = BuildAttemptsKey(email, ipAddress);

        var attempts = await cache.GetAsync<int>(attemptsKey, cancellationToken);

        attempts++;

        await cache.SetAsync(
            attemptsKey,
            attempts,
            CacheEntryOptions.Default(TimeSpan.FromMinutes(_options.WindowMinutes)),
            cancellationToken
        );

        if (attempts < _options.MaxFailedAttempts)
        {
            return;
        }

        await cache.SetAsync(
            BuildBlockedKey(email, ipAddress),
            true,
            CacheEntryOptions.Default(TimeSpan.FromMinutes(_options.BlockMinutes)),
            cancellationToken
        );
    }

    public async Task ResetAsync(
        string email,
        string? ipAddress,
        CancellationToken cancellationToken = default
    )
    {
        await cache.RemoveAsync(BuildAttemptsKey(email, ipAddress), cancellationToken);

        await cache.RemoveAsync(BuildBlockedKey(email, ipAddress), cancellationToken);
    }

    private static string BuildAttemptsKey(string email, string? ipAddress)
    {
        return $"auth:login:attempts:{Normalize(email)}:{Normalize(ipAddress)}";
    }

    private static string BuildBlockedKey(string email, string? ipAddress)
    {
        return $"auth:login:blocked:{Normalize(email)}:{Normalize(ipAddress)}";
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "unknown" : value.Trim().ToLowerInvariant();
    }
}
