namespace Dhole.Auth.Persistence.Seed;

public sealed class SuperAdminSeedOptions
{
    public const string SectionName = "Seed:SuperAdmin";

    public string Email { get; init; } = default!;
    public string UserName { get; init; } = default!;
    public string DisplayName { get; init; } = default!;
    public string Password { get; init; } = default!;
}
