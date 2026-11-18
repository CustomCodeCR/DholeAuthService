using Dhole.Auth.Api.Authorization;
using Dhole.Auth.Domain.Users.Enums;
using Dhole.Auth.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Auth.Api.Endpoints;

/// <summary>Authorized read-only directory. Never creates user accounts or grants.</summary>
public static class OperationsUserDirectoryEndpoints
{
    public static IEndpointRouteBuilder MapOperationsUserDirectoryEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/auth/users").RequireAuthorization().WithTags("Operations · Auth users");
        api.MapGet("/operations-directory", async (ServiceDbContext db,
            int? page, int? pageSize, string? search, CancellationToken ct) =>
        {
            if (search?.Length > 100) return Results.BadRequest(new { code = "SEARCH_TOO_LONG" });
            var query = db.Users.AsNoTracking().Where(u =>
                !u.IsDeleted && u.IsActive && !u.IsLocked && u.UserType == UserType.Internal);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim().ToLowerInvariant();
                query = query.Where(u => u.DisplayName.ToLower().Contains(q) ||
                    u.UserName.ToLower().Contains(q));
            }
            var size = Math.Clamp(pageSize ?? 50, 1, 100);
            var n = Math.Max(page ?? 1, 1);
            return Results.Ok(new {
                page = n, pageSize = size, total = await query.CountAsync(ct),
                items = await query.OrderBy(u => u.DisplayName).ThenBy(u => u.Id)
                    .Skip((n - 1) * size).Take(size)
                    .Select(u => new { u.Id, u.DisplayName, u.UserName })
                    .ToListAsync(ct)
            });
        }).RequireScope("operations.team.manage");

        api.MapGet("/{userId:guid}/operations-eligibility", async (
            Guid userId, ServiceDbContext db, CancellationToken ct) =>
        {
            var user = await db.Users.AsNoTracking()
                .Where(u => u.Id == userId && !u.IsDeleted && u.IsActive && !u.IsLocked &&
                    u.UserType == UserType.Internal)
                .Select(u => new { u.Id, u.DisplayName, u.UserName }).SingleOrDefaultAsync(ct);
            return user is null ? Results.NotFound() : Results.Ok(user);
        }).RequireScope("operations.team.manage");
        return app;
    }
}
