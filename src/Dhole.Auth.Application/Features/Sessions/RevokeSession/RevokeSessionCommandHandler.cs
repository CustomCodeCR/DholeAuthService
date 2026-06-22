using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Auditing;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Application.Auditing;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Sessions.RevokeSession;

public sealed class RevokeSessionCommandHandler(
    ISessionRepository sessions,
    IUserRepository users,
    IAuthAuditService audit,
    IUnitOfWork unitOfWork
) : ICommandHandler<RevokeSessionCommand, Result>
{
    public async Task<Result> HandleAsync(
        RevokeSessionCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var session = await sessions.GetByIdAsync(command.SessionId, cancellationToken);

        if (session is null)
        {
            return Result.Failure(AuthErrors.SessionNotFound);
        }

        var targetUser = await users.GetByIdAsync(session.UserId, cancellationToken);
        var before = SessionAuditSnapshot.From(session);

        session.Revoke(command.RevokedBy, command.Reason ?? "Sesión revocada manualmente.");

        var after = SessionAuditSnapshot.From(session);

        sessions.Update(session);

        if (!before.IsRevoked && after.IsRevoked)
        {
            await audit.PublishAsync(
                new AuthAuditEvent(
                    EventType: AuthAuditEventTypes.SessionRevoked,
                    Action: AuthAuditActions.Revoked,
                    EntityType: AuthAuditEntityTypes.Session,
                    EntityId: session.Id,
                    ActorUserId: command.RevokedBy,
                    Before: before,
                    After: after,
                    Payload: new
                    {
                        revokedSessionId = session.Id,
                        targetUserId = session.UserId,
                        targetUserName = targetUser?.UserName,
                        targetUserEmail = targetUser?.Email,
                        reason = after.RevocationReason,
                    }
                ),
                cancellationToken
            );
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
