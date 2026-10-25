using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Auth.Application.Roles.RevokeScopesFromRole;

public sealed record RevokeScopesFromRoleCommand(
    Guid RoleId,
    IReadOnlyCollection<Guid> ScopeIds,
    Guid? RevokedBy
) : ICommand<Result>;
