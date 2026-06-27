using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Auditing;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Application.Users;
using Dhole.Auth.Application.Auditing;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Users.RevokeRolesFromUser;

public sealed class RevokeRolesFromUserCommandHandler(
    IUserRepository users,
    IRoleRepository roles,
    IAuthAuditService audit,
    IUnitOfWork unitOfWork
) : ICommandHandler<RevokeRolesFromUserCommand, Result>
{
    public async Task<Result> HandleAsync(
        RevokeRolesFromUserCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var user = await users.GetWithRolesAndScopesAsync(command.UserId, cancellationToken);

        if (user is null)
            return Result.Failure(AuthErrors.UserNotFound);


        if (ProtectedSeedUserGuard.IsProtected(user.Email))
            return Result.Failure(AuthErrors.ProtectedSeedUser);

        var before = UserAuditSnapshot.From(user);
        var existingRoleIds = before.RoleIds.ToHashSet();
        var revokedRoles = new List<object>();

        foreach (var roleId in command.RoleIds.Distinct())
        {
            var role = await roles.GetByIdAsync(roleId, cancellationToken);
            user.RevokeRole(roleId, command.RevokedBy);

            if (existingRoleIds.Contains(roleId))
            {
                revokedRoles.Add(
                    new
                    {
                        roleId,
                        roleName = role?.Name,
                        roleDescription = role?.Description,
                    }
                );
            }
        }

        var after = UserAuditSnapshot.From(user);

        users.Update(user);

        if (revokedRoles.Count > 0)
        {
            await audit.PublishAsync(
                new AuthAuditEvent(
                    EventType: AuthAuditEventTypes.UserRoleRevoked,
                    Action: AuthAuditActions.RoleRevoked,
                    EntityType: AuthAuditEntityTypes.User,
                    EntityId: user.Id,
                    ActorUserId: command.RevokedBy,
                    Before: before,
                    After: after,
                    Payload: new
                    {
                        targetUserId = user.Id,
                        targetUserName = user.UserName,
                        targetUserEmail = user.Email,
                        revokedRoles,
                    },
                    Metadata: new
                    {
                        operation = "revoke_roles_from_user",
                        requestedRoleIds = command.RoleIds.Distinct().OrderBy(x => x).ToArray(),
                    }
                ),
                cancellationToken
            );
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
