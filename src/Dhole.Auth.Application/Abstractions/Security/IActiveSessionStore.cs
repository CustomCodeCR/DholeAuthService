namespace Dhole.Auth.Application.Abstractions.Security;

public interface IActiveSessionStore
{
    Task SetActiveAsync(
        Guid sessionId,
        Guid userId,
        TimeSpan expiration,
        CancellationToken cancellationToken = default
    );
    Task RemoveAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<bool> IsActiveAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
