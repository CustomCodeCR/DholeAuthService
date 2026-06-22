using CustomCodeFramework.Cqrs.Queries;
using Dhole.Auth.Contracts.Users;

namespace Dhole.Auth.Application.Users.GetUserPermissions;

public sealed record GetUserPermissionsQuery(Guid UserId) : IQuery<UserPermissionsDto>;
