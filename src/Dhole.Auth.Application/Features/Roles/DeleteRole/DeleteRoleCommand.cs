using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Auth.Application.Roles.DeleteRole;

public sealed record DeleteRoleCommand(Guid RoleId, Guid? DeletedBy) : ICommand<Result>;
