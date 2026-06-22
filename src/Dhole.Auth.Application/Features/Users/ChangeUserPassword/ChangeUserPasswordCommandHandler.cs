using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Auditing;
using Dhole.Auth.Application.Abstractions.Authentication;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Application.Auditing;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Users.ChangeUserPassword;

public sealed class ChangeUserPasswordCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IAuthAuditService audit,
    IUnitOfWork unitOfWork
) : ICommandHandler<ChangeUserPasswordCommand, Result>
{
    public async Task<Result> HandleAsync(
        ChangeUserPasswordCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var user = await users.GetWithRolesAndScopesAsync(command.UserId, cancellationToken);

        if (user is null)
            return Result.Failure(AuthErrors.UserNotFound);

        var before = UserAuditSnapshot.From(user);
        var passwordHash = passwordHasher.Hash(command.Password);

        user.ChangePassword(passwordHash, command.UpdatedBy);

        var after = UserAuditSnapshot.From(user);

        users.Update(user);

        await audit.PublishAsync(
            new AuthAuditEvent(
                EventType: AuthAuditEventTypes.UserPasswordChanged,
                Action: AuthAuditActions.PasswordChanged,
                EntityType: AuthAuditEntityTypes.User,
                EntityId: user.Id,
                ActorUserId: command.UpdatedBy,
                Before: before,
                After: after,
                Payload: new
                {
                    targetUserId = user.Id,
                    targetUserName = user.UserName,
                    tokenVersionBefore = before.TokenVersion,
                    tokenVersionAfter = after.TokenVersion,
                },
                Metadata: new
                {
                    passwordHashIncluded = false,
                    tokensInvalidated = after.TokenVersion != before.TokenVersion,
                }
            ),
            cancellationToken
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
