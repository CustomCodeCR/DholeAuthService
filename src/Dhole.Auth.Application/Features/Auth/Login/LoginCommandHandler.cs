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

namespace Dhole.Auth.Application.Auth.Login;

public sealed class LoginCommandHandler(
    IUserRepository users,
    ISessionRepository sessions,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    IEffectivePermissionService effectivePermissionService,
    IPasswordHasher passwordHasher,
    IAuthAuditService audit,
    IUnitOfWork unitOfWork
) : ICommandHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var email = command.Email.Trim().ToLowerInvariant();

        var user = await users.GetByEmailAsync(email, cancellationToken);

        if (user is null)
        {
            await AuditLoginFailedAsync(
                email,
                null,
                null,
                "invalid_credentials",
                "Auth.InvalidCredentials",
                cancellationToken
            );

            return Result.Failure<LoginResponse>(AuthErrors.InvalidCredentials);
        }

        if (!user.IsActive)
        {
            await AuditLoginFailedAsync(
                email,
                user.Id,
                user.UserName,
                "user_inactive",
                "Auth.UserInactive",
                cancellationToken
            );

            return Result.Failure<LoginResponse>(AuthErrors.UserInactive);
        }

        if (user.IsLocked)
        {
            await AuditLoginFailedAsync(
                email,
                user.Id,
                user.UserName,
                "user_locked",
                "Auth.UserLocked",
                cancellationToken
            );

            return Result.Failure<LoginResponse>(AuthErrors.UserLocked);
        }

        var isPasswordValid = passwordHasher.Verify(command.Password, user.PasswordHash);

        if (!isPasswordValid)
        {
            await AuditLoginFailedAsync(
                email,
                user.Id,
                user.UserName,
                "invalid_credentials",
                "Auth.InvalidCredentials",
                cancellationToken
            );

            return Result.Failure<LoginResponse>(AuthErrors.InvalidCredentials);
        }

        var permissions = await effectivePermissionService.GetAsync(user.Id, cancellationToken);

        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(30);
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        var singleActiveSessions = await sessions.GetActiveEntitiesByUserAsync(
            user.Id,
            cancellationToken
        );

        foreach (var activeSession in singleActiveSessions.OrderBy(x => x.LastUsedAt))
        {
            activeSession.Revoke(
                revokedBy: user.Id,
                reason: "Superseded by newer login"
            );

            sessions.Update(activeSession);
        }

        user.RegisterSuccessfulLogin();
        users.Update(user);

        var refreshToken = refreshTokenGenerator.Generate();
        var refreshTokenHash = refreshTokenGenerator.Hash(refreshToken);

        var session = Session.Create(
            user.Id,
            refreshTokenHash,
            refreshTokenExpiresAt,
            command.IpAddress,
            command.UserAgent
        );

        await sessions.AddAsync(session, cancellationToken);

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
                EventType: AuthAuditEventTypes.LoginSucceeded,
                Action: AuthAuditActions.LoginSucceeded,
                EntityType: AuthAuditEntityTypes.Session,
                EntityId: session.Id,
                ActorUserId: user.Id,
                ActorUserName: user.UserName,
                After: SessionAuditSnapshot.From(session),
                Payload: new
                {
                    sessionId = session.Id,
                    userId = user.Id,
                    userName = user.UserName,
                    email = user.Email,
                    userType = user.UserType.ToString(),
                    roles = permissions.Roles.OrderBy(x => x).ToArray(),
                    effectiveScopes = permissions.EffectiveScopes.OrderBy(x => x).ToArray(),
                    accessTokenExpiresAt,
                    refreshTokenExpiresAt,
                },
                Metadata: new
                {
                    operation = "login",
                    refreshTokenIncluded = false,
                    accessTokenIncluded = false,
                }
            ),
            cancellationToken
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(
            new LoginResponse(
                accessToken,
                refreshToken,
                session.Id,
                accessTokenExpiresAt,
                refreshTokenExpiresAt,
                user.DisplayName,
                user.UserName,
                user.Email
            )
        );
    }

    private async Task AuditLoginFailedAsync(
        string email,
        Guid? userId,
        string? userName,
        string reason,
        string errorCode,
        CancellationToken cancellationToken
    )
    {
        await audit.PublishAsync(
            new AuthAuditEvent(
                EventType: AuthAuditEventTypes.LoginFailed,
                Action: AuthAuditActions.LoginFailed,
                EntityType: AuthAuditEntityTypes.Authentication,
                EntityId: userId,
                ActorUserId: userId,
                ActorUserName: userName,
                Payload: new
                {
                    email,
                    userId,
                    userName,
                    reason,
                    errorCode,
                },
                Metadata: new
                {
                    operation = "login",
                    passwordIncluded = false,
                },
                ErrorMessage: errorCode
            ),
            cancellationToken
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
