using CustomCodeFramework.Cqrs.Queries;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Contracts.Users;

namespace Dhole.Auth.Application.Auth.GetEffectivePermissions;

public sealed class GetEffectivePermissionsQueryHandler(IUserRepository users)
    : IQueryHandler<GetEffectivePermissionsQuery, UserPermissionsDto>
{
    public Task<UserPermissionsDto> HandleAsync(
        GetEffectivePermissionsQuery query,
        CancellationToken cancellationToken = default
    )
    {
        return users.GetUserPermissionsAsync(query.UserId, cancellationToken);
    }
}
