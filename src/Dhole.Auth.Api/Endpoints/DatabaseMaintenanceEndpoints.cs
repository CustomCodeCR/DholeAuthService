using System.Security.Claims;
using Dhole.Auth.Api.Authorization;
using Dhole.Auth.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Dhole.Auth.Persistence.DbContexts;

namespace Dhole.Auth.Api.Endpoints;

public static class DatabaseMaintenanceEndpoints
{
    private const string ProtectedMigrationTable = "__EFMigrationsHistory";

    public static IEndpointRouteBuilder MapDatabaseMaintenanceEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/auth/database-maintenance/catalog", GetCatalogAsync)
            .WithTags("Database maintenance")
            .RequireAuthorization()
            .RequireScope(AuthConstants.Scopes.DatabaseMaintenanceManage);

        app.MapPost("/api/auth/database-maintenance/truncate", TruncateAsync)
            .WithTags("Database maintenance")
            .RequireAuthorization()
            .RequireScope(AuthConstants.Scopes.DatabaseMaintenanceManage);

        return app;
    }

    private static async Task<IResult> GetCatalogAsync(
        ServiceDbContext dbContext,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        if (!IsSystemSuperUser(httpContext.User))
        {
            return Results.Forbid();
        }

        var allowedDatabases = ResolveAllowedDatabases(configuration, dbContext);
        var databases = new List<object>(allowedDatabases.Count);

        foreach (var database in allowedDatabases)
        {
            try
            {
                await using var connection = CreateConnection(configuration, dbContext, database);
                await connection.OpenAsync(cancellationToken);

                var tables = await LoadTablesAsync(connection, cancellationToken);
                databases.Add(
                    new
                    {
                        name = database,
                        available = true,
                        tables = tables.Select(
                            table => new
                            {
                                table.schema,
                                table.name,
                                key = $"{table.schema}.{table.name}",
                                isProtected = table.IsProtected,
                            }
                        ),
                    }
                );
            }
            catch (Exception ex) when (ex is NpgsqlException or InvalidOperationException)
            {
                databases.Add(
                    new
                    {
                        name = database,
                        available = false,
                        tables = Array.Empty<object>(),
                    }
                );
            }
        }

        return Results.Ok(
            new
            {
                environment = hostEnvironment.EnvironmentName,
                databases,
                safeguards = new
                {
                    superUserOnly = true,
                    migrationHistoryProtected = true,
                    databaseOperationKeepsSchema = true,
                },
            }
        );
    }

    private static async Task<IResult> TruncateAsync(
        DatabaseTruncateRequest request,
        ServiceDbContext dbContext,
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

        var database = request.Database?.Trim() ?? string.Empty;
        var allowedDatabases = ResolveAllowedDatabases(configuration, dbContext);
        if (!allowedDatabases.Contains(database, StringComparer.OrdinalIgnoreCase))
        {
            return Results.BadRequest(new { message = "La base de datos seleccionada no está permitida." });
        }

        var mode = request.Mode?.Trim().ToLowerInvariant();
        if (mode is not ("table" or "database"))
        {
            return Results.BadRequest(new { message = "La operación debe ser 'table' o 'database'." });
        }

        await using var connection = CreateConnection(configuration, dbContext, database);
        await connection.OpenAsync(cancellationToken);

        var availableTables = await LoadTablesAsync(connection, cancellationToken);
        var targets = new List<DatabaseTable>();
        string expectedConfirmation;

        if (mode == "database")
        {
            targets.AddRange(availableTables.Where(x => !x.IsProtected));
            expectedConfirmation = $"TRUNCAR BASE {database}";

            if (targets.Count == 0)
            {
                return Results.BadRequest(new { message = "La base no contiene tablas truncables." });
            }
        }
        else
        {
            var tableKey = request.Table?.Trim() ?? string.Empty;
            var selected = availableTables.FirstOrDefault(
                x => string.Equals(
                    $"{x.Schema}.{x.Name}",
                    tableKey,
                    StringComparison.OrdinalIgnoreCase
                )
            );

            if (selected is null)
            {
                return Results.BadRequest(new { message = "La tabla seleccionada no existe en la base indicada." });
            }

            if (selected.IsProtected)
            {
                return Results.BadRequest(
                    new { message = "La tabla de historial de migraciones está protegida y no puede truncarse desde Dhole." }
                );
            }

            targets.Add(selected);
            expectedConfirmation = $"TRUNCAR TABLA {database}.{selected.Schema}.{selected.Name}";
        }

        if (!string.Equals(request.Confirmation?.Trim(), expectedConfirmation, StringComparison.Ordinal))
        {
            return Results.BadRequest(
                new
                {
                    message = "La frase de confirmación no coincide.",
                    expectedConfirmation,
                }
            );
        }

        var quotedTargets = targets
            .Select(x => $"{QuoteIdentifier(x.Schema)}.{QuoteIdentifier(x.Name)}")
            .ToArray();

        var sql = $"TRUNCATE TABLE {string.Join(", ", quotedTargets)} RESTART IDENTITY";
        if (mode == "database" || request.Cascade)
        {
            sql += " CASCADE";
        }
        sql += ";";

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection, transaction)
        {
            CommandTimeout = 120,
        };

        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (PostgresException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Results.BadRequest(
                new
                {
                    message = ex.SqlState == PostgresErrorCodes.FeatureNotSupported
                        ? "PostgreSQL rechazó la operación por una dependencia. Active CASCADE únicamente si desea vaciar también tablas relacionadas."
                        : "PostgreSQL rechazó la operación de truncado.",
                    detail = ex.MessageText,
                }
            );
        }

        var logger = loggerFactory.CreateLogger("Dhole.DatabaseMaintenance");
        var actor = ResolveActor(httpContext.User);
        logger.LogWarning(
            "SUPERUSER DATABASE TRUNCATE executed. Environment={Environment} Database={Database} Mode={Mode} Table={Table} Cascade={Cascade} Actor={Actor} Targets={TargetCount}",
            hostEnvironment.EnvironmentName,
            database,
            mode,
            request.Table,
            mode == "database" || request.Cascade,
            actor,
            targets.Count
        );

        return Results.Ok(
            new
            {
                environment = hostEnvironment.EnvironmentName,
                database,
                mode,
                table = mode == "table" ? $"{targets[0].Schema}.{targets[0].Name}" : null,
                cascade = mode == "database" || request.Cascade,
                tablesTruncated = targets.Count,
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

    private static List<string> ResolveAllowedDatabases(
        IConfiguration configuration,
        ServiceDbContext dbContext
    )
    {
        var databases = configuration
            .AsEnumerable()
            .Where(
                item =>
                    item.Key.EndsWith("_DB", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(item.Key, "POSTGRES_DB", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(item.Key, "MONGO_DB", StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(item.Value)
            )
            .Select(item => item.Value!.Trim().Trim('"', '\''))
            .Where(IsSafeDatabaseName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var currentDatabase = dbContext.Database.GetDbConnection().Database;
        if (IsSafeDatabaseName(currentDatabase)
            && !databases.Contains(currentDatabase, StringComparer.OrdinalIgnoreCase))
        {
            databases.Add(currentDatabase);
        }

        databases.Sort(StringComparer.OrdinalIgnoreCase);
        return databases;
    }

    private static bool IsSafeDatabaseName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 63)
        {
            return false;
        }

        return value.All(ch => char.IsLetterOrDigit(ch) || ch is '_' or '-');
    }

    private static NpgsqlConnection CreateConnection(
        IConfiguration configuration,
        ServiceDbContext dbContext,
        string database
    )
    {
        var baseConnectionString = configuration.GetConnectionString("Postgres")
            ?? configuration["Postgres:ConnectionString"]
            ?? dbContext.Database.GetDbConnection().ConnectionString;

        if (string.IsNullOrWhiteSpace(baseConnectionString))
        {
            throw new InvalidOperationException("PostgreSQL connection string is not configured.");
        }

        var builder = new NpgsqlConnectionStringBuilder(baseConnectionString)
        {
            Database = database,
            ApplicationName = "DholeAuthService.DatabaseMaintenance",
        };

        return new NpgsqlConnection(builder.ConnectionString);
    }

    private static async Task<List<DatabaseTable>> LoadTablesAsync(
        NpgsqlConnection connection,
        CancellationToken cancellationToken
    )
    {
        const string sql = """
            SELECT schemaname, tablename
            FROM pg_catalog.pg_tables
            WHERE schemaname NOT IN ('pg_catalog', 'information_schema')
              AND schemaname NOT LIKE 'pg_toast%'
            ORDER BY schemaname, tablename;
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var tables = new List<DatabaseTable>();

        while (await reader.ReadAsync(cancellationToken))
        {
            var schema = reader.GetString(0);
            var name = reader.GetString(1);
            tables.Add(
                new DatabaseTable(
                    schema,
                    name,
                    string.Equals(name, ProtectedMigrationTable, StringComparison.OrdinalIgnoreCase)
                )
            );
        }

        return tables;
    }

    private static string QuoteIdentifier(string value) => $"\"{value.Replace("\"", "\"\"")}\"";

    private sealed record DatabaseTable(string Schema, string Name, bool IsProtected);

    public sealed record DatabaseTruncateRequest(
        string? Database,
        string? Mode,
        string? Table,
        bool Cascade,
        string? Confirmation
    );
}
