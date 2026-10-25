using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Auditing;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Application.Auditing;

namespace Dhole.Auth.Application.Sessions.RevokeUserSessions;

public sealed class RevokeUserSessionsCommandHandler(
    ISessionRepository sessions,
    IUserRepository users,
    IAuthAuditService audit,
    IUnitOfWork unitOfWork
) : ICommandHandler<RevokeUserSessionsCommand, Result>
{
    public async Task<Result> HandleAsync(
        RevokeUserSessionsCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var targetUser = await users.GetByIdAsync(command.UserId, cancellationToken);
        var activeSessions = await sessions.GetActiveEntitiesByUserAsync(
            command.UserId,
            cancellationToken
        );

        var before = activeSessions.Select(SessionAuditSnapshot.From).ToArray();
        var revokedSessionIds = new List<Guid>();

        foreach (var session in activeSessions)
        {
            session.Revoke(
                command.RevokedBy,
                command.Reason ?? "Todas las sesiones del usuario fueron revocadas."
            );

            if (session.IsRevoked)
            {
                revokedSessionIds.Add(session.Id);
            }

            sessions.Update(session);
        }

        var after = activeSessions.Select(SessionAuditSnapshot.From).ToArray();

        if (revokedSessionIds.Count > 0)
        {
            await audit.PublishAsync(
                new AuthAuditEvent(
                    EventType: AuthAuditEventTypes.UserSessionsRevoked,
                    Action: AuthAuditActions.RevokedAll,
                    EntityType: AuthAuditEntityTypes.User,
                    EntityId: command.UserId,
                    ActorUserId: command.RevokedBy,
                    Before: before,
                    After: after,
                    Payload: new
                    {
                        targetUserId = command.UserId,
                        targetUserName = targetUser?.UserName,
                        targetUserEmail = targetUser?.Email,
                        revokedSessionIds,
                        revokedSessionsCount = revokedSessionIds.Count,
                        reason = command.Reason ?? "Todas las sesiones del usuario fueron revocadas.",
                    }
                ),
                cancellationToken
            );
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
