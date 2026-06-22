using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Authentication;
using Dhole.Auth.Application.Abstractions.Permissions;
using Dhole.Auth.Application.Abstractions.Repositories;
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
            return Result.Failure<LoginResponse>(AuthErrors.InvalidCredentials);
        }

        if (!user.IsActive)
        {
            return Result.Failure<LoginResponse>(AuthErrors.UserInactive);
        }

        if (user.IsLocked)
        {
            return Result.Failure<LoginResponse>(AuthErrors.UserLocked);
        }

        var isPasswordValid = passwordHasher.Verify(command.Password, user.PasswordHash);

        if (!isPasswordValid)
        {
            return Result.Failure<LoginResponse>(AuthErrors.InvalidCredentials);
        }

        var permissions = await effectivePermissionService.GetAsync(user.Id, cancellationToken);

        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(30);
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

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
            permissions.Roles.ToList(),
            permissions.EffectiveScopes.ToList(),
            user.TokenVersion,
            accessTokenExpiresAt
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(
            new LoginResponse(
                accessToken,
                refreshToken,
                session.Id,
                accessTokenExpiresAt,
                refreshTokenExpiresAt
            )
        );
    }
}
