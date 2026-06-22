namespace Dhole.Auth.Persistence.Seed;

internal static class ScopeCatalog
{
    public static ScopeSeedDefinition Create(string code, string name, string? description)
    {
        return new ScopeSeedDefinition(code, name, description);
    }
}
