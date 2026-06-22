using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Users.AssignRolesToUser;

public sealed class AssignRolesToUserCommandHandler(
    IUserRepository users,
    IRoleRepository roles,
    IUnitOfWork unitOfWork
) : ICommandHandler<AssignRolesToUserCommand, Result>
{
    public async Task<Result> HandleAsync(
        AssignRolesToUserCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var user = await users.GetWithRolesAndScopesAsync(command.UserId, cancellationToken);

        if (user is null)
            return Result.Failure(AuthErrors.UserNotFound);

        foreach (var roleId in command.RoleIds.Distinct())
        {
            var role = await roles.GetByIdAsync(roleId, cancellationToken);

            if (role is null)
                return Result.Failure(AuthErrors.RoleNotFound);

            if (!role.IsActive)
                return Result.Failure(AuthErrors.RoleInactive);

            user.AssignRole(roleId, command.AssignedBy);
        }

        users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
