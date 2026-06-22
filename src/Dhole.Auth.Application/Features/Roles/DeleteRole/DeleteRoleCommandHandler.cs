using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Roles.DeleteRole;

public sealed class DeleteRoleCommandHandler(IRoleRepository roles, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteRoleCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeleteRoleCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var role = await roles.GetByIdAsync(command.RoleId, cancellationToken);

        if (role is null)
        {
            return Result.Failure(AuthErrors.RoleNotFound);
        }

        if (role.IsSystemRole)
        {
            return Result.Failure(AuthErrors.SystemRoleCannotBeDeleted);
        }

        role.Delete(command.DeletedBy);

        roles.Update(role);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
