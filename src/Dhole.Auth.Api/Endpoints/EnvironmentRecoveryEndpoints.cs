using System.Security.Claims;
using Dhole.Auth.Api.Authorization;
using Dhole.Auth.Domain.Shared;
using Dhole.Auth.Persistence.Seed;
using Microsoft.Extensions.Options;

namespace Dhole.Auth.Api.Endpoints;

public static class EnvironmentRecoveryEndpoints
{
    private const string ExpectedConfirmation = "REGENERAR DATOS ENV";
    private const string ServiceKeyHeader = "X-Internal-Service-Key";

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
        IConfiguration configuration,
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
                    message = "El ambiente actual no tiene configurados todos los datos requeridos de AUTH_SEED_* en su archivo .env.",
                }
            );
        }

        await databaseSeeder.SeedAsync(cancellationToken);

        var restored = new List<string>
        {
            "Auth: roles del sistema",
            "Auth: permisos/scopes",
            "Auth: asignación de scopes de SuperUsuario",
            "Auth: SuperUsuario configurado por AUTH_SEED_*",
        };

        var dataExtractionResult = await ReseedInternalServiceAsync(
            configuration,
            "DATAEXTRACTION_HTTP_URL",
            "DataExtraction:HttpUrl",
            "/api/internal/data-extraction/environment-reseed",
            "DataExtraction",
            cancellationToken
        );
        if (!dataExtractionResult.Success)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status502BadGateway,
                title: "Auth fue regenerado, pero DataExtraction no pudo regenerar sus datos del ambiente.",
                detail: dataExtractionResult.Error
            );
        }

        restored.Add("DataExtraction: cuenta de correo configurada desde el env del ambiente");

        var storageResult = await ReseedInternalServiceAsync(
            configuration,
            "STORAGE_HTTP_URL",
            "Storage:HttpUrl",
            "/api/internal/storage/environment-reseed",
            "Storage",
            cancellationToken
        );
        if (!storageResult.Success)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status502BadGateway,
                title: "Auth y DataExtraction fueron regenerados, pero Storage no pudo regenerar sus datos del ambiente.",
                detail: storageResult.Error
            );
        }

        restored.Add("Storage: proveedor MinIO predeterminado configurado desde el env del ambiente");

        var logger = loggerFactory.CreateLogger("Dhole.EnvironmentRecovery");
        logger.LogWarning(
            "SUPERUSER ENVIRONMENT RESEED executed. Environment={Environment} Actor={Actor} DataExtractionRestored={DataExtractionRestored} StorageRestored={StorageRestored}",
            hostEnvironment.EnvironmentName,
            ResolveActor(httpContext.User),
            dataExtractionResult.Success,
            storageResult.Success
        );

        return Results.Ok(
            new
            {
                environment = hostEnvironment.EnvironmentName,
                restored = restored.ToArray(),
                secretValuesReturned = false,
                completedAtUtc = DateTimeOffset.UtcNow,
            }
        );
    }

    private static async Task<InternalServiceReseedResult> ReseedInternalServiceAsync(
        IConfiguration configuration,
        string primaryUrlKey,
        string fallbackUrlKey,
        string endpointPath,
        string serviceName,
        CancellationToken cancellationToken
    )
    {
        var baseUrl = configuration[primaryUrlKey] ?? configuration[fallbackUrlKey];
        var serviceKey = configuration["INTERNAL_SERVICE_KEY"]
            ?? configuration["InternalServices:ServiceKey"];

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return new InternalServiceReseedResult(
                false,
                $"Falta {primaryUrlKey} en el ambiente actual."
            );
        }

        if (string.IsNullOrWhiteSpace(serviceKey))
        {
            return new InternalServiceReseedResult(
                false,
                "Falta INTERNAL_SERVICE_KEY en el ambiente actual."
            );
        }

        var endpoint = $"{baseUrl.TrimEnd('/')}{endpointPath}";

        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            using var message = new HttpRequestMessage(HttpMethod.Post, endpoint);
            message.Headers.TryAddWithoutValidation(ServiceKeyHeader, serviceKey);
            using var response = await client.SendAsync(message, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new InternalServiceReseedResult(
                    false,
                    $"{serviceName} respondió HTTP {(int)response.StatusCode}."
                );
            }

            return new InternalServiceReseedResult(true, null);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return new InternalServiceReseedResult(
                false,
                $"No se pudo contactar {serviceName}: {ex.Message}"
            );
        }
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
    private sealed record InternalServiceReseedResult(bool Success, string? Error);
}
