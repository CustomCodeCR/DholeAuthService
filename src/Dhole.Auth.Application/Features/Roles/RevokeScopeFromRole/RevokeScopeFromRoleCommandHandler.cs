using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Auditing;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Application.Auditing;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Roles.RevokeScopesFromRole;

public sealed class RevokeScopesFromRoleCommandHandler(
    IRoleRepository roles,
    IScopeRepository scopes,
    IAuthAuditService audit,
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

        var before = RoleAuditSnapshot.From(role);
        var existingScopeIds = before.ScopeIds.ToHashSet();
        var revokedScopes = new List<object>();

        foreach (var scopeId in command.ScopeIds.Distinct())
        {
            var scope = await scopes.GetByIdAsync(scopeId, cancellationToken);
            role.RevokeScope(scopeId, command.RevokedBy);

            if (existingScopeIds.Contains(scopeId))
            {
                revokedScopes.Add(
                    new
                    {
                        scopeId,
                        scopeCode = scope?.Code,
                        scopeName = scope?.Name,
                        scopeDescription = scope?.Description,
                    }
                );
            }
        }

        var after = RoleAuditSnapshot.From(role);

        roles.Update(role);

        if (revokedScopes.Count > 0)
        {
            await audit.PublishAsync(
                new AuthAuditEvent(
                    EventType: AuthAuditEventTypes.RoleScopeRevoked,
                    Action: AuthAuditActions.ScopeRevoked,
                    EntityType: AuthAuditEntityTypes.Role,
                    EntityId: role.Id,
                    ActorUserId: command.RevokedBy,
                    Before: before,
                    After: after,
                    Payload: new
                    {
                        targetRoleId = role.Id,
                        targetRoleName = role.Name,
                        revokedScopes,
                    },
                    Metadata: new
                    {
                        operation = "revoke_scopes_from_role",
                        requestedScopeIds = command.ScopeIds.Distinct().OrderBy(x => x).ToArray(),
                    }
                ),
                cancellationToken
            );
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
