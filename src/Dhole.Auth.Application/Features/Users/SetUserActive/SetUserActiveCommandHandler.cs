using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Users.SetUserActive;

public sealed class SetUserActiveCommandHandler(IUserRepository users, IUnitOfWork unitOfWork)
    : ICommandHandler<SetUserActiveCommand, Result>
{
    public async Task<Result> HandleAsync(
        SetUserActiveCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var user = await users.GetByIdAsync(command.UserId, cancellationToken);

        if (user is null)
            return Result.Failure(AuthErrors.UserNotFound);

        user.SetActive(command.IsActive, command.UpdatedBy);

        users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
