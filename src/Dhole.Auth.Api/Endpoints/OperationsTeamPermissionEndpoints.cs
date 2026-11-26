using Dhole.Auth.Domain.Users.Enums;
using Dhole.Auth.Persistence.DbContexts;
using Dhole.Auth.Persistence.Seed;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Auth.Api.Endpoints;

/// <summary>Internal authoritative snapshot of active Operations memberships.
/// Only this projection is reconciled; direct grants, roles and their grants are untouched.</summary>
public static class OperationsTeamPermissionEndpoints
{
    public sealed record Snapshot(long Revision, OperationsTeamScopePolicy.Membership[] Members);
    public sealed class StoredGrant
    {
        public Guid UserId { get; set; }
        public string ScopeCode { get; set; } = "";
    }

    public static IEndpointRouteBuilder MapOperationsTeamPermissionEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/internal/auth/operations-team-permissions", async (
            Snapshot input, HttpRequest request, IConfiguration config, ServiceDbContext db, CancellationToken ct) =>
        {
            if (!InternalCrmUserEndpoints.Authorized(request, config)) return Results.Unauthorized();
            if (input.Revision <= 0 || input.Members is null || input.Members.Length > 50000 ||
                input.Members.Any(m => m is null || m.UserId == Guid.Empty ||
                    !OperationsTeamScopePolicy.Supports(m.DepartmentCode)))
                return Results.BadRequest(new { code = "INVALID_OPERATIONS_MEMBERSHIP_SNAPSHOT" });

            var strategy = db.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                db.ChangeTracker.Clear();
                await using var tx = await db.Database.BeginTransactionAsync(ct);
                await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(839162097425);", ct);
                var prior = await db.Database.SqlQueryRaw<long>(
                    "SELECT \"Revision\" AS \"Value\" FROM auth.\"OperationsTeamScopeCheckpoint\" WHERE \"Id\" = 1")
                    .ToListAsync(ct);
                if (prior.Count != 0 && prior[0] >= input.Revision)
                {
                    await tx.CommitAsync(ct);
                    return Results.Ok(new { changedUsers = 0, duplicate = true });
                }

                var requested = input.Members.Select(m => m.UserId).Distinct().ToArray();
                var roleNames = OperationsRolePolicy.AssignmentEligible;
                var roleIds = db.Roles.AsNoTracking().Where(r => r.IsActive && !r.IsDeleted &&
                    roleNames.Contains(r.Name)).Select(r => r.Id);
                var eligible = (await db.Users.AsNoTracking().Where(u =>
                    requested.Contains(u.Id) && !u.IsDeleted && u.IsActive && !u.IsLocked &&
                    u.UserType == UserType.Internal && db.UserRoles.Any(a =>
                        a.UserId == u.Id && roleIds.Contains(a.RoleId)))
                    .Select(u => u.Id).ToListAsync(ct)).ToHashSet();

                var desired = OperationsTeamScopePolicy.Resolve(input.Members, eligible);
                var active = (await db.Scopes.AsNoTracking().Where(s => s.IsActive)
                    .Select(s => s.Code).ToListAsync(ct)).ToHashSet(StringComparer.Ordinal);
                desired.RemoveWhere(g => !active.Contains(g.ScopeCode));

                var stored = await db.Database.SqlQueryRaw<StoredGrant>(
                    "SELECT \"UserId\", \"ScopeCode\" FROM auth.\"OperationsTeamScopeGrants\"").ToListAsync(ct);
                var existing = stored.Select(g => new OperationsTeamScopePolicy.Grant(g.UserId, g.ScopeCode)).ToHashSet();
                var removed = existing.Except(desired).ToArray();
                var added = desired.Except(existing).ToArray();
                foreach (var grant in removed)
                    await db.Database.ExecuteSqlInterpolatedAsync($"""
                        DELETE FROM auth."OperationsTeamScopeGrants"
                        WHERE "UserId" = {grant.UserId} AND "ScopeCode" = {grant.ScopeCode}
                        """, ct);
                foreach (var grant in added)
                    await db.Database.ExecuteSqlInterpolatedAsync($"""
                        INSERT INTO auth."OperationsTeamScopeGrants" ("UserId", "ScopeCode")
                        VALUES ({grant.UserId}, {grant.ScopeCode}) ON CONFLICT DO NOTHING
                        """, ct);

                var changed = removed.Concat(added).Select(g => g.UserId).Distinct().ToArray();
                foreach (var userId in changed)
                    db.QueueOperationsTeamPermissionRefresh(userId);

                // The monotonically increasing Operations database transaction ID is
                // the durable Inbox cursor. Retries and out-of-order snapshots are harmless.
                await db.Database.ExecuteSqlInterpolatedAsync($"""
                    INSERT INTO auth."OperationsTeamScopeCheckpoint" ("Id", "Revision")
                    VALUES (1, {input.Revision})
                    ON CONFLICT ("Id") DO UPDATE SET "Revision" = EXCLUDED."Revision"
                    """, ct);
                await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return Results.Ok(new { changedUsers = changed.Length, duplicate = false });
            });
        }).AllowAnonymous().WithTags("Internal");
        return app;
    }
}
