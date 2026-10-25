namespace Dhole.Auth.Persistence.Seed;
internal static class CrmFinanceScopes
{
    public static IReadOnlyCollection<ScopeSeedDefinition> All =>
    [
        ScopeCatalog.Create("crm.credit.read", "Consultar condiciones financieras CRM", "Leer perfiles, coberturas e historiales financieros autorizados."),
        ScopeCatalog.Create("crm.credit.manage", "Administrar propuestas financieras CRM", "Crear y cambiar propuestas; no otorga aprobación."),
        ScopeCatalog.Create("crm.credit.approve", "Aprobar crédito CRM", "Aprobar o rechazar propuestas con autorización financiera independiente."),
        ScopeCatalog.Create("crm.contract.read", "Consultar contratos CRM", "Leer contratos permitidos por alcance."),
        ScopeCatalog.Create("crm.contract.manage", "Administrar contratos CRM", "Crear, cambiar y renovar contratos con historial.")
    ];
}
