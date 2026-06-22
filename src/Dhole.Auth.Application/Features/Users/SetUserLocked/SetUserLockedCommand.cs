using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Auth.Application.Users.SetUserLocked;

public sealed record SetUserLockedCommand(
    Guid UserId,
    bool IsLocked,
    string? Reason,
    Guid? UpdatedBy
) : ICommand<Result>;
