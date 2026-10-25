using CustomCodeFramework.Cqrs.Queries;
using Dhole.Auth.Contracts.Roles;

namespace Dhole.Auth.Application.Roles.GetRoleScopes;

public sealed record GetRoleScopesQuery(Guid RoleId) : IQuery<IReadOnlyCollection<RoleScopeDto>>;
