using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Roles.AssignScopesToRole;

public sealed class AssignScopesToRoleCommandHandler(
    IRoleRepository roles,
    IScopeRepository scopes,
    IUnitOfWork unitOfWork
) : ICommandHandler<AssignScopesToRoleCommand, Result>
{
    public async Task<Result> HandleAsync(
        AssignScopesToRoleCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var role = await roles.GetWithScopesAsync(command.RoleId, cancellationToken);

        if (role is null)
        {
            return Result.Failure(AuthErrors.RoleNotFound);
        }

        var scopeIds = command.ScopeIds.Distinct().ToList();

        foreach (var scopeId in scopeIds)
        {
            var scope = await scopes.GetByIdAsync(scopeId, cancellationToken);

            if (scope is null)
            {
                return Result.Failure(AuthErrors.ScopeNotFound);
            }

            if (!scope.IsActive)
            {
                return Result.Failure(AuthErrors.ScopeInactive);
            }

            role.AssignScope(scopeId, command.AssignedBy);
        }

        roles.Update(role);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
