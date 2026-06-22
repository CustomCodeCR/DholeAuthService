using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Users.RevokeRolesFromUser;

public sealed class RevokeRolesFromUserCommandHandler(IUserRepository users, IUnitOfWork unitOfWork)
    : ICommandHandler<RevokeRolesFromUserCommand, Result>
{
    public async Task<Result> HandleAsync(
        RevokeRolesFromUserCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var user = await users.GetWithRolesAndScopesAsync(command.UserId, cancellationToken);

        if (user is null)
            return Result.Failure(AuthErrors.UserNotFound);

        foreach (var roleId in command.RoleIds.Distinct())
        {
            user.RevokeRole(roleId, command.RevokedBy);
        }

        users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
