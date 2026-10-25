using CustomCodeFramework.Postgres.DependencyInjection;
using CustomCodeFramework.Postgres.EntityFramework.DependencyInjection;
using Dhole.Auth.Application.Abstractions.Auditing;
using Dhole.Auth.Application.Abstractions.Messaging;
using Dhole.Auth.Application.Abstractions.Permissions;
using Dhole.Auth.Application.Abstractions.Repositories;
using Dhole.Auth.Persistence.Auditing;
using Dhole.Auth.Persistence.DbContexts;
using Dhole.Auth.Persistence.Messaging;
using Dhole.Auth.Persistence.Repositories;
using Dhole.Auth.Persistence.Seed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dhole.Auth.Persistence.DependencyInjection;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddCustomCodePostgres(configuration);
        services.AddCustomCodePostgresEntityFramework<ServiceDbContext>();

        services.Configure<SuperAdminSeedOptions>(
            configuration.GetSection(SuperAdminSeedOptions.SectionName)
        );

        services.AddScoped<DatabaseSeeder>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IScopeRepository, ScopeRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IAuthPermissionReadRepository, AuthPermissionReadRepository>();
        services.AddScoped<IIntegrationEventOutboxWriter, IntegrationEventOutboxWriter>();
        services.AddScoped<IAuthAuditService, AuthAuditService>();

        return services;
    }
}
