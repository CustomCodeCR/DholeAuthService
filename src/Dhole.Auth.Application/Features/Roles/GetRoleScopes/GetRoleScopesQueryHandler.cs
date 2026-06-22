using CustomCodeFramework.Cqrs.Queries;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Contracts.Roles;

namespace Dhole.Auth.Application.Roles.GetRoleScopes;

public sealed class GetRoleScopesQueryHandler(IRoleRepository roles)
    : IQueryHandler<GetRoleScopesQuery, IReadOnlyCollection<RoleScopeDto>>
{
    public Task<IReadOnlyCollection<RoleScopeDto>> HandleAsync(
        GetRoleScopesQuery query,
        CancellationToken cancellationToken = default
    )
    {
        return roles.GetRoleScopesAsync(query.RoleId, cancellationToken);
    }
}
