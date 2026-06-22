using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Users.DeleteUser;

public sealed class DeleteUserCommandHandler(IUserRepository users, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteUserCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeleteUserCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var user = await users.GetByIdAsync(command.UserId, cancellationToken);

        if (user is null)
            return Result.Failure(AuthErrors.UserNotFound);

        user.Delete(command.DeletedBy);

        users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
