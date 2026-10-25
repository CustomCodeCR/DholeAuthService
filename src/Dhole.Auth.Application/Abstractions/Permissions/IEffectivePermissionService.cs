namespace Dhole.Auth.Application.Abstractions.Permissions;

public interface IEffectivePermissionService
{
    Task<EffectivePermissions> GetAsync(Guid userId, CancellationToken cancellationToken = default);
}
