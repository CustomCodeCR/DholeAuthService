using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Contracts.Roles;

namespace Dhole.Auth.Application.Roles.GetRoles;

public sealed class GetRolesQueryHandler(IRoleRepository roles)
    : IQueryHandler<GetRolesQuery, PagedResult<RoleDto>>
{
    public Task<PagedResult<RoleDto>> HandleAsync(
        GetRolesQuery query,
        CancellationToken cancellationToken = default
    )
    {
        return roles.GetPagedAsync(query.Page, query.Search, query.IsActive, cancellationToken);
    }
}
