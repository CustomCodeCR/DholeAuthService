using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Auditing;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Application.Auditing;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Users.SetUserActive;

public sealed class SetUserActiveCommandHandler(
    IUserRepository users,
    IAuthAuditService audit,
    IUnitOfWork unitOfWork
) : ICommandHandler<SetUserActiveCommand, Result>
{
    public async Task<Result> HandleAsync(
        SetUserActiveCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var user = await users.GetWithRolesAndScopesAsync(command.UserId, cancellationToken);

        if (user is null)
            return Result.Failure(AuthErrors.UserNotFound);

        var before = UserAuditSnapshot.From(user);

        user.SetActive(command.IsActive, command.UpdatedBy);

        var after = UserAuditSnapshot.From(user);

        users.Update(user);

        if (before.IsActive != after.IsActive)
        {
            await audit.PublishAsync(
                new AuthAuditEvent(
                    EventType: command.IsActive
                        ? AuthAuditEventTypes.UserActivated
                        : AuthAuditEventTypes.UserInactivated,
                    Action: command.IsActive
                        ? AuthAuditActions.Activated
                        : AuthAuditActions.Inactivated,
                    EntityType: AuthAuditEntityTypes.User,
                    EntityId: user.Id,
                    ActorUserId: command.UpdatedBy,
                    Before: before,
                    After: after,
                    Payload: new
                    {
                        targetUserId = user.Id,
                        targetUserName = user.UserName,
                        previousIsActive = before.IsActive,
                        currentIsActive = after.IsActive,
                    }
                ),
                cancellationToken
            );
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
