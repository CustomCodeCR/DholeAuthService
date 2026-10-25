using CustomCodeFramework.Mongo.Abstractions;
using Dhole.Auth.Application.Abstractions.Mongo;
using Dhole.Auth.Infrastructure.Mongo.Documents;

namespace Dhole.Auth.Infrastructure.Mongo;

public sealed class AuthLoginAttemptLogWriter(IMongoContext mongoContext)
    : IAuthLoginAttemptLogWriter
{
    public Task WriteAsync(
        Guid? userId,
        string email,
        bool succeeded,
        string? failureReason,
        string? ipAddress,
        string? userAgent,
        DateTime occurredAtUtc,
        CancellationToken cancellationToken = default
    )
    {
        var document = new AuthLoginAttemptLogDocument
        {
            UserId = userId?.ToString(),
            Email = email,
            Succeeded = succeeded,
            FailureReason = failureReason,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            OccurredAtUtc = occurredAtUtc,
        };

        return mongoContext
            .GetCollection<AuthLoginAttemptLogDocument>()
            .InsertOneAsync(document, cancellationToken: cancellationToken);
    }
}
