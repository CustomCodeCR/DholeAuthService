namespace Dhole.Auth.Application.Abstractions.Mongo;

public interface IAuthUserSecuritySnapshotWriter
{
    Task WriteAsync(
        Guid userId,
        string email,
        string userName,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> scopes,
        bool isActive,
        bool isLocked,
        DateTime occurredAtUtc,
        CancellationToken cancellationToken = default
    );
}
