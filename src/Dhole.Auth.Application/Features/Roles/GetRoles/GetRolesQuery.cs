using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Auth.Contracts.Roles;

namespace Dhole.Auth.Application.Roles.GetRoles;

public sealed record GetRolesQuery(PageRequest Page, string? Search, bool? IsActive)
    : IQuery<PagedResult<RoleDto>>;
