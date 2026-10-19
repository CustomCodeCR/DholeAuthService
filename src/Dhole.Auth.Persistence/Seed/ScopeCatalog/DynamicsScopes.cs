namespace Dhole.Auth.Persistence.Seed;

/// <summary>
/// Explicit, separately assignable Dynamics administration permissions.
/// DholeAuth stores these in auth.scopes and issues them in signed JWT "scopes".
/// </summary>
internal static class DynamicsScopes
{
    public const string Admin = "dynamics.admin";
    public const string SecurityAdmin = "dynamics.security.admin";

    public const string AdministratorRole = "Dynamics";
    public const string SecurityAdministratorRole = "Dynamics Seguridad";

    public static IReadOnlyCollection<ScopeSeedDefinition> All =>
    [
        ScopeCatalog.Create(
            Admin,
            "Dynamics 365 - Administrar conexiones",
            "Permite registrar, consultar, actualizar, probar y rotar las credenciales de conexión de Microsoft Dataverse."
        ),
        ScopeCatalog.Create(
            SecurityAdmin,
            "Dynamics 365 - Administrar seguridad",
            "Permite administrar las aplicaciones cliente, los grants y las políticas de seguridad de Dynamics."
        ),
    ];
}
