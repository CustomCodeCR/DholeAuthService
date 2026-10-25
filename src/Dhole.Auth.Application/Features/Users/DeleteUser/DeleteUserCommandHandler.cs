using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Auditing;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Application.Users;
using Dhole.Auth.Application.Auditing;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Users.DeleteUser;

public sealed class DeleteUserCommandHandler(
    IUserRepository users,
    IAuthAuditService audit,
    IUnitOfWork unitOfWork
) : ICommandHandler<DeleteUserCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeleteUserCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var user = await users.GetWithRolesAndScopesAsync(command.UserId, cancellationToken);

        if (user is null)
            return Result.Failure(AuthErrors.UserNotFound);


        if (ProtectedSeedUserGuard.IsProtected(user.Email))
            return Result.Failure(AuthErrors.ProtectedSeedUser);

        var before = UserAuditSnapshot.From(user);

        user.Delete(command.DeletedBy);

        var after = UserAuditSnapshot.From(user);

        users.Update(user);

        await audit.PublishAsync(
            new AuthAuditEvent(
                EventType: AuthAuditEventTypes.UserDeleted,
                Action: AuthAuditActions.Deleted,
                EntityType: AuthAuditEntityTypes.User,
                EntityId: user.Id,
                ActorUserId: command.DeletedBy,
                Before: before,
                After: after,
                Payload: new
                {
                    deletedUserId = user.Id,
                    deletedUserName = user.UserName,
                    deletedUserEmail = user.Email,
                }
            ),
            cancellationToken
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
