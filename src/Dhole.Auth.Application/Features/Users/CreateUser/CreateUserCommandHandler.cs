using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Authentication;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Domain.Shared;
using Dhole.Auth.Domain.Users.Entities;

namespace Dhole.Auth.Application.Users.CreateUser;

public sealed class CreateUserCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default
    )
    {
        if (await users.ExistsByUserNameAsync(command.UserName, cancellationToken))
            return Result.Failure<Guid>(AuthErrors.UserNameAlreadyExists);

        if (await users.ExistsByEmailAsync(command.Email, cancellationToken))
            return Result.Failure<Guid>(AuthErrors.EmailAlreadyExists);

        var passwordHash = passwordHasher.Hash(command.Password);

        var user = User.Create(
            command.UserName,
            command.Email,
            command.DisplayName,
            command.UserType,
            passwordHash,
            command.CreatedBy
        );

        await users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(user.Id);
    }
}
