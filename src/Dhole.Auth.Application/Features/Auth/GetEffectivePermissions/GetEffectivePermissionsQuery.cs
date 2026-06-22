using CustomCodeFramework.Cqrs.Queries;
using Dhole.Auth.Contracts.Users;

namespace Dhole.Auth.Application.Auth.GetEffectivePermissions;

public sealed record GetEffectivePermissionsQuery(Guid UserId) : IQuery<UserPermissionsDto>;
