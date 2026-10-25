using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Auth.Application.Users.RevokeRolesFromUser;

public sealed record RevokeRolesFromUserCommand(
    Guid UserId,
    IReadOnlyCollection<Guid> RoleIds,
    Guid? RevokedBy
) : ICommand<Result>;
