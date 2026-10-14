using System.Security.Cryptography;
using System.Text;
using Dhole.Auth.Persistence.DbContexts;
using Dhole.Auth.Application.Abstractions.Permissions;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Auth.Api.Endpoints;

/// <summary>Internal CRM user reference validation; service key is mandatory.</summary>
public static class InternalCrmUserEndpoints
{
    public static IEndpointRouteBuilder MapInternalCrmUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/internal/auth/crm-users/{userId:guid}", async (
            Guid userId, HttpRequest request, IConfiguration config,
            ServiceDbContext db, IEffectivePermissionService permissions, CancellationToken ct) =>
        {
            var expected = config["InternalServices:ServiceKey"]?.Trim();
            var given = request.Headers[config["InternalServices:HeaderName"] ?? "X-Dhole-Service-Key"].FirstOrDefault()?.Trim();
            if (string.IsNullOrWhiteSpace(expected) || string.IsNullOrWhiteSpace(given)) return Results.Unauthorized();
            var a = Encoding.UTF8.GetBytes(expected);
            var b = Encoding.UTF8.GetBytes(given);
            if (a.Length != b.Length || !CryptographicOperations.FixedTimeEquals(a, b)) return Results.Unauthorized();
            var user = await db.Users.AsNoTracking()
                .Where(u => u.Id == userId && u.IsActive && !u.IsLocked && !u.IsDeleted)
                .Select(u => new { id = u.Id, active = true }).FirstOrDefaultAsync(ct);
            if (user is null) return Results.NotFound();
            var permissionSet = await permissions.GetAsync(userId, ct);
            return Results.Ok(new { user.id, user.active, scopes = permissionSet.EffectiveScopes });
        }).AllowAnonymous().WithTags("Internal");
        return app;
    }
}
