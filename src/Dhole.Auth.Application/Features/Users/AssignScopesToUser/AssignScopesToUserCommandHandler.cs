using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Auditing;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Application.Auditing;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Users.AssignScopesToUser;

public sealed class AssignScopesToUserCommandHandler(
    IUserRepository users,
    IScopeRepository scopes,
    IAuthAuditService audit,
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

        var before = UserAuditSnapshot.From(user);
        var existingScopeIds = before.DirectScopeIds.ToHashSet();
        var assignedScopes = new List<object>();

        foreach (var scopeId in command.ScopeIds.Distinct())
        {
            var scope = await scopes.GetByIdAsync(scopeId, cancellationToken);

            if (scope is null)
                return Result.Failure(AuthErrors.ScopeNotFound);

            if (!scope.IsActive)
                return Result.Failure(AuthErrors.ScopeInactive);

            user.AssignScope(scopeId, command.AssignedBy);

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

        var after = UserAuditSnapshot.From(user);

        users.Update(user);

        if (assignedScopes.Count > 0)
        {
            await audit.PublishAsync(
                new AuthAuditEvent(
                    EventType: AuthAuditEventTypes.UserScopeAssigned,
                    Action: AuthAuditActions.ScopeAssigned,
                    EntityType: AuthAuditEntityTypes.User,
                    EntityId: user.Id,
                    ActorUserId: command.AssignedBy,
                    Before: before,
                    After: after,
                    Payload: new
                    {
                        targetUserId = user.Id,
                        targetUserName = user.UserName,
                        targetUserEmail = user.Email,
                        assignedScopes,
                    },
                    Metadata: new
                    {
                        operation = "assign_direct_scopes_to_user",
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
