using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Roles.RevokeScopesFromRole;

public sealed class RevokeScopesFromRoleCommandHandler(
    IRoleRepository roles,
    IUnitOfWork unitOfWork
) : ICommandHandler<RevokeScopesFromRoleCommand, Result>
{
    public async Task<Result> HandleAsync(
        RevokeScopesFromRoleCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var role = await roles.GetWithScopesAsync(command.RoleId, cancellationToken);

        if (role is null)
        {
            return Result.Failure(AuthErrors.RoleNotFound);
        }

        foreach (var scopeId in command.ScopeIds.Distinct())
        {
            role.RevokeScope(scopeId, command.RevokedBy);
        }

        roles.Update(role);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
