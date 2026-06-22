using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Users.UpdateUser;

public sealed class UpdateUserCommandHandler(IUserRepository users, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateUserCommand, Result>
{
    public async Task<Result> HandleAsync(
        UpdateUserCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var user = await users.GetByIdAsync(command.UserId, cancellationToken);

        if (user is null)
            return Result.Failure(AuthErrors.UserNotFound);

        var sameUserName = await users.GetByUserNameAsync(command.UserName, cancellationToken);

        if (sameUserName is not null && sameUserName.Id != user.Id)
            return Result.Failure(AuthErrors.UserNameAlreadyExists);

        var sameEmail = await users.GetByEmailAsync(command.Email, cancellationToken);

        if (sameEmail is not null && sameEmail.Id != user.Id)
            return Result.Failure(AuthErrors.EmailAlreadyExists);

        user.UpdateProfile(command.UserName, command.Email, command.DisplayName, command.UpdatedBy);

        users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
