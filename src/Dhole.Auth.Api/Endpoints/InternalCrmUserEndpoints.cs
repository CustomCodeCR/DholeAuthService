using System.Security.Cryptography;
using System.Text;
using Dhole.Auth.Application.Abstractions.Permissions;
using Dhole.Auth.Persistence.DbContexts;
using Dhole.Auth.Domain.Users.Enums;
using Dhole.Auth.Domain.Shared;
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
        app.MapGet("/api/internal/auth/crm-users/directory", GetActiveCrmUsersAsync)
            .WithTags("Internal")
            .AllowAnonymous();
        app.MapGet("/api/internal/auth/crm-users/by-username/{username}", GetCrmUserByUsernameAsync)
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
            roles = effective.Roles.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToArray(),
            email = user.Email,
            displayName = user.DisplayName
        });
    }


    // Internal-only identity resolution for CRM commitment escalations.
    // Deliberately uses the same service-key authentication and active-user check.
    private static async Task<IResult> GetCrmUserByUsernameAsync(
        string username, HttpRequest request, IConfiguration configuration,
        ServiceDbContext db, IEffectivePermissionService permissions, CancellationToken ct)
    {
        if (!Authorized(request, configuration))
            return Results.Unauthorized();
        if (string.IsNullOrWhiteSpace(username) || username.Length > 80)
            return Results.NotFound();

        var normalized = username.Trim().ToLowerInvariant();
        var userId = await db.Users.AsNoTracking()
            .Where(u => u.UserName.ToLower() == normalized &&
                !u.IsDeleted && u.IsActive && !u.IsLocked)
            .Select(u => (Guid?)u.Id)
            .SingleOrDefaultAsync(ct);
        return userId.HasValue
            ? await GetCrmUserAsync(userId.Value, request, configuration, db, permissions, ct)
            : Results.NotFound();
    }

    // Internal and service-key protected. CRM applies administrator authorization
    // before proxying these public employee labels to a logged-in administrator.
    private static async Task<IResult> GetActiveCrmUsersAsync(
        HttpRequest request,IConfiguration configuration,ServiceDbContext db,
        int? page,int? pageSize,string? search,CancellationToken ct)
    {
        if(!Authorized(request,configuration))return Results.Unauthorized();
        if(search?.Length>100)return Results.BadRequest(new{error="Search too long"});
        // Calendar assignees must be active internal salespeople or superusers.
        // Resolve membership from Auth roles, not user names, JWT claims or UI filters.
        var eligibleRoles=db.Roles.AsNoTracking()
            .Where(role=>!role.IsDeleted && role.IsActive &&
                (role.Name.ToLower()=="vendedor" || role.Name.ToLower()=="vendedores" ||
                 role.Name.ToLower()=="vendedor supervisor" ||
                 role.Name.ToLower()=="vendedor jefe" ||
                 role.Name==AuthConstants.SystemRoles.SuperUser))
            .Select(role=>role.Id);
        var query=db.Users.AsNoTracking()
            .Where(u=>!u.IsDeleted && u.IsActive && !u.IsLocked &&
                u.UserType==UserType.Internal &&
                db.UserRoles.Any(ur=>ur.UserId==u.Id && eligibleRoles.Contains(ur.RoleId)));
        if(!string.IsNullOrWhiteSpace(search))
        {
            var term=search.Trim().ToLower();
            query=query.Where(u=>u.DisplayName.ToLower().Contains(term) ||
                u.UserName.ToLower().Contains(term));
        }
        var n=Math.Max(1,page??1);
        var size=Math.Clamp(pageSize??100,1,100);
        var total=await query.CountAsync(ct);
        var users=await query.OrderBy(u=>u.DisplayName).ThenBy(u=>u.Id)
            .Skip((n-1)*size).Take(size)
            .Select(u=>new{id=u.Id,displayName=u.DisplayName,userName=u.UserName})
            .ToListAsync(ct);
        return Results.Ok(new{total,page=n,pageSize=size,items=users});
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
