using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Auth.Application.Users.AssignScopesToUser;

public sealed record AssignScopesToUserCommand(
    Guid UserId,
    IReadOnlyCollection<Guid> ScopeIds,
    Guid? AssignedBy
) : ICommand<Result>;
