namespace Dhole.Auth.Persistence.Seed;

/// <summary>
/// Explicit per-user scopes for the standalone Dynamics administration API.
/// No users or roles receive these scopes implicitly.
/// </summary>
internal static class DynamicsScopes
{
    public static IReadOnlyCollection<ScopeSeedDefinition> All =>
        [
            ScopeCatalog.Create(
                "dynamics.admin",
                "Administrar conexiones Dynamics",
                "Permite ver, registrar, rotar y verificar conexiones de Microsoft Dataverse."
            ),
            ScopeCatalog.Create(
                "dynamics.security.admin",
                "Administrar seguridad Dynamics",
                "Permite gestionar grants, clientes, y políticas privilegiadas del conector Dynamics."
            )
        ];
}
