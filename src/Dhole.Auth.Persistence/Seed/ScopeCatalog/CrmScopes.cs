namespace Dhole.Auth.Persistence.Seed;

internal static class CrmScopes
{
    public static IReadOnlyCollection<ScopeSeedDefinition> All =>
    [
        ScopeCatalog.Create("crm.account.read", "crm.account.read", "Permiso específico de CRM: crm.account.read."),
        ScopeCatalog.Create("crm.account.read-all", "crm.account.read-all", "Permiso específico de CRM: crm.account.read-all."),
        ScopeCatalog.Create("crm.account.create", "crm.account.create", "Permiso específico de CRM: crm.account.create."),
        ScopeCatalog.Create("crm.account.update", "crm.account.update", "Permiso específico de CRM: crm.account.update."),
        ScopeCatalog.Create("crm.lead.read", "crm.lead.read", "Permiso específico de CRM: crm.lead.read."),
        ScopeCatalog.Create("crm.lead.read-all", "crm.lead.read-all", "Permiso específico de CRM: crm.lead.read-all."),
        ScopeCatalog.Create("crm.lead.create", "crm.lead.create", "Permiso específico de CRM: crm.lead.create."),
        ScopeCatalog.Create("crm.lead.update", "crm.lead.update", "Permiso específico de CRM: crm.lead.update."),
        ScopeCatalog.Create("crm.contact.read", "crm.contact.read", "Permiso específico de CRM: crm.contact.read."),
        ScopeCatalog.Create("crm.contact.create", "crm.contact.create", "Permiso específico de CRM: crm.contact.create."),
        ScopeCatalog.Create("crm.contact.update", "crm.contact.update", "Permiso específico de CRM: crm.contact.update."),
        ScopeCatalog.Create("crm.opportunity.read", "crm.opportunity.read", "Permiso específico de CRM: crm.opportunity.read."),
        ScopeCatalog.Create("crm.opportunity.read-all", "crm.opportunity.read-all", "Permiso específico de CRM: crm.opportunity.read-all."),
        ScopeCatalog.Create("crm.opportunity.create", "crm.opportunity.create", "Permiso específico de CRM: crm.opportunity.create."),
        ScopeCatalog.Create("crm.opportunity.update", "crm.opportunity.update", "Permiso específico de CRM: crm.opportunity.update."),
        ScopeCatalog.Create("crm.activity.read", "crm.activity.read", "Permiso específico de CRM: crm.activity.read."),
        ScopeCatalog.Create("crm.activity.create", "crm.activity.create", "Permiso específico de CRM: crm.activity.create."),
        ScopeCatalog.Create("crm.activity.update", "crm.activity.update", "Permiso específico de CRM: crm.activity.update."),
        ScopeCatalog.Create("crm.followup.manage", "crm.followup.manage", "Permiso específico de CRM: crm.followup.manage."),
        ScopeCatalog.Create("crm.task.manage", "crm.task.manage", "Permiso específico de CRM: crm.task.manage."),
        ScopeCatalog.Create("crm.analysis.read", "crm.analysis.read", "Permiso específico de CRM: crm.analysis.read."),
        ScopeCatalog.Create("crm.analysis.manage", "crm.analysis.manage", "Permiso específico de CRM: crm.analysis.manage."),
        ScopeCatalog.Create("crm.credit.read", "crm.credit.read", "Permiso específico de CRM: crm.credit.read."),
        ScopeCatalog.Create("crm.contract.read", "crm.contract.read", "Permiso específico de CRM: crm.contract.read."),
        ScopeCatalog.Create("crm.settings.manage", "crm.settings.manage", "Permiso específico de CRM: crm.settings.manage."),
    ];
}
