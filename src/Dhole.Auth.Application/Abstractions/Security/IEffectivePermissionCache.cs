namespace Dhole.Auth.Application.Abstractions.Permissions;

public interface IEffectivePermissionCache
{
    Task<EffectivePermissions?> GetAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
    Task SetAsync(
        EffectivePermissions permissions,
        TimeSpan expiration,
        CancellationToken cancellationToken = default
    );
    Task RemoveAsync(Guid userId, CancellationToken cancellationToken = default);
}
