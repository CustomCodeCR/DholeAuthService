using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Roles.SetRoleActive;

public sealed class SetRoleActiveCommandHandler(IRoleRepository roles, IUnitOfWork unitOfWork)
    : ICommandHandler<SetRoleActiveCommand, Result>
{
    public async Task<Result> HandleAsync(
        SetRoleActiveCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var role = await roles.GetByIdAsync(command.RoleId, cancellationToken);

        if (role is null)
        {
            return Result.Failure(AuthErrors.RoleNotFound);
        }

        role.SetActive(command.IsActive, command.UpdatedBy);

        roles.Update(role);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
