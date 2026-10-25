using CustomCodeFramework.Cqrs.Queries;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Contracts.Users;

namespace Dhole.Auth.Application.Users.GetUserPermissions;

public sealed class GetUserPermissionsQueryHandler(IUserRepository users)
    : IQueryHandler<GetUserPermissionsQuery, UserPermissionsDto>
{
    public Task<UserPermissionsDto> HandleAsync(
        GetUserPermissionsQuery query,
        CancellationToken cancellationToken = default
    )
    {
        return users.GetUserPermissionsAsync(query.UserId, cancellationToken);
    }
}
