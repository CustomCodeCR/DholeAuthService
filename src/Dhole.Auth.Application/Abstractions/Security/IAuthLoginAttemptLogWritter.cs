namespace Dhole.Auth.Application.Abstractions.Mongo;

public interface IAuthLoginAttemptLogWriter
{
    Task WriteAsync(
        Guid? userId,
        string email,
        bool succeeded,
        string? failureReason,
        string? ipAddress,
        string? userAgent,
        DateTime occurredAtUtc,
        CancellationToken cancellationToken = default
    );
}
