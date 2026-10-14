using System.Security.Cryptography;
using System.Text;
using Dhole.Auth.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Auth.Api.Endpoints;

/// <summary>Internal CRM user reference validation; service key is mandatory.</summary>
public static class InternalCrmUserEndpoints
{
    public static IEndpointRouteBuilder MapInternalCrmUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/internal/auth/crm-users/{userId:guid}", async (
            Guid userId, HttpRequest request, IConfiguration config,
            ServiceDbContext db, CancellationToken ct) =>
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
            return user is null ? Results.NotFound() : Results.Ok(user);
        }).AllowAnonymous().WithTags("Internal");
        return app;
    }
}
