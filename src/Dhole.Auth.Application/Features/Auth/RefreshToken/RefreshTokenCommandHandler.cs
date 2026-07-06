using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Auditing;
using Dhole.Auth.Application.Abstractions.Authentication;
using Dhole.Auth.Application.Abstractions.Permissions;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Application.Auditing;
using Dhole.Auth.Contracts.Authentication;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Auth.RefreshToken;

public sealed class RefreshTokenCommandHandler(
    IUserRepository users,
    ISessionRepository sessions,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    IEffectivePermissionService effectivePermissionService,
    IAuthAuditService audit,
    IUnitOfWork unitOfWork
) : ICommandHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    public async Task<Result<RefreshTokenResponse>> HandleAsync(
        RefreshTokenCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var refreshTokenHash = refreshTokenGenerator.Hash(command.RefreshToken);

        var session = await sessions.GetByRefreshTokenHashAsync(
            refreshTokenHash,
            cancellationToken
        );

        if (session is null)
        {
            return Result.Failure<RefreshTokenResponse>(AuthErrors.InvalidRefreshToken);
        }

        if (session.IsRevoked)
        {
            return Result.Failure<RefreshTokenResponse>(AuthErrors.SessionRevoked);
        }

        if (session.IsExpired(DateTime.UtcNow))
        {
            return Result.Failure<RefreshTokenResponse>(AuthErrors.RefreshTokenExpired);
        }

        var user = await users.GetByIdAsync(session.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<RefreshTokenResponse>(AuthErrors.UserNotFound);
        }

        if (!user.IsActive)
        {
            return Result.Failure<RefreshTokenResponse>(AuthErrors.UserInactive);
        }

        if (user.IsLocked)
        {
            return Result.Failure<RefreshTokenResponse>(AuthErrors.UserLocked);
        }

        var permissions = await effectivePermissionService.GetAsync(user.Id, cancellationToken);

        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(30);
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        var newRefreshToken = refreshTokenGenerator.Generate();
        var newRefreshTokenHash = refreshTokenGenerator.Hash(newRefreshToken);
        var before = SessionAuditSnapshot.From(session);

        session.Refresh(
            newRefreshTokenHash,
            refreshTokenExpiresAt,
            command.IpAddress,
            command.UserAgent
        );

        var after = SessionAuditSnapshot.From(session);

        sessions.Update(session);

        var accessToken = jwtTokenGenerator.Generate(
            user.Id,
            session.Id,
            user.UserType.ToString(),
            user.Email,
            user.UserName,
            user.DisplayName,
            permissions.Roles.ToList(),
            permissions.EffectiveScopes.ToList(),
            user.TokenVersion,
            accessTokenExpiresAt
        );

        await audit.PublishAsync(
            new AuthAuditEvent(
                EventType: AuthAuditEventTypes.SessionRefreshed,
                Action: AuthAuditActions.Refreshed,
                EntityType: AuthAuditEntityTypes.Session,
                EntityId: session.Id,
                ActorUserId: user.Id,
                ActorUserName: user.UserName,
                Before: before,
                After: after,
                Payload: new
                {
                    sessionId = session.Id,
                    userId = user.Id,
                    userName = user.UserName,
                    email = user.Email,
                    accessTokenExpiresAt,
                    refreshTokenExpiresAt,
                },
                Metadata: new
                {
                    operation = "refresh_token",
                    oldRefreshTokenIncluded = false,
                    newRefreshTokenIncluded = false,
                    accessTokenIncluded = false,
                }
            ),
            cancellationToken
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(
            new RefreshTokenResponse(
                accessToken,
                newRefreshToken,
                session.Id,
                accessTokenExpiresAt,
                refreshTokenExpiresAt,
                user.DisplayName,
                user.UserName,
                user.Email
            )
        );
    }
}
