using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Sessions.RevokeSession;

public sealed class RevokeSessionCommandHandler(ISessionRepository sessions, IUnitOfWork unitOfWork)
    : ICommandHandler<RevokeSessionCommand, Result>
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

        session.Revoke(command.RevokedBy, command.Reason ?? "Sesión revocada manualmente.");

        sessions.Update(session);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
