using CustomCodeFramework.Cqrs.Queries;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Contracts.Users;

namespace Dhole.Auth.Application.Users.GetUserScopes;

public sealed class GetUserScopesQueryHandler(IUserRepository users)
    : IQueryHandler<GetUserScopesQuery, IReadOnlyCollection<UserScopeDto>>
{
    public Task<IReadOnlyCollection<UserScopeDto>> HandleAsync(
        GetUserScopesQuery query,
        CancellationToken cancellationToken = default
    )
    {
        return users.GetUserScopesAsync(query.UserId, cancellationToken);
    }
}
