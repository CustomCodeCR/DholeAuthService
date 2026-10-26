namespace Dhole.Auth.Persistence.Seed;

internal static class CrmScopes
{
    public static IReadOnlyCollection<ScopeSeedDefinition> All =>
    [
        ScopeCatalog.Create("crm.account.read", "CRM · Clientes · Consultar asignados", "Permiso específico de CRM: crm.account.read."),
        ScopeCatalog.Create("crm.account.read-all", "CRM · Clientes · Consultar todos", "Permiso específico de CRM: crm.account.read-all."),
        ScopeCatalog.Create("crm.account.create", "CRM · Clientes · Crear", "Permiso específico de CRM: crm.account.create."),
        ScopeCatalog.Create("crm.account.update", "CRM · Clientes · Editar", "Permiso específico de CRM: crm.account.update."),
        ScopeCatalog.Create("crm.lead.read", "CRM · Prospectos · Consultar asignados", "Permiso específico de CRM: crm.lead.read."),
        ScopeCatalog.Create("crm.lead.read-all", "CRM · Prospectos · Consultar todos", "Permiso específico de CRM: crm.lead.read-all."),
        ScopeCatalog.Create("crm.lead.create", "CRM · Prospectos · Crear", "Permiso específico de CRM: crm.lead.create."),
        ScopeCatalog.Create("crm.lead.update", "CRM · Prospectos · Editar", "Permiso específico de CRM: crm.lead.update."),
        ScopeCatalog.Create("crm.contact.read", "CRM · Contactos · Consultar", "Permiso específico de CRM: crm.contact.read."),
        ScopeCatalog.Create("crm.contact.create", "CRM · Contactos · Crear", "Permiso específico de CRM: crm.contact.create."),
        ScopeCatalog.Create("crm.contact.update", "CRM · Contactos · Editar", "Permiso específico de CRM: crm.contact.update."),
        ScopeCatalog.Create("crm.opportunity.read", "CRM · Oportunidades · Consultar asignadas", "Permiso específico de CRM: crm.opportunity.read."),
        ScopeCatalog.Create("crm.opportunity.read-all", "CRM · Oportunidades · Consultar todas", "Permiso específico de CRM: crm.opportunity.read-all."),
        ScopeCatalog.Create("crm.opportunity.create", "CRM · Oportunidades · Crear", "Permiso específico de CRM: crm.opportunity.create."),
        ScopeCatalog.Create("crm.opportunity.update", "CRM · Oportunidades · Editar", "Permiso específico de CRM: crm.opportunity.update."),
        ScopeCatalog.Create("crm.activity.read", "CRM · Actividades · Consultar", "Permiso específico de CRM: crm.activity.read."),
        ScopeCatalog.Create("crm.activity.create", "CRM · Actividades · Crear", "Permiso específico de CRM: crm.activity.create."),
        ScopeCatalog.Create("crm.activity.update", "CRM · Actividades · Editar", "Permiso específico de CRM: crm.activity.update."),
        ScopeCatalog.Create("crm.followup.manage", "CRM · Seguimientos · Administrar", "Permiso específico de CRM: crm.followup.manage."),
        ScopeCatalog.Create("crm.task.manage", "CRM · Tareas · Administrar", "Permiso específico de CRM: crm.task.manage."),
        ScopeCatalog.Create("crm.analysis.read", "CRM · Analítica · Consultar", "Permiso específico de CRM: crm.analysis.read."),
        ScopeCatalog.Create("crm.analysis.manage", "CRM · Analítica · Administrar", "Permiso específico de CRM: crm.analysis.manage."),
        ScopeCatalog.Create("crm.credit.read", "CRM · Crédito · Consultar", "Permiso específico de CRM: crm.credit.read."),
        ScopeCatalog.Create("crm.contract.read", "CRM · Contratos · Consultar", "Permiso específico de CRM: crm.contract.read."),
        ScopeCatalog.Create("crm.settings.manage", "CRM · Configuración · Administrar", "Permiso específico de CRM: crm.settings.manage."),
    ];
}
