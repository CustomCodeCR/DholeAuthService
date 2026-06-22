using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Users.SetUserLocked;

public sealed class SetUserLockedCommandHandler(IUserRepository users, IUnitOfWork unitOfWork)
    : ICommandHandler<SetUserLockedCommand, Result>
{
    public async Task<Result> HandleAsync(
        SetUserLockedCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var user = await users.GetByIdAsync(command.UserId, cancellationToken);

        if (user is null)
            return Result.Failure(AuthErrors.UserNotFound);

        user.SetLocked(command.IsLocked, command.Reason, command.UpdatedBy);

        users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
