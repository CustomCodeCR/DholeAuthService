using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Auditing;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Application.Auditing;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Roles.UpdateRole;

public sealed class UpdateRoleCommandHandler(
    IRoleRepository roles,
    IAuthAuditService audit,
    IUnitOfWork unitOfWork
) : ICommandHandler<UpdateRoleCommand, Result>
{
    public async Task<Result> HandleAsync(
        UpdateRoleCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var role = await roles.GetWithScopesAsync(command.RoleId, cancellationToken);

        if (role is null)
        {
            return Result.Failure(AuthErrors.RoleNotFound);
        }

        var roleWithSameName = await roles.GetByNameAsync(command.Name, cancellationToken);

        if (roleWithSameName is not null && roleWithSameName.Id != role.Id)
        {
            return Result.Failure(AuthErrors.RoleAlreadyExists);
        }

        var before = RoleAuditSnapshot.From(role);

        role.Update(command.Name, command.Description, command.UpdatedBy);

        var after = RoleAuditSnapshot.From(role);

        roles.Update(role);

        await audit.PublishAsync(
            new AuthAuditEvent(
                EventType: AuthAuditEventTypes.RoleUpdated,
                Action: AuthAuditActions.Updated,
                EntityType: AuthAuditEntityTypes.Role,
                EntityId: role.Id,
                ActorUserId: command.UpdatedBy,
                Before: before,
                After: after,
                Payload: new
                {
                    roleId = role.Id,
                    beforeName = before.Name,
                    afterName = after.Name,
                    beforeDescription = before.Description,
                    afterDescription = after.Description,
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
        RoleAuditSnapshot before,
        RoleAuditSnapshot after
    )
    {
        var fields = new List<string>();

        if (before.Name != after.Name)
            fields.Add("name");

        if (before.Description != after.Description)
            fields.Add("description");

        return fields;
    }
}
