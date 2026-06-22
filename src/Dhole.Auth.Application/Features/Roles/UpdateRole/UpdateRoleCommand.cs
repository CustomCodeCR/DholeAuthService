using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Auth.Application.Roles.UpdateRole;

public sealed record UpdateRoleCommand(
    Guid RoleId,
    string Name,
    string? Description,
    Guid? UpdatedBy
) : ICommand<Result>;
