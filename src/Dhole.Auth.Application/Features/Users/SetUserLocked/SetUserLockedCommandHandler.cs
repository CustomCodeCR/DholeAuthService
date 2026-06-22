using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Auditing;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Application.Auditing;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Users.SetUserLocked;

public sealed class SetUserLockedCommandHandler(
    IUserRepository users,
    IAuthAuditService audit,
    IUnitOfWork unitOfWork
) : ICommandHandler<SetUserLockedCommand, Result>
{
    public async Task<Result> HandleAsync(
        SetUserLockedCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var user = await users.GetWithRolesAndScopesAsync(command.UserId, cancellationToken);

        if (user is null)
            return Result.Failure(AuthErrors.UserNotFound);

        var before = UserAuditSnapshot.From(user);

        user.SetLocked(command.IsLocked, command.Reason, command.UpdatedBy);

        var after = UserAuditSnapshot.From(user);

        users.Update(user);

        if (before.IsLocked != after.IsLocked)
        {
            await audit.PublishAsync(
                new AuthAuditEvent(
                    EventType: command.IsLocked
                        ? AuthAuditEventTypes.UserBlocked
                        : AuthAuditEventTypes.UserUnblocked,
                    Action: command.IsLocked
                        ? AuthAuditActions.Blocked
                        : AuthAuditActions.Unblocked,
                    EntityType: AuthAuditEntityTypes.User,
                    EntityId: user.Id,
                    ActorUserId: command.UpdatedBy,
                    Before: before,
                    After: after,
                    Payload: new
                    {
                        targetUserId = user.Id,
                        targetUserName = user.UserName,
                        previousIsLocked = before.IsLocked,
                        currentIsLocked = after.IsLocked,
                        reason = command.Reason,
                    }
                ),
                cancellationToken
            );
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
