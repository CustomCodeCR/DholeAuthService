using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using Dhole.Auth.Domain.Users.Enums;

namespace Dhole.Auth.Application.Users.CreateUser;

public sealed record CreateUserCommand(
    string UserName,
    string Email,
    string DisplayName,
    UserType UserType,
    string Password,
    Guid? CreatedBy
) : ICommand<Result<Guid>>;
