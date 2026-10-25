namespace Dhole.Auth.Application.Abstractions.Security;

public interface ILoginRateLimiter
{
    Task<bool> IsBlockedAsync(
        string email,
        string? ipAddress,
        CancellationToken cancellationToken = default
    );

    Task RegisterFailedAttemptAsync(
        string email,
        string? ipAddress,
        CancellationToken cancellationToken = default
    );

    Task ResetAsync(string email, string? ipAddress, CancellationToken cancellationToken = default);
}
