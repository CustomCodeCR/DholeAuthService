using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Contracts.Users;
using Dhole.Auth.Domain.Users.Entities;

namespace Dhole.Auth.Application.Abstractions.Repositories;

public interface IUserRepository : IRepository<User, Guid>
{
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<User?> GetWithRolesAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<User?> GetWithScopesAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<User?> GetWithRolesAndScopesAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByUserNameAsync(
        string userName,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<PagedResult<UserDto>> GetPagedAsync(
        PageRequest page,
        string? search = null,
        bool? isActive = null,
        bool? isLocked = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<UserRoleDto>> GetUserRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<UserScopeDto>> GetUserScopesAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    Task<UserPermissionsDto> GetUserPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
}
