namespace Dhole.Auth.Persistence.Seed;

/// <summary>Administra las APIs propias de Dhole publicadas para integraciones de terceros.</summary>
internal static class ThirdPartyScopes
{
    public const string Manage = "integrations.thirdparty.manage";

    public static IReadOnlyCollection<ScopeSeedDefinition> All =>
    [
        ScopeCatalog.Create(
            Manage,
            "Integraciones · APIs para terceros · Administrar",
            "Permite publicar o deshabilitar contratos API, seleccionar rutas internas autorizadas y generar, limitar o revocar API keys para sistemas externos.")
    ];
}
