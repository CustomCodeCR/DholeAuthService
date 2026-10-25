using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Auditing;
using Dhole.Auth.Application.Abstractions.Authentication;
using Dhole.Auth.Application.Abstractions.Permissions;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Application.Auditing;
using Dhole.Auth.Contracts.Authentication;
using Dhole.Auth.Domain.Sessions.Entities;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Auth.StartImpersonation;

public sealed class StartImpersonationCommandHandler(
    IUserRepository users,
    ISessionRepository sessions,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    IEffectivePermissionService effectivePermissionService,
    IAuthAuditService audit,
    IUnitOfWork unitOfWork
) : ICommandHandler<StartImpersonationCommand, Result<ImpersonationResponse>>
{
    public async Task<Result<ImpersonationResponse>> HandleAsync(
        StartImpersonationCommand command,
        CancellationToken cancellationToken = default
    )
    {
        if (command.ActorUserId == command.TargetUserId)
        {
            return Result.Failure<ImpersonationResponse>(AuthErrors.CannotImpersonateSelf);
        }

        var actor = await users.GetByIdAsync(command.ActorUserId, cancellationToken);
        if (actor is null)
        {
            return Result.Failure<ImpersonationResponse>(AuthErrors.UserNotFound);
        }

        var target = await users.GetByIdAsync(command.TargetUserId, cancellationToken);
        if (target is null)
        {
            return Result.Failure<ImpersonationResponse>(AuthErrors.UserNotFound);
        }

        if (!target.IsActive)
        {
            return Result.Failure<ImpersonationResponse>(AuthErrors.UserInactive);
        }

        if (target.IsLocked)
        {
            return Result.Failure<ImpersonationResponse>(AuthErrors.UserLocked);
        }

        var actorPermissions = await effectivePermissionService.GetAsync(actor.Id, cancellationToken);
        var targetPermissions = await effectivePermissionService.GetAsync(target.Id, cancellationToken);

        var actorIsSuperUser = actorPermissions.Roles.Contains(
            AuthConstants.SystemRoles.SuperUser,
            StringComparer.OrdinalIgnoreCase
        );
        var targetIsSuperUser = targetPermissions.Roles.Contains(
            AuthConstants.SystemRoles.SuperUser,
            StringComparer.OrdinalIgnoreCase
        );

        if (targetIsSuperUser && !actorIsSuperUser)
        {
            return Result.Failure<ImpersonationResponse>(AuthErrors.CannotImpersonateSuperUser);
        }

        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(30);
        var hiddenRefreshToken = refreshTokenGenerator.Generate();

        var session = Session.Create(
            target.Id,
            refreshTokenGenerator.Hash(hiddenRefreshToken),
            accessTokenExpiresAt,
            command.IpAddress,
            command.UserAgent
        );

        await sessions.AddAsync(session, cancellationToken);

        var accessToken = jwtTokenGenerator.Generate(
            target.Id,
            session.Id,
            target.UserType.ToString(),
            target.Email,
            target.UserName,
            target.DisplayName,
            targetPermissions.Roles.ToList(),
            targetPermissions.EffectiveScopes.ToList(),
            target.TokenVersion,
            accessTokenExpiresAt,
            new Dictionary<string, string>
            {
                ["impersonation"] = "true",
                ["impersonator_user_id"] = actor.Id.ToString(),
                ["impersonator_user_name"] = actor.UserName,
                ["impersonated_user_id"] = target.Id.ToString(),
            }
        );

        await audit.PublishAsync(
            new AuthAuditEvent(
                EventType: AuthAuditEventTypes.ImpersonationStarted,
                Action: AuthAuditActions.ImpersonationStarted,
                EntityType: AuthAuditEntityTypes.Session,
                EntityId: session.Id,
                ActorUserId: actor.Id,
                ActorUserName: actor.UserName,
                After: SessionAuditSnapshot.From(session),
                Payload: new
                {
                    sessionId = session.Id,
                    impersonatorUserId = actor.Id,
                    impersonatorUserName = actor.UserName,
                    targetUserId = target.Id,
                    targetUserName = target.UserName,
                    targetEmail = target.Email,
                    accessTokenExpiresAt,
                },
                Metadata: new
                {
                    operation = "impersonation_start",
                    passwordUsed = false,
                    refreshTokenIncluded = false,
                    accessTokenIncluded = false,
                }
            ),
            cancellationToken
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(
            new ImpersonationResponse(
                accessToken,
                session.Id,
                accessTokenExpiresAt,
                target.DisplayName,
                target.UserName,
                target.Email,
                actor.Id,
                actor.UserName
            )
        );
    }
}
