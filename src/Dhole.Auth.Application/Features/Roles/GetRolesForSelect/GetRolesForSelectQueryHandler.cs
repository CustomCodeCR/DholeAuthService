using CustomCodeFramework.Cqrs.Queries;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Contracts.Roles;

namespace Dhole.Auth.Application.Roles.GetRolesForSelect;

public sealed class GetRolesForSelectQueryHandler(IRoleRepository roles)
    : IQueryHandler<GetRolesForSelectQuery, IReadOnlyCollection<RoleSelectDto>>
{
    public Task<IReadOnlyCollection<RoleSelectDto>> HandleAsync(
        GetRolesForSelectQuery query,
        CancellationToken cancellationToken = default
    )
    {
        return roles.GetForSelectAsync(cancellationToken);
    }
}
