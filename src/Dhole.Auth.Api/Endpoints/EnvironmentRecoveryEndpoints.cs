using System.Security.Claims;
using Dhole.Auth.Api.Authorization;
using Dhole.Auth.Domain.Shared;
using Dhole.Auth.Persistence.Seed;
using Microsoft.Extensions.Options;

namespace Dhole.Auth.Api.Endpoints;

public static class EnvironmentRecoveryEndpoints
{
    private const string ExpectedConfirmation = "REGENERAR DATOS ENV";

    public static IEndpointRouteBuilder MapEnvironmentRecoveryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/database-maintenance/reseed-environment", ReseedEnvironmentAsync)
            .WithTags("Database maintenance")
            .RequireAuthorization()
            .RequireScope(AuthConstants.Scopes.DatabaseMaintenanceManage);

        return app;
    }

    private static async Task<IResult> ReseedEnvironmentAsync(
        EnvironmentReseedRequest request,
        DatabaseSeeder databaseSeeder,
        IOptions<SuperAdminSeedOptions> superAdminOptions,
        IHostEnvironment hostEnvironment,
        HttpContext httpContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken
    )
    {
        if (!IsSystemSuperUser(httpContext.User))
        {
            return Results.Forbid();
        }

        if (!string.Equals(request.Confirmation?.Trim(), ExpectedConfirmation, StringComparison.Ordinal))
        {
            return Results.BadRequest(
                new
                {
                    message = "La frase de confirmación no coincide.",
                    expectedConfirmation = ExpectedConfirmation,
                }
            );
        }

        var superAdmin = superAdminOptions.Value;
        if (
            string.IsNullOrWhiteSpace(superAdmin.Email)
            || string.IsNullOrWhiteSpace(superAdmin.UserName)
            || string.IsNullOrWhiteSpace(superAdmin.Password)
        )
        {
            return Results.BadRequest(
                new
                {
                    message = "El ambiente actual no tiene configurados todos los datos requeridos de Seed:SuperAdmin en el .env.",
                }
            );
        }

        await databaseSeeder.SeedAsync(cancellationToken);

        var logger = loggerFactory.CreateLogger("Dhole.EnvironmentRecovery");
        logger.LogWarning(
            "SUPERUSER ENVIRONMENT RESEED executed. Environment={Environment} Actor={Actor}",
            hostEnvironment.EnvironmentName,
            ResolveActor(httpContext.User)
        );

        return Results.Ok(
            new
            {
                environment = hostEnvironment.EnvironmentName,
                restored = new[]
                {
                    "roles del sistema",
                    "permisos/scopes",
                    "asignación de scopes de SuperUsuario",
                    "SuperUsuario configurado en Seed:SuperAdmin",
                },
                secretValuesReturned = false,
                completedAtUtc = DateTimeOffset.UtcNow,
            }
        );
    }

    private static bool IsSystemSuperUser(ClaimsPrincipal user)
    {
        if (user.IsInRole(AuthConstants.SystemRoles.SuperUser))
        {
            return true;
        }

        return user.Claims
            .Where(
                claim =>
                    string.Equals(claim.Type, "role", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(claim.Type, "roles", StringComparison.OrdinalIgnoreCase)
                    || claim.Type.EndsWith("/role", StringComparison.OrdinalIgnoreCase)
            )
            .SelectMany(
                claim => claim.Value.Split(
                    [' ', ',', ';'],
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                )
            )
            .Any(role => string.Equals(role, AuthConstants.SystemRoles.SuperUser, StringComparison.OrdinalIgnoreCase));
    }

    private static string ResolveActor(ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub")
            ?? user.Identity?.Name
            ?? "unknown";
    }

    public sealed record EnvironmentReseedRequest(string? Confirmation);
}
