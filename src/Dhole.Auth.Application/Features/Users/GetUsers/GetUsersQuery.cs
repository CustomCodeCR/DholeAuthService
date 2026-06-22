using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Auth.Contracts.Users;

namespace Dhole.Auth.Application.Users.GetUsers;

public sealed record GetUsersQuery(PageRequest Page, string? Search, bool? IsActive, bool? IsLocked)
    : IQuery<PagedResult<UserDto>>;
