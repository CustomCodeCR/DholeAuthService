using System.Security.Cryptography;
using System.Text;
using Dhole.Auth.Application.Abstractions.Permissions;
using Dhole.Auth.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Auth.Api.Endpoints;

/// <summary>
/// Internal, service-key-authenticated lookup used by DholeCRM. It returns current
/// effective permissions from Auth (direct and active-role scopes), never JWT claims
/// alone. No password, token, security setting or account secret is exposed.
/// </summary>
public static class InternalCrmUserEndpoints
{
    public static IEndpointRouteBuilder MapInternalCrmUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/internal/auth/crm-users/{userId:guid}", GetCrmUserAsync)
            .WithTags("Internal")
            .AllowAnonymous();
        return app;
    }

    private static async Task<IResult> GetCrmUserAsync(
        Guid userId, HttpRequest request, IConfiguration configuration,
        ServiceDbContext db, IEffectivePermissionService permissions, CancellationToken ct)
    {
        if (!Authorized(request, configuration))
            return Results.Unauthorized();

        var user = await db.Users.AsNoTracking()
            .Where(u => u.Id == userId && !u.IsDeleted && u.IsActive && !u.IsLocked)
            .Select(u => new { u.Id, u.Email, u.DisplayName })
            .SingleOrDefaultAsync(ct);
        if (user is null) return Results.NotFound();

        // Reuse the same effective-permission policy as JWT and the Auth UI:
        // an active SuperUsuario receives ALL active scopes, even if a role-scope
        // association has not yet been materialized. Auth remains the authority.
        var effective = await permissions.GetAsync(userId, ct);
        var scopes = effective.EffectiveScopes
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return Results.Ok(new {
            id = user.Id,
            active = true,
            scopes,
            email = user.Email,
            displayName = user.DisplayName
        });
    }

    private static bool Authorized(HttpRequest req, IConfiguration configuration)
    {
        var header = configuration["InternalServices:HeaderName"]?.Trim();
        if (string.IsNullOrEmpty(header)) header = "X-Dhole-Service-Key";
        var expected = configuration["InternalServices:ServiceKey"]?.Trim();
        var supplied = req.Headers[header].FirstOrDefault()?.Trim();
        if (string.IsNullOrEmpty(expected) || string.IsNullOrEmpty(supplied))
            return false;

        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var suppliedBytes = Encoding.UTF8.GetBytes(supplied);
        return expectedBytes.Length == suppliedBytes.Length &&
               CryptographicOperations.FixedTimeEquals(expectedBytes, suppliedBytes);
    }
}
