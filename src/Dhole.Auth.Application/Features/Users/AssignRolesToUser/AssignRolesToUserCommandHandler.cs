using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Auditing;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Application.Auditing;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Users.AssignRolesToUser;

public sealed class AssignRolesToUserCommandHandler(
    IUserRepository users,
    IRoleRepository roles,
    IAuthAuditService audit,
    IUnitOfWork unitOfWork
) : ICommandHandler<AssignRolesToUserCommand, Result>
{
    public async Task<Result> HandleAsync(
        AssignRolesToUserCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var user = await users.GetWithRolesAndScopesAsync(command.UserId, cancellationToken);

        if (user is null)
            return Result.Failure(AuthErrors.UserNotFound);

        var before = UserAuditSnapshot.From(user);
        var existingRoleIds = before.RoleIds.ToHashSet();
        var assignedRoles = new List<object>();

        foreach (var roleId in command.RoleIds.Distinct())
        {
            var role = await roles.GetByIdAsync(roleId, cancellationToken);

            if (role is null)
                return Result.Failure(AuthErrors.RoleNotFound);

            if (!role.IsActive)
                return Result.Failure(AuthErrors.RoleInactive);

            user.AssignRole(roleId, command.AssignedBy);

            if (!existingRoleIds.Contains(roleId))
            {
                assignedRoles.Add(
                    new
                    {
                        roleId = role.Id,
                        roleName = role.Name,
                        roleDescription = role.Description,
                    }
                );
            }
        }

        var after = UserAuditSnapshot.From(user);

        users.Update(user);

        if (assignedRoles.Count > 0)
        {
            await audit.PublishAsync(
                new AuthAuditEvent(
                    EventType: AuthAuditEventTypes.UserRoleAssigned,
                    Action: AuthAuditActions.RoleAssigned,
                    EntityType: AuthAuditEntityTypes.User,
                    EntityId: user.Id,
                    ActorUserId: command.AssignedBy,
                    Before: before,
                    After: after,
                    Payload: new
                    {
                        targetUserId = user.Id,
                        targetUserName = user.UserName,
                        targetUserEmail = user.Email,
                        assignedRoles,
                    },
                    Metadata: new
                    {
                        operation = "assign_roles_to_user",
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
