using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Auth.Application.Users.UpdateUser;

public sealed record UpdateUserCommand(
    Guid UserId,
    string UserName,
    string Email,
    string DisplayName,
    Guid? UpdatedBy
) : ICommand<Result>;
