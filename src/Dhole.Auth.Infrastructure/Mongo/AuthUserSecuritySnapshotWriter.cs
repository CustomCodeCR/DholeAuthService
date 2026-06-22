using CustomCodeFramework.Mongo.Abstractions;
using Dhole.Auth.Application.Abstractions.Mongo;
using Dhole.Auth.Infrastructure.Mongo.Documents;

namespace Dhole.Auth.Infrastructure.Mongo;

public sealed class AuthUserSecuritySnapshotWriter(IMongoContext mongoContext)
    : IAuthUserSecuritySnapshotWriter
{
    public Task WriteAsync(
        Guid userId,
        string email,
        string userName,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> scopes,
        bool isActive,
        bool isLocked,
        DateTime occurredAtUtc,
        CancellationToken cancellationToken = default
    )
    {
        var document = new AuthUserSecuritySnapshotDocument
        {
            UserId = userId.ToString(),
            Email = email,
            UserName = userName,
            Roles = roles,
            Scopes = scopes,
            IsActive = isActive,
            IsLocked = isLocked,
            OccurredAtUtc = occurredAtUtc,
        };

        return mongoContext
            .GetCollection<AuthUserSecuritySnapshotDocument>()
            .InsertOneAsync(document, cancellationToken: cancellationToken);
    }
}
