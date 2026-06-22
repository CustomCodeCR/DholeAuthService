using CustomCodeFramework.Cqrs.DependencyInjection;
using CustomCodeFramework.Validation.DependencyInjection;
using Dhole.Auth.Application.Abstractions.Permissions;
using Dhole.Auth.Application.Permissions;
using Microsoft.Extensions.DependencyInjection;

namespace Dhole.Auth.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddCustomCodeValidation(AssemblyReference.Assembly);

        services.AddCustomCodeCqrs(AssemblyReference.Assembly);
        services.AddCustomCodeCqrsBehaviors();

        services.AddScoped<IEffectivePermissionService, EffectivePermissionService>();

        return services;
    }
}
