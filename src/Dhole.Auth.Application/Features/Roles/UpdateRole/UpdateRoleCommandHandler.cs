using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Roles.UpdateRole;

public sealed class UpdateRoleCommandHandler(IRoleRepository roles, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateRoleCommand, Result>
{
    public async Task<Result> HandleAsync(
        UpdateRoleCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var role = await roles.GetByIdAsync(command.RoleId, cancellationToken);

        if (role is null)
        {
            return Result.Failure(AuthErrors.RoleNotFound);
        }

        var roleWithSameName = await roles.GetByNameAsync(command.Name, cancellationToken);

        if (roleWithSameName is not null && roleWithSameName.Id != role.Id)
        {
            return Result.Failure(AuthErrors.RoleAlreadyExists);
        }

        role.Update(command.Name, command.Description, command.UpdatedBy);

        roles.Update(role);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
