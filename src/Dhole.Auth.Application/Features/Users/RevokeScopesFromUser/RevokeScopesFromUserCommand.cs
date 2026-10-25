using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Auth.Application.Users.RevokeScopesFromUser;

public sealed record RevokeScopesFromUserCommand(
    Guid UserId,
    IReadOnlyCollection<Guid> ScopeIds,
    Guid? RevokedBy
) : ICommand<Result>;
