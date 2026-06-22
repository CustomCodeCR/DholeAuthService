using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Repositories;

namespace Dhole.Auth.Application.Sessions.RevokeUserSessions;

public sealed class RevokeUserSessionsCommandHandler(
    ISessionRepository sessions,
    IUnitOfWork unitOfWork
) : ICommandHandler<RevokeUserSessionsCommand, Result>
{
    public async Task<Result> HandleAsync(
        RevokeUserSessionsCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var activeSessions = await sessions.GetActiveEntitiesByUserAsync(
            command.UserId,
            cancellationToken
        );

        foreach (var session in activeSessions)
        {
            session.Revoke(
                command.RevokedBy,
                command.Reason ?? "Todas las sesiones del usuario fueron revocadas."
            );

            sessions.Update(session);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
