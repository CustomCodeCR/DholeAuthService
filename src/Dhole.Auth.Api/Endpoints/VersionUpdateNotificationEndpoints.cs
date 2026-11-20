using System.Security.Claims;
using System.Text.Json;
using CustomCodeFramework.Messaging.Outbox;
using Dhole.Auth.Api.Extensions;
using Dhole.Auth.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Auth.Api.Endpoints;

/// <summary>Preferences for the Web's new-version dialog, not operational notifications.</summary>
public static class VersionUpdateNotificationEndpoints
{
    private const string GlobalKey = "global";
    private const string EventName = "auth.version_update_notifications.changed";

    public static IEndpointRouteBuilder MapVersionUpdateNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth/version-update-notifications")
            .WithTags("Version update notifications").RequireAuthorization();

        group.MapGet("/", async (HttpContext http, ServiceDbContext db, CancellationToken ct) =>
        {
            var userId = http.GetCurrentUserId();
            if (userId is null) return Results.Unauthorized();
            return Results.Ok(await GetEffectiveAsync(db, userId.Value, ct));
        });

        group.MapPut("/me", async (UpdatePreferenceRequest request, HttpContext http,
            ServiceDbContext db, ILoggerFactory loggerFactory, CancellationToken ct) =>
        {
            var actor = http.GetCurrentUserId();
            if (actor is null) return Results.Unauthorized();
            await SetAsync(db, UserKey(actor.Value), request.Enabled, actor.Value, ct);
            loggerFactory.CreateLogger("Dhole.VersionUpdateSettings").LogInformation(
                "Version update preference changed by {Actor} for self; enabled={Enabled}", actor, request.Enabled);
            return Results.Ok(await GetEffectiveAsync(db, actor.Value, ct));
        });

        group.MapGet("/users/{userId:guid}", async (Guid userId, HttpContext http,
            ServiceDbContext db, CancellationToken ct) =>
        {
            if (!IsSuperUser(http.User)) return Results.Forbid();
            if (!await db.Users.AnyAsync(u => u.Id == userId, ct)) return Results.NotFound();
            return Results.Ok(await GetEffectiveAsync(db, userId, ct));
        });

        group.MapPut("/users/{userId:guid}", async (Guid userId, UpdatePreferenceRequest request,
            HttpContext http, ServiceDbContext db, ILoggerFactory loggerFactory, CancellationToken ct) =>
        {
            if (!IsSuperUser(http.User)) return Results.Forbid();
            var actor = http.GetCurrentUserId();
            if (actor is null) return Results.Unauthorized();
            if (!await db.Users.AnyAsync(u => u.Id == userId, ct)) return Results.NotFound();
            await SetAsync(db, UserKey(userId), request.Enabled, actor.Value, ct);
            loggerFactory.CreateLogger("Dhole.VersionUpdateSettings").LogInformation(
                "Version update preference changed by {Actor} for {Target}; enabled={Enabled}", actor, userId, request.Enabled);
            return Results.Ok(await GetEffectiveAsync(db, userId, ct));
        });

        group.MapPut("/global", async (UpdateGlobalPreferenceRequest request,
            HttpContext http, ServiceDbContext db, ILoggerFactory loggerFactory, CancellationToken ct) =>
        {
            if (!IsSuperUser(http.User)) return Results.Forbid();
            var actor = http.GetCurrentUserId();
            if (actor is null) return Results.Unauthorized();
            await SetAsync(db, GlobalKey, request.Enabled, actor.Value, ct);
            loggerFactory.CreateLogger("Dhole.VersionUpdateSettings").LogInformation(
                "Version update global preference changed by {Actor}; enabled={Enabled}", actor, request.Enabled);
            return Results.Ok(await GetEffectiveAsync(db, actor.Value, ct));
        });
        return app;
    }

    private static string UserKey(Guid userId) => $"user:{userId:D}";

    private static async Task<object> GetEffectiveAsync(ServiceDbContext db, Guid userId, CancellationToken ct)
    {
        var globalEnabled = await GetAsync(db, GlobalKey, ct) ?? true;
        var userEnabled = await GetAsync(db, UserKey(userId), ct);
        return new { globalEnabled, userEnabled, effectiveEnabled = globalEnabled && userEnabled != false };
    }

    private static async Task<bool?> GetAsync(ServiceDbContext db, string key, CancellationToken ct)
    {
        var matches = await db.Database.SqlQueryRaw<bool>(
            "SELECT \"Enabled\" AS \"Value\" FROM auth.\"VersionUpdateNotificationSettings\" WHERE \"Key\" = {0}", key
        ).ToListAsync(ct);
        return matches.Count == 0 ? null : matches[0];
    }

    private static async Task SetAsync(ServiceDbContext db, string key, bool? enabled,
        Guid actor, CancellationToken ct)
    {
        // Persist the preference and the durable outbox event in one transaction.
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        if (enabled is null)
        {
            // Only per-user overrides may be reset to the global default.
            if (key == GlobalKey) throw new InvalidOperationException("The global preference cannot be null.");
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM auth.\"VersionUpdateNotificationSettings\" WHERE \"Key\" = {key}", ct);
        }
        else
        {
            var timestamp = DateTime.UtcNow;
            await db.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO auth.""VersionUpdateNotificationSettings""
                    (""Key"", ""Enabled"", ""UpdatedAtUtc"", ""UpdatedByUserId"")
                VALUES ({key}, {enabled.Value}, {timestamp}, {actor})
                ON CONFLICT (""Key"") DO UPDATE SET
                    ""Enabled"" = EXCLUDED.""Enabled"",
                    ""UpdatedAtUtc"" = EXCLUDED.""UpdatedAtUtc"",
                    ""UpdatedByUserId"" = EXCLUDED.""UpdatedByUserId""", ct);
        }

        // Durable event in the existing Auth outbox for downstream SignalR consumers.
        db.OutboxMessages.Add(new OutboxMessage
        {
            EventId = Guid.NewGuid(),
            EventType = EventName,
            EventName = EventName,
            SourceService = "DholeAuthService",
            PayloadJson = JsonSerializer.Serialize(new
            {
                scope = key == GlobalKey ? "global" : "user",
                userId = key == GlobalKey ? null : key["user:".Length..],
                actorUserId = actor,
                updatedAtUtc = DateTime.UtcNow,
            }),
            CorrelationId = Guid.NewGuid().ToString(),
            Status = OutboxMessageStatus.Pending,
            RetryCount = 0,
            CreatedAtUtc = DateTime.UtcNow,
        });
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }

    private static bool IsSuperUser(ClaimsPrincipal principal) =>
        principal.IsInRole("SuperUsuario") ||
        principal.Claims.Where(c => c.Type == "role" || c.Type == "roles" ||
            c.Type == ClaimTypes.Role || c.Type.EndsWith("/role", StringComparison.OrdinalIgnoreCase))
            .SelectMany(c => c.Value.Split([' ', ',', ';'],
                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            .Any(r => string.Equals(r, "SuperUsuario", StringComparison.OrdinalIgnoreCase));

    public sealed record UpdatePreferenceRequest(bool? Enabled);
    public sealed record UpdateGlobalPreferenceRequest(bool Enabled);
}
