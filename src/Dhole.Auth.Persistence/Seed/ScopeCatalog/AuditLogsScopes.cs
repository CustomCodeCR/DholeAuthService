namespace Dhole.Auth.Persistence.Seed;

internal static class AuditLogsScopes
{
    public static IReadOnlyCollection<ScopeSeedDefinition> All =>
        [
            ScopeCatalog.Create(
                "auditlogs.events.view",
                "Ver auditoría",
                "Permite consultar eventos de auditoría del sistema."
            ),
            ScopeCatalog.Create(
                "auditlogs.events.export",
                "Exportar auditoría",
                "Permite exportar eventos de auditoría para revisión o cumplimiento."
            ),
            ScopeCatalog.Create(
                "auditlogs.entity-history.view",
                "Ver historial de entidad",
                "Permite consultar el historial de auditoría de una entidad específica."
            ),
            ScopeCatalog.Create(
                "auditlogs.user-history.view",
                "Ver historial de usuario",
                "Permite consultar el historial de auditoría de un usuario específico."
            ),
        ];
}
