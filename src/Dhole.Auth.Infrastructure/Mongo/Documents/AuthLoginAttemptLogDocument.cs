using CustomCodeFramework.Mongo.Abstractions;
using CustomCodeFramework.Mongo.Collections;

namespace Dhole.Auth.Infrastructure.Mongo.Documents;

[MongoCollectionName("auth_login_attempt_logs")]
public sealed class AuthLoginAttemptLogDocument : IReadModel
{
    public string Id { get; init; } = Guid.NewGuid().ToString();

    public string? UserId { get; init; }

    public string Email { get; init; } = default!;

    public bool Succeeded { get; init; }

    public string? FailureReason { get; init; }

    public string? IpAddress { get; init; }

    public string? UserAgent { get; init; }

    public DateTime OccurredAtUtc { get; init; }
}
