using Dhole.Auth.Api.Authorization;
using Dhole.Auth.Application.Abstractions.Permissions;
using Dhole.Auth.Domain.Users.Enums;
using Dhole.Auth.Persistence.DbContexts;
using Dhole.Auth.Persistence.Seed;
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
            // Do not expose all internal users: only explicitly assigned logistics
            // operators/supervisors are eligible for operational team membership.
            var eligibleRoleNames = OperationsRolePolicy.AssignmentEligible;
            var eligibleRoleIds = db.Roles.AsNoTracking()
                .Where(role => role.IsActive && !role.IsDeleted &&
                    eligibleRoleNames.Contains(role.Name))
                .Select(role => role.Id);
            var query = db.Users.AsNoTracking().Where(u =>
                !u.IsDeleted && u.IsActive && !u.IsLocked && u.UserType == UserType.Internal &&
                db.UserRoles.Any(assignment => assignment.UserId == u.Id &&
                    eligibleRoleIds.Contains(assignment.RoleId)));
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

        // Audit activity may be performed by SuperUsuarios who are not eligible
        // as operational team members. Resolve only audited actors' display names
        // through a separate, permission-gated read-only API.
        api.MapGet("/operations-audit-actors", async (
            string? ids, ServiceDbContext db, CancellationToken ct) =>
        {
            var segments = (ids ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0 || segments.Length > 100 ||
                segments.Any(id => !Guid.TryParse(id, out var parsed) || parsed == Guid.Empty))
                return Results.BadRequest(new { code = "INVALID_AUDIT_ACTOR_IDS" });
            var actorIds = segments.Select(Guid.Parse).Distinct().ToArray();
            var items = await db.Users.AsNoTracking()
                .Where(u => actorIds.Contains(u.Id) && u.UserType == UserType.Internal)
                .Select(u => new { u.Id, u.DisplayName, u.UserName })
                .ToArrayAsync(ct);
            return Results.Ok(new { items });
        }).RequireScope("operations.audit.read");

        api.MapGet("/{userId:guid}/operations-eligibility", EligibilityAsync)
            .RequireScope("operations.team.manage");
        app.MapGet("/api/internal/auth/operations-users/{userId:guid}/eligibility", async (
            Guid userId, HttpRequest request, IConfiguration configuration,
            ServiceDbContext db, IEffectivePermissionService permissions, CancellationToken ct) =>
        {
            if (!InternalCrmUserEndpoints.Authorized(request, configuration))
                return Results.Unauthorized();
            return await EligibilityAsync(userId, db, permissions, ct);
        }).AllowAnonymous().WithTags("Internal");
        return app;
    }

    private static async Task<IResult> EligibilityAsync(Guid userId, ServiceDbContext db,
        IEffectivePermissionService permissions, CancellationToken ct)
    {
            var eligibleRoleNames = OperationsRolePolicy.AssignmentEligible;
            var eligibleRoleIds = db.Roles.AsNoTracking()
                .Where(role => role.IsActive && !role.IsDeleted &&
                    eligibleRoleNames.Contains(role.Name))
                .Select(role => role.Id);
            var user = await db.Users.AsNoTracking()
                .Where(u => u.Id == userId && !u.IsDeleted && u.IsActive && !u.IsLocked &&
                    u.UserType == UserType.Internal)
                .Select(u => new { u.Id, u.DisplayName, u.UserName }).SingleOrDefaultAsync(ct);
            if (user is null) return Results.NotFound();
            var eligibleForOperations = await db.UserRoles.AsNoTracking()
                .AnyAsync(assignment => assignment.UserId == userId &&
                    eligibleRoleIds.Contains(assignment.RoleId), ct);
            var effective = await permissions.GetAsync(userId, ct);
            var canReviewAperturas =
                effective.EffectiveScopes.Contains("operations.aperture.validation.review",
                    StringComparer.OrdinalIgnoreCase) &&
                effective.EffectiveScopes.Contains("operations.case.read",
                    StringComparer.OrdinalIgnoreCase);
            return Results.Ok(new {
                user.Id, user.DisplayName, user.UserName, eligibleForOperations,
                canReviewAperturas = eligibleForOperations && canReviewAperturas
            });

    }
}
