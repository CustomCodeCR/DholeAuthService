using Dhole.Auth.Application.Abstractions.Permissions;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Permissions.GetUserEffectivePermissions;

public sealed class EffectivePermissionService(
    IAuthPermissionReadRepository permissionReadRepository,
    IEffectivePermissionCache permissionCache
) : IEffectivePermissionService
{
    public async Task<EffectivePermissions> GetAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var cached = await permissionCache.GetAsync(userId, cancellationToken);

        if (cached is not null)
        {
            return cached;
        }

        var data = await permissionReadRepository.GetUserPermissionDataAsync(
            userId,
            cancellationToken
        );

        if (data is null || !data.IsActive || data.IsLocked)
        {
            return new EffectivePermissions(userId, [], [], [], []);
        }

        IReadOnlyCollection<string> effectiveScopes;

        if (
            data.ActiveRoles.Contains(
                AuthConstants.SystemRoles.SuperUser,
                StringComparer.OrdinalIgnoreCase
            )
        )
        {
            effectiveScopes = await permissionReadRepository.GetAllActiveScopeCodesAsync(
                cancellationToken
            );
        }
        else
        {
            effectiveScopes = data
                .DirectScopes.Concat(data.RoleScopes)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToArray();
        }

        var permissions = new EffectivePermissions(
            data.UserId,
            data.ActiveRoles,
            data.DirectScopes,
            data.RoleScopes,
            effectiveScopes
        );

        await permissionCache.SetAsync(permissions, TimeSpan.FromMinutes(15), cancellationToken);

        return permissions;
    }
}
