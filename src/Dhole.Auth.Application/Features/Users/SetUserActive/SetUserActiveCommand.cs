using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Auth.Application.Users.SetUserActive;

public sealed record SetUserActiveCommand(Guid UserId, bool IsActive, Guid? UpdatedBy)
    : ICommand<Result>;
