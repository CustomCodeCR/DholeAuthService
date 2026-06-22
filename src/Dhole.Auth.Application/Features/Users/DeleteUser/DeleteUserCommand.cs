using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Auth.Application.Users.DeleteUser;

public sealed record DeleteUserCommand(Guid UserId, Guid? DeletedBy) : ICommand<Result>;
