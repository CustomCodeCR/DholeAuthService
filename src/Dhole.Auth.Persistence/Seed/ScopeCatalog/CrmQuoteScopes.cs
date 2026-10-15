namespace Dhole.Auth.Persistence.Seed;

// Phase 4: Pricing links remain additional to Pricing's own existing scopes.
internal static class CrmQuoteScopes
{
    public static IReadOnlyCollection<ScopeSeedDefinition> All =>
    [
        ScopeCatalog.Create("crm.quote.read", "Consultar vínculos comerciales CRM con Pricing",
            "Leer únicamente cotizaciones y solicitudes ligadas a oportunidades autorizadas."),
        ScopeCatalog.Create("crm.quote.link", "Vincular solicitudes y tarifas reales a oportunidades",
            "Crear correlaciones de Pricing, validar vendedor y refrescar snapshots comerciales.")
    ];
}
