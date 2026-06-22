using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Authentication;
using Dhole.Auth.Application.Abstractions.Permissions;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Contracts.Authentication;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Auth.RefreshToken;

public sealed class RefreshTokenCommandHandler(
    IUserRepository users,
    ISessionRepository sessions,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    IEffectivePermissionService effectivePermissionService,
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

        session.Refresh(
            newRefreshTokenHash,
            refreshTokenExpiresAt,
            command.IpAddress,
            command.UserAgent
        );

        sessions.Update(session);

        var accessToken = jwtTokenGenerator.Generate(
            user.Id,
            session.Id,
            user.UserType.ToString(),
            user.Email,
            user.UserName,
            permissions.Roles.ToList(),
            permissions.EffectiveScopes.ToList(),
            user.TokenVersion,
            accessTokenExpiresAt
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(
            new RefreshTokenResponse(
                accessToken,
                newRefreshToken,
                session.Id,
                accessTokenExpiresAt,
                refreshTokenExpiresAt
            )
        );
    }
}
