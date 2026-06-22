using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Auth.Application.Roles.AssignScopesToRole;

public sealed record AssignScopesToRoleCommand(
    Guid RoleId,
    IReadOnlyCollection<Guid> ScopeIds,
    Guid? AssignedBy
) : ICommand<Result>;
