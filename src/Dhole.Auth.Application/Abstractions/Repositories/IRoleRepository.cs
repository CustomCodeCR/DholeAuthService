using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Contracts.Roles;
using Dhole.Auth.Domain.Roles.Entities;

namespace Dhole.Auth.Application.Abstractions.Repositories;

public interface IRoleRepository : IRepository<Role, Guid>
{
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<Role?> GetWithScopesAsync(Guid roleId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<PagedResult<RoleDto>> GetPagedAsync(
        PageRequest page,
        string? search = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<RoleSelectDto>> GetForSelectAsync(
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<RoleScopeDto>> GetRoleScopesAsync(
        Guid roleId,
        CancellationToken cancellationToken = default
    );
}
