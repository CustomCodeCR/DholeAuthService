namespace Dhole.Auth.Persistence.Seed;

internal static class MonitoringScopes
{
    public static IReadOnlyCollection<ScopeSeedDefinition> All =>
        [
            ScopeCatalog.Create(
                "monitoring.services.view",
                "Ver monitoreo de servicios",
                "Permite consultar el estado, latencia y disponibilidad de los servicios Dhole."
            ),
        ];
}
