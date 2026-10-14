namespace Dhole.Auth.Persistence.Seed;

// Fase 3: scopes for opportunities, activities, analyses and pipeline configuration.
// No implicit grants to Vendedor, Pricing, Finance or other roles.
internal static class CrmCommercialScopes
{
    public static IReadOnlyCollection<ScopeSeedDefinition> All =>
    [
        ScopeCatalog.Create("crm.opportunity.read", "Consultar oportunidades propias", "Consultar oportunidades propias; siempre aplicar autorización por registro."),
        ScopeCatalog.Create("crm.opportunity.read-all", "Consultar oportunidades de todo el equipo", "Consultar oportunidades de todo el equipo; siempre aplicar autorización por registro."),
        ScopeCatalog.Create("crm.opportunity.create", "Crear oportunidades", "Crear oportunidades; siempre aplicar autorización por registro."),
        ScopeCatalog.Create("crm.opportunity.update", "Modificar oportunidades autorizadas", "Modificar oportunidades autorizadas; siempre aplicar autorización por registro."),
        ScopeCatalog.Create("crm.opportunity.close", "Cerrar oportunidades ganadas o perdidas", "Cerrar oportunidades ganadas o perdidas; siempre aplicar autorización por registro."),
        ScopeCatalog.Create("crm.opportunity.assign", "Asignar ejecutivo de oportunidades", "Asignar ejecutivo de oportunidades; siempre aplicar autorización por registro."),
        ScopeCatalog.Create("crm.opportunity.export", "Exportar oportunidades autorizadas", "Exportar oportunidades autorizadas; siempre aplicar autorización por registro."),
        ScopeCatalog.Create("crm.activity.read", "Consultar actividades propias", "Consultar actividades propias; siempre aplicar autorización por registro."),
        ScopeCatalog.Create("crm.activity.read-all", "Consultar actividades del equipo", "Consultar actividades del equipo; siempre aplicar autorización por registro."),
        ScopeCatalog.Create("crm.activity.create", "Programar visitas, llamadas y actividades", "Programar visitas, llamadas y actividades; siempre aplicar autorización por registro."),
        ScopeCatalog.Create("crm.activity.update", "Completar, reprogramar o cancelar actividades", "Completar, reprogramar o cancelar actividades; siempre aplicar autorización por registro."),
        ScopeCatalog.Create("crm.activity.delete", "Archivar actividades sin borrar historial", "Archivar actividades sin borrar historial; siempre aplicar autorización por registro."),
        ScopeCatalog.Create("crm.analysis.read", "Consultar análisis comerciales autorizados", "Consultar análisis comerciales autorizados; siempre aplicar autorización por registro."),
        ScopeCatalog.Create("crm.analysis.manage", "Crear y modificar análisis y rutas", "Crear y modificar análisis y rutas; siempre aplicar autorización por registro."),
        ScopeCatalog.Create("crm.settings.manage", "Administrar embudos y sus etapas", "Administrar embudos y sus etapas; siempre aplicar autorización por registro.")
    ];
}
