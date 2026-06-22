using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Auditing;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Application.Auditing;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Users.RevokeScopesFromUser;

public sealed class RevokeScopesFromUserCommandHandler(
    IUserRepository users,
    IScopeRepository scopes,
    IAuthAuditService audit,
    IUnitOfWork unitOfWork
) : ICommandHandler<RevokeScopesFromUserCommand, Result>
{
    public async Task<Result> HandleAsync(
        RevokeScopesFromUserCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var user = await users.GetWithRolesAndScopesAsync(command.UserId, cancellationToken);

        if (user is null)
            return Result.Failure(AuthErrors.UserNotFound);

        var before = UserAuditSnapshot.From(user);
        var existingScopeIds = before.DirectScopeIds.ToHashSet();
        var revokedScopes = new List<object>();

        foreach (var scopeId in command.ScopeIds.Distinct())
        {
            var scope = await scopes.GetByIdAsync(scopeId, cancellationToken);
            user.RevokeScope(scopeId, command.RevokedBy);

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

        var after = UserAuditSnapshot.From(user);

        users.Update(user);

        if (revokedScopes.Count > 0)
        {
            await audit.PublishAsync(
                new AuthAuditEvent(
                    EventType: AuthAuditEventTypes.UserScopeRevoked,
                    Action: AuthAuditActions.ScopeRevoked,
                    EntityType: AuthAuditEntityTypes.User,
                    EntityId: user.Id,
                    ActorUserId: command.RevokedBy,
                    Before: before,
                    After: after,
                    Payload: new
                    {
                        targetUserId = user.Id,
                        targetUserName = user.UserName,
                        targetUserEmail = user.Email,
                        revokedScopes,
                    },
                    Metadata: new
                    {
                        operation = "revoke_direct_scopes_from_user",
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
