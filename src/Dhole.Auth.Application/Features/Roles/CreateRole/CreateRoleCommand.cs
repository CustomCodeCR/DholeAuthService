using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Auth.Application.Roles.CreateRole;

public sealed record CreateRoleCommand(
    string Name,
    string? Description,
    bool IsSystemRole,
    Guid? CreatedBy
) : ICommand<Result<Guid>>;
