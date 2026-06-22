using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Auditing;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Application.Auditing;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Roles.SetRoleActive;

public sealed class SetRoleActiveCommandHandler(
    IRoleRepository roles,
    IAuthAuditService audit,
    IUnitOfWork unitOfWork
) : ICommandHandler<SetRoleActiveCommand, Result>
{
    public async Task<Result> HandleAsync(
        SetRoleActiveCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var role = await roles.GetWithScopesAsync(command.RoleId, cancellationToken);

        if (role is null)
        {
            return Result.Failure(AuthErrors.RoleNotFound);
        }

        var before = RoleAuditSnapshot.From(role);

        role.SetActive(command.IsActive, command.UpdatedBy);

        var after = RoleAuditSnapshot.From(role);

        roles.Update(role);

        if (before.IsActive != after.IsActive)
        {
            await audit.PublishAsync(
                new AuthAuditEvent(
                    EventType: command.IsActive
                        ? AuthAuditEventTypes.RoleActivated
                        : AuthAuditEventTypes.RoleInactivated,
                    Action: command.IsActive
                        ? AuthAuditActions.Activated
                        : AuthAuditActions.Inactivated,
                    EntityType: AuthAuditEntityTypes.Role,
                    EntityId: role.Id,
                    ActorUserId: command.UpdatedBy,
                    Before: before,
                    After: after,
                    Payload: new
                    {
                        roleId = role.Id,
                        roleName = role.Name,
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
