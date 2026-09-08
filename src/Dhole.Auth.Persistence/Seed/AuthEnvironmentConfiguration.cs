using Microsoft.Extensions.Configuration;

namespace Dhole.Auth.Persistence.Seed;

public static class AuthEnvironmentConfiguration
{
    public static IReadOnlyDictionary<string, string?> BuildOverrides(IConfiguration configuration)
    {
        var overrides = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        Copy(configuration, overrides, "AUTH_SEED_EMAIL", "Seed:SuperAdmin:Email");
        Copy(configuration, overrides, "AUTH_SEED_USERNAME", "Seed:SuperAdmin:UserName");
        Copy(configuration, overrides, "AUTH_SEED_DISPLAY_NAME", "Seed:SuperAdmin:DisplayName");
        Copy(configuration, overrides, "AUTH_SEED_PASSWORD", "Seed:SuperAdmin:Password");

        return overrides;
    }

    private static void Copy(
        IConfiguration configuration,
        IDictionary<string, string?> destination,
        string sourceKey,
        string targetKey
    )
    {
        var value = configuration[sourceKey];
        if (!string.IsNullOrWhiteSpace(value))
        {
            destination[targetKey] = value.Trim();
        }
    }
}
