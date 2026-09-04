namespace Dhole.Auth.Persistence.Seed;

internal static class ScopeCatalog
{
    public static ScopeSeedDefinition Create(string code, string name, string? description)
    {
        var normalizedCode = code.Trim().ToLowerInvariant();
        var normalizedName = name.Trim();
        var baseDescription = string.IsNullOrWhiteSpace(description)
            ? $"Permite ejecutar la operación {normalizedName.ToLowerInvariant()} en Dhole."
            : description.Trim();

        var clarifiedDescription =
            $"{baseDescription} Este scope autoriza únicamente la acción asociada al código «{normalizedCode}»; no concede permisos de otras operaciones.";

        return new ScopeSeedDefinition(normalizedCode, normalizedName, clarifiedDescription);
    }
}
