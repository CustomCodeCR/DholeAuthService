using System.Security.Cryptography;
using System.Text;
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
        ServiceDbContext db, CancellationToken ct)
    {
        if (!Authorized(request, configuration))
            return Results.Unauthorized();

        var user = await db.Users.AsNoTracking()
            .Where(u => u.Id == userId && !u.IsDeleted && u.IsActive && !u.IsLocked)
            .Select(u => new { u.Id, u.Email, u.DisplayName })
            .SingleOrDefaultAsync(ct);
        if (user is null) return Results.NotFound();

        var directScopeIds = db.UserScopes.AsNoTracking()
            .Where(x => x.UserId == userId).Select(x => x.ScopeId);

        var roleScopeIds =
            from ur in db.UserRoles.AsNoTracking()
            join role in db.Roles.AsNoTracking() on ur.RoleId equals role.Id
            join rs in db.RoleScopes.AsNoTracking() on role.Id equals rs.RoleId
            where ur.UserId == userId && role.IsActive && !role.IsDeleted
            select rs.ScopeId;

        var effectiveIds = directScopeIds.Union(roleScopeIds);
        var scopes = await db.Scopes.AsNoTracking()
            .Where(scope => scope.IsActive && effectiveIds.Contains(scope.Id))
            .Select(scope => scope.Code).Distinct().OrderBy(code => code).ToArrayAsync(ct);

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
