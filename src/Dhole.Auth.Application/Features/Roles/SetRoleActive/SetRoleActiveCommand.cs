using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Auth.Application.Roles.SetRoleActive;

public sealed record SetRoleActiveCommand(Guid RoleId, bool IsActive, Guid? UpdatedBy)
    : ICommand<Result>;
