namespace Dhole.Auth.Application.Users;

public static class ProtectedSeedUserGuard
{
    public const string ProtectedEmail = "mlang@castrofallas.com";

    public static bool IsProtected(string? email)
    {
        return string.Equals(
            email?.Trim(),
            ProtectedEmail,
            StringComparison.OrdinalIgnoreCase
        );
    }
}
