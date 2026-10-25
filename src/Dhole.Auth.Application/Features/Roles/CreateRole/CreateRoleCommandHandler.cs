using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Auditing;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Application.Auditing;
using Dhole.Auth.Domain.Roles.Entities;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Roles.CreateRole;

public sealed class CreateRoleCommandHandler(
    IRoleRepository roles,
    IAuthAuditService audit,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateRoleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(
        CreateRoleCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var exists = await roles.ExistsByNameAsync(command.Name, cancellationToken);

        if (exists)
        {
            return Result.Failure<Guid>(AuthErrors.RoleAlreadyExists);
        }

        var role = Role.Create(
            command.Name,
            command.Description,
            command.IsSystemRole,
            command.CreatedBy
        );

        await roles.AddAsync(role, cancellationToken);

        await audit.PublishAsync(
            new AuthAuditEvent(
                EventType: AuthAuditEventTypes.RoleCreated,
                Action: AuthAuditActions.Created,
                EntityType: AuthAuditEntityTypes.Role,
                EntityId: role.Id,
                ActorUserId: command.CreatedBy,
                After: RoleAuditSnapshot.From(role),
                Payload: new
                {
                    createdRoleId = role.Id,
                    roleName = role.Name,
                    roleDescription = role.Description,
                    role.IsSystemRole,
                }
            ),
            cancellationToken
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(role.Id);
    }
}
