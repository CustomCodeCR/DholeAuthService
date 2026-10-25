using System.Security.Cryptography;
using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Auth.Application.Abstractions.Auditing;
using Dhole.Auth.Application.Abstractions.Authentication;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Application.Auditing;
using Dhole.Auth.Application.Users;
using Dhole.Auth.Contracts.Users;
using Dhole.Auth.Domain.Shared;

namespace Dhole.Auth.Application.Users.IssueUserCredentials;

public sealed class IssueUserCredentialsCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IAuthAuditService audit,
    IUnitOfWork unitOfWork
) : ICommandHandler<IssueUserCredentialsCommand, Result<IssuedUserCredentialsDto>>
{
    private const int TemporaryPasswordLength = 16;
    private const string Uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Lowercase = "abcdefghijkmnopqrstuvwxyz";
    private const string Digits = "23456789";
    private const string Symbols = "!@#$%*-_";

    public async Task<Result<IssuedUserCredentialsDto>> HandleAsync(
        IssueUserCredentialsCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var user = await users.GetWithRolesAndScopesAsync(command.UserId, cancellationToken);

        if (user is null)
            return Result.Failure<IssuedUserCredentialsDto>(AuthErrors.UserNotFound);

        if (ProtectedSeedUserGuard.IsProtected(user.Email))
            return Result.Failure<IssuedUserCredentialsDto>(AuthErrors.ProtectedSeedUser);

        var before = UserAuditSnapshot.From(user);
        var temporaryPassword = GenerateTemporaryPassword();

        user.ChangePassword(
            passwordHasher.Hash(temporaryPassword),
            command.IssuedBy,
            mustChangePassword: true
        );

        var after = UserAuditSnapshot.From(user);
        users.Update(user);

        await audit.PublishAsync(
            new AuthAuditEvent(
                EventType: AuthAuditEventTypes.UserPasswordChanged,
                Action: AuthAuditActions.PasswordChanged,
                EntityType: AuthAuditEntityTypes.User,
                EntityId: user.Id,
                ActorUserId: command.IssuedBy,
                Before: before,
                After: after,
                Payload: new
                {
                    targetUserId = user.Id,
                    targetUserName = user.UserName,
                    targetUserEmail = user.Email,
                    credentialsIssued = true,
                    tokenVersionBefore = before.TokenVersion,
                    tokenVersionAfter = after.TokenVersion,
                },
                Metadata: new
                {
                    passwordIncluded = false,
                    passwordHashIncluded = false,
                    temporaryPasswordReturnedOnce = true,
                    tokensInvalidated = after.TokenVersion != before.TokenVersion,
                }
            ),
            cancellationToken
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(
            new IssuedUserCredentialsDto(
                user.Id,
                user.UserName,
                user.Email,
                user.DisplayName,
                temporaryPassword
            )
        );
    }

    private static string GenerateTemporaryPassword()
    {
        var password = new char[TemporaryPasswordLength];
        password[0] = RandomCharacter(Uppercase);
        password[1] = RandomCharacter(Lowercase);
        password[2] = RandomCharacter(Digits);
        password[3] = RandomCharacter(Symbols);

        var allCharacters = Uppercase + Lowercase + Digits + Symbols;
        for (var index = 4; index < password.Length; index++)
            password[index] = RandomCharacter(allCharacters);

        for (var index = password.Length - 1; index > 0; index--)
        {
            var swapIndex = RandomNumberGenerator.GetInt32(index + 1);
            (password[index], password[swapIndex]) = (password[swapIndex], password[index]);
        }

        return new string(password);
    }

    private static char RandomCharacter(string characters)
        => characters[RandomNumberGenerator.GetInt32(characters.Length)];
}
