using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Auditing;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Application.Auditing;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Roles.DeleteRole;

public sealed class DeleteRoleCommandHandler(
    IRoleRepository roles,
    IAuthAuditService audit,
    IUnitOfWork unitOfWork
) : ICommandHandler<DeleteRoleCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeleteRoleCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var role = await roles.GetWithScopesAsync(command.RoleId, cancellationToken);

        if (role is null)
        {
            return Result.Failure(AuthErrors.RoleNotFound);
        }

        if (role.IsSystemRole)
        {
            return Result.Failure(AuthErrors.SystemRoleCannotBeDeleted);
        }

        var before = RoleAuditSnapshot.From(role);

        role.Delete(command.DeletedBy);

        var after = RoleAuditSnapshot.From(role);

        roles.Update(role);

        await audit.PublishAsync(
            new AuthAuditEvent(
                EventType: AuthAuditEventTypes.RoleDeleted,
                Action: AuthAuditActions.Deleted,
                EntityType: AuthAuditEntityTypes.Role,
                EntityId: role.Id,
                ActorUserId: command.DeletedBy,
                Before: before,
                After: after,
                Payload: new
                {
                    deletedRoleId = role.Id,
                    deletedRoleName = role.Name,
                }
            ),
            cancellationToken
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
