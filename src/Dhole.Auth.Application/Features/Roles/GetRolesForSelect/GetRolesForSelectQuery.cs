using CustomCodeFramework.Cqrs.Queries;
using Dhole.Auth.Contracts.Roles;

namespace Dhole.Auth.Application.Roles.GetRolesForSelect;

public sealed record GetRolesForSelectQuery : IQuery<IReadOnlyCollection<RoleSelectDto>>;
