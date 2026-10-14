namespace Dhole.Auth.Persistence.Seed;

// Fase 2: no roles receive these permissions automatically.
internal static class CrmScopes
{
    public static IReadOnlyCollection<ScopeSeedDefinition> All =>
    [
        ScopeCatalog.Create("crm.account.read", "CRM account read", "Permiso de read para account en CRM."),
        ScopeCatalog.Create("crm.account.read-all", "CRM account read-all", "Permiso de read-all para account en CRM."),
        ScopeCatalog.Create("crm.account.create", "CRM account create", "Permiso de create para account en CRM."),
        ScopeCatalog.Create("crm.account.update", "CRM account update", "Permiso de update para account en CRM."),
        ScopeCatalog.Create("crm.account.archive", "CRM account archive", "Permiso de archive para account en CRM."),
        ScopeCatalog.Create("crm.account.assign", "CRM account assign", "Permiso de assign para account en CRM."),
        ScopeCatalog.Create("crm.account.export", "CRM account export", "Permiso de export para account en CRM."),
        ScopeCatalog.Create("crm.lead.read", "CRM lead read", "Permiso de read para lead en CRM."),
        ScopeCatalog.Create("crm.lead.read-all", "CRM lead read-all", "Permiso de read-all para lead en CRM."),
        ScopeCatalog.Create("crm.lead.create", "CRM lead create", "Permiso de create para lead en CRM."),
        ScopeCatalog.Create("crm.lead.update", "CRM lead update", "Permiso de update para lead en CRM."),
        ScopeCatalog.Create("crm.lead.convert", "CRM lead convert", "Permiso de convert para lead en CRM."),
        ScopeCatalog.Create("crm.lead.assign", "CRM lead assign", "Permiso de assign para lead en CRM."),
        ScopeCatalog.Create("crm.lead.archive", "CRM lead archive", "Permiso de archive para lead en CRM."),
        ScopeCatalog.Create("crm.contact.read", "CRM contact read", "Permiso de read para contact en CRM."),
        ScopeCatalog.Create("crm.contact.create", "CRM contact create", "Permiso de create para contact en CRM."),
        ScopeCatalog.Create("crm.contact.update", "CRM contact update", "Permiso de update para contact en CRM."),
        ScopeCatalog.Create("crm.contact.archive", "CRM contact archive", "Permiso de archive para contact en CRM."),
        ScopeCatalog.Create("crm.contact.marketing-consent.manage", "CRM contact marketing-consent.manage", "Permiso de marketing-consent.manage para contact en CRM.")
    ];
}
