using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Auth.Application.Users.ChangeUserPassword;

public sealed record ChangeUserPasswordCommand(Guid UserId, string Password, Guid? UpdatedBy)
    : ICommand<Result>;
