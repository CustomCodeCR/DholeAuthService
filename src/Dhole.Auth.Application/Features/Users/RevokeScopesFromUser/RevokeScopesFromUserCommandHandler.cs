using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Users.RevokeScopesFromUser;

public sealed class RevokeScopesFromUserCommandHandler(
    IUserRepository users,
    IUnitOfWork unitOfWork
) : ICommandHandler<RevokeScopesFromUserCommand, Result>
{
    public async Task<Result> HandleAsync(
        RevokeScopesFromUserCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var user = await users.GetWithRolesAndScopesAsync(command.UserId, cancellationToken);

        if (user is null)
            return Result.Failure(AuthErrors.UserNotFound);

        foreach (var scopeId in command.ScopeIds.Distinct())
        {
            user.RevokeScope(scopeId, command.RevokedBy);
        }

        users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
