using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Auditing;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Application.Auditing;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Auth.StopImpersonation;

public sealed class StopImpersonationCommandHandler(
    ISessionRepository sessions,
    IUserRepository users,
    IAuthAuditService audit,
    IUnitOfWork unitOfWork
) : ICommandHandler<StopImpersonationCommand, Result>
{
    public async Task<Result> HandleAsync(
        StopImpersonationCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var session = await sessions.GetByIdAsync(command.SessionId, cancellationToken);
        if (session is null || session.UserId != command.TargetUserId)
        {
            return Result.Failure(AuthErrors.SessionNotFound);
        }

        var impersonator = await users.GetByIdAsync(command.ImpersonatorUserId, cancellationToken);
        var target = await users.GetByIdAsync(command.TargetUserId, cancellationToken);
        var before = SessionAuditSnapshot.From(session);

        session.Revoke(command.ImpersonatorUserId, "Impersonation stopped");
        sessions.Update(session);

        var after = SessionAuditSnapshot.From(session);

        await audit.PublishAsync(
            new AuthAuditEvent(
                EventType: AuthAuditEventTypes.ImpersonationStopped,
                Action: AuthAuditActions.ImpersonationStopped,
                EntityType: AuthAuditEntityTypes.Session,
                EntityId: session.Id,
                ActorUserId: command.ImpersonatorUserId,
                ActorUserName: impersonator?.UserName,
                Before: before,
                After: after,
                Payload: new
                {
                    sessionId = session.Id,
                    impersonatorUserId = command.ImpersonatorUserId,
                    impersonatorUserName = impersonator?.UserName,
                    targetUserId = command.TargetUserId,
                    targetUserName = target?.UserName,
                    targetEmail = target?.Email,
                },
                Metadata: new
                {
                    operation = "impersonation_stop",
                }
            ),
            cancellationToken
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
