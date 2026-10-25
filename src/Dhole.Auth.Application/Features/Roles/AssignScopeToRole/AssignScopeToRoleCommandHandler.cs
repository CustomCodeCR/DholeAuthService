using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Auditing;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Application.Auditing;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Roles.AssignScopesToRole;

public sealed class AssignScopesToRoleCommandHandler(
    IRoleRepository roles,
    IScopeRepository scopes,
    IAuthAuditService audit,
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

        var before = RoleAuditSnapshot.From(role);
        var existingScopeIds = before.ScopeIds.ToHashSet();
        var assignedScopes = new List<object>();
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

            if (!existingScopeIds.Contains(scopeId))
            {
                assignedScopes.Add(
                    new
                    {
                        scopeId = scope.Id,
                        scopeCode = scope.Code,
                        scopeName = scope.Name,
                        scopeDescription = scope.Description,
                    }
                );
            }
        }

        var after = RoleAuditSnapshot.From(role);

        roles.Update(role);

        if (assignedScopes.Count > 0)
        {
            await audit.PublishAsync(
                new AuthAuditEvent(
                    EventType: AuthAuditEventTypes.RoleScopeAssigned,
                    Action: AuthAuditActions.ScopeAssigned,
                    EntityType: AuthAuditEntityTypes.Role,
                    EntityId: role.Id,
                    ActorUserId: command.AssignedBy,
                    Before: before,
                    After: after,
                    Payload: new
                    {
                        targetRoleId = role.Id,
                        targetRoleName = role.Name,
                        assignedScopes,
                    },
                    Metadata: new
                    {
                        operation = "assign_scopes_to_role",
                        requestedScopeIds = scopeIds.OrderBy(x => x).ToArray(),
                    }
                ),
                cancellationToken
            );
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
