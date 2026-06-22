using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Users.AssignScopesToUser;

public sealed class AssignScopesToUserCommandHandler(
    IUserRepository users,
    IScopeRepository scopes,
    IUnitOfWork unitOfWork
) : ICommandHandler<AssignScopesToUserCommand, Result>
{
    public async Task<Result> HandleAsync(
        AssignScopesToUserCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var user = await users.GetWithRolesAndScopesAsync(command.UserId, cancellationToken);

        if (user is null)
            return Result.Failure(AuthErrors.UserNotFound);

        foreach (var scopeId in command.ScopeIds.Distinct())
        {
            var scope = await scopes.GetByIdAsync(scopeId, cancellationToken);

            if (scope is null)
                return Result.Failure(AuthErrors.ScopeNotFound);

            if (!scope.IsActive)
                return Result.Failure(AuthErrors.ScopeInactive);

            user.AssignScope(scopeId, command.AssignedBy);
        }

        users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
