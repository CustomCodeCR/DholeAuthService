namespace Dhole.Auth.Infrastructure.Security;

public sealed class LoginRateLimitOptions
{
    public const string SectionName = "Security:LoginRateLimit";

    public int MaxFailedAttempts { get; init; } = 5;

    public int WindowMinutes { get; init; } = 15;

    public int BlockMinutes { get; init; } = 15;
}
