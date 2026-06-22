using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Contracts.Users;

namespace Dhole.Auth.Application.Users.GetUsers;

public sealed class GetUsersQueryHandler(IUserRepository users)
    : IQueryHandler<GetUsersQuery, PagedResult<UserDto>>
{
    public Task<PagedResult<UserDto>> HandleAsync(
        GetUsersQuery query,
        CancellationToken cancellationToken = default
    )
    {
        return users.GetPagedAsync(
            query.Page,
            query.Search,
            query.IsActive,
            query.IsLocked,
            cancellationToken
        );
    }
}
