using CustomCodeFramework.Mongo.Abstractions;
using CustomCodeFramework.Mongo.Collections;

namespace Dhole.Auth.Infrastructure.Mongo.Documents;

[MongoCollectionName("auth_user_security_snapshots")]
public sealed class AuthUserSecuritySnapshotDocument : IReadModel
{
    public string Id { get; init; } = Guid.NewGuid().ToString();

    public string UserId { get; init; } = default!;

    public string Email { get; init; } = default!;

    public string UserName { get; init; } = default!;

    public IReadOnlyCollection<string> Roles { get; init; } = [];

    public IReadOnlyCollection<string> Scopes { get; init; } = [];

    public bool IsActive { get; init; }

    public bool IsLocked { get; init; }

    public DateTime OccurredAtUtc { get; init; }
}
