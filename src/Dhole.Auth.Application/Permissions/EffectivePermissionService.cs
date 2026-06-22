using Dhole.Auth.Application.Abstractions.Permissions;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Permissions;

public sealed class EffectivePermissionService(
    IAuthPermissionReadRepository permissionReadRepository
) : IEffectivePermissionService
{
    public async Task<EffectivePermissions> GetAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var data = await permissionReadRepository.GetUserPermissionDataAsync(
            userId,
            cancellationToken
        );

        if (data is null || !data.IsActive || data.IsLocked)
        {
            return new EffectivePermissions(userId, [], [], [], []);
        }

        if (
            data.ActiveRoles.Contains(
                AuthConstants.SystemRoles.SuperUser,
                StringComparer.OrdinalIgnoreCase
            )
        )
        {
            var allScopes = await permissionReadRepository.GetAllActiveScopeCodesAsync(
                cancellationToken
            );

            return new EffectivePermissions(
                data.UserId,
                data.ActiveRoles,
                data.DirectScopes,
                data.RoleScopes,
                allScopes.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToArray()
            );
        }

        var effectiveScopes = data
            .DirectScopes.Concat(data.RoleScopes)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToArray();

        return new EffectivePermissions(
            data.UserId,
            data.ActiveRoles,
            data.DirectScopes,
            data.RoleScopes,
            effectiveScopes
        );
    }
}
