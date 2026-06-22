using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Auditing;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Application.Auditing;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Users.UpdateUser;

public sealed class UpdateUserCommandHandler(
    IUserRepository users,
    IAuthAuditService audit,
    IUnitOfWork unitOfWork
) : ICommandHandler<UpdateUserCommand, Result>
{
    public async Task<Result> HandleAsync(
        UpdateUserCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var user = await users.GetWithRolesAndScopesAsync(command.UserId, cancellationToken);

        if (user is null)
            return Result.Failure(AuthErrors.UserNotFound);

        var sameUserName = await users.GetByUserNameAsync(command.UserName, cancellationToken);

        if (sameUserName is not null && sameUserName.Id != user.Id)
            return Result.Failure(AuthErrors.UserNameAlreadyExists);

        var sameEmail = await users.GetByEmailAsync(command.Email, cancellationToken);

        if (sameEmail is not null && sameEmail.Id != user.Id)
            return Result.Failure(AuthErrors.EmailAlreadyExists);

        var before = UserAuditSnapshot.From(user);

        user.UpdateProfile(command.UserName, command.Email, command.DisplayName, command.UpdatedBy);

        var after = UserAuditSnapshot.From(user);

        users.Update(user);

        await audit.PublishAsync(
            new AuthAuditEvent(
                EventType: AuthAuditEventTypes.UserUpdated,
                Action: AuthAuditActions.Updated,
                EntityType: AuthAuditEntityTypes.User,
                EntityId: user.Id,
                ActorUserId: command.UpdatedBy,
                Before: before,
                After: after,
                Payload: new
                {
                    targetUserId = user.Id,
                    beforeUserName = before.UserName,
                    afterUserName = after.UserName,
                    beforeEmail = before.Email,
                    afterEmail = after.Email,
                    beforeDisplayName = before.DisplayName,
                    afterDisplayName = after.DisplayName,
                },
                Metadata: new
                {
                    changedFields = GetChangedFields(before, after),
                }
            ),
            cancellationToken
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static IReadOnlyCollection<string> GetChangedFields(
        UserAuditSnapshot before,
        UserAuditSnapshot after
    )
    {
        var fields = new List<string>();

        if (before.UserName != after.UserName)
            fields.Add("userName");

        if (before.Email != after.Email)
            fields.Add("email");

        if (before.DisplayName != after.DisplayName)
            fields.Add("displayName");

        return fields;
    }
}
