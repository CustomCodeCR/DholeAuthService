namespace Dhole.Auth.Application.Abstractions.Permissions;

public interface IAuthPermissionReadRepository
{
    Task<UserPermissionData?> GetUserPermissionDataAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<string>> GetAllActiveScopeCodesAsync(
        CancellationToken cancellationToken = default
    );
}
