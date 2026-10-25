using CustomCodeFramework.Cqrs.Queries;
using Dhole.Auth.Contracts.Users;

namespace Dhole.Auth.Application.Users.GetUserRoles;

public sealed record GetUserRolesQuery(Guid UserId) : IQuery<IReadOnlyCollection<UserRoleDto>>;
