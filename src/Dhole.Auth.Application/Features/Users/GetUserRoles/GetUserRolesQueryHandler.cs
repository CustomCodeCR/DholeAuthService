using CustomCodeFramework.Cqrs.Queries;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Contracts.Users;

namespace Dhole.Auth.Application.Users.GetUserRoles;

public sealed class GetUserRolesQueryHandler(IUserRepository users)
    : IQueryHandler<GetUserRolesQuery, IReadOnlyCollection<UserRoleDto>>
{
    public Task<IReadOnlyCollection<UserRoleDto>> HandleAsync(
        GetUserRolesQuery query,
        CancellationToken cancellationToken = default
    )
    {
        return users.GetUserRolesAsync(query.UserId, cancellationToken);
    }
}
