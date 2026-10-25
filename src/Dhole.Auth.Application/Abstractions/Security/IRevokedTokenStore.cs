namespace Dhole.Auth.Application.Abstractions.Security;

public interface IRevokedTokenStore
{
    Task RevokeSessionAsync(
        Guid sessionId,
        TimeSpan expiration,
        CancellationToken cancellationToken = default
    );
    Task<bool> IsSessionRevokedAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
