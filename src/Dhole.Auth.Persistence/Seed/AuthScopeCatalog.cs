namespace Dhole.Auth.Persistence.Seed;

internal static class AuthScopeCatalog
{
    public static IReadOnlyCollection<ScopeSeedDefinition> Scopes =>
        [
            .. AuthScopes.All,
            .. ConfigScopes.All,
            .. AuditLogsScopes.All,
            .. ScrapingScopes.All,
            .. PricingScopes.All,
            .. AiScopes.All,
            .. StorageScopes.All,
            .. MonitoringScopes.All,
            .. ReportsScopes.All,
        ];
}
