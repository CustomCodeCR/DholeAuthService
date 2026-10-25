namespace Dhole.Auth.Persistence.Seed;
internal static class CrmFollowupScopes
{
    public static IReadOnlyCollection<ScopeSeedDefinition> All =>
    [
        ScopeCatalog.Create("crm.followup.manage", "Gestionar seguimientos CRM", "Configurar políticas y ejecutar seguimientos autorizados, sin acceso global implícito."),
        ScopeCatalog.Create("crm.task.manage", "Gestionar tareas CRM", "Modificar tareas y recordatorios propios o autorizados, con trazabilidad."),
        ScopeCatalog.Create("crm.reminder.replay", "Reprocesar recordatorios CRM", "Reencolar solo entregas fallidas después de revisión administrativa.")
    ];
}
