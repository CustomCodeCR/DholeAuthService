using System.Security.Cryptography;
using System.Text;
using Dhole.Auth.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Auth.Api.Endpoints;

public static class InternalPricingRecipientEndpoints
{
    private static readonly string[] PricingNotificationScopeCodes =
    [
        "pricing.import-fcl-rate.review",
        "pricing.rate.update",
    ];

    private const string PricingSellerScopeCode = "pricing.rate-request.create";
    private const string PricingSalesExecutiveRoleName = "Vendedor";

    public static IEndpointRouteBuilder MapInternalPricingRecipientEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/internal/auth/pricing-notification-recipients", GetPricingRecipientsAsync)
            .WithTags("Internal")
            .AllowAnonymous();

        app.MapGet("/api/internal/auth/pricing-sellers", GetPricingSellersAsync)
            .WithTags("Internal")
            .AllowAnonymous();

        app.MapGet("/api/internal/auth/pricing-sales-executives", GetPricingSalesExecutivesAsync)
            .WithTags("Internal")
            .AllowAnonymous();

        return app;
    }

    private static async Task<IResult> GetPricingRecipientsAsync(
        HttpRequest request,
        IConfiguration configuration,
        ServiceDbContext db,
        CancellationToken cancellationToken)
    {
        if (!HasValidServiceKey(request, configuration))
            return Results.Unauthorized();

        var recipients = await GetActiveUsersWithAnyScopeAsync(
            db,
            PricingNotificationScopeCodes,
            cancellationToken
        );

        return Results.Ok(recipients);
    }

    private static async Task<IResult> GetPricingSellersAsync(
        HttpRequest request,
        IConfiguration configuration,
        ServiceDbContext db,
        CancellationToken cancellationToken)
    {
        if (!HasValidServiceKey(request, configuration))
            return Results.Unauthorized();

        var sellers = await GetActiveUsersWithAnyScopeAsync(
            db,
            [PricingSellerScopeCode],
            cancellationToken
        );

        return Results.Ok(sellers);
    }

    private static async Task<IResult> GetPricingSalesExecutivesAsync(
        HttpRequest request,
        IConfiguration configuration,
        ServiceDbContext db,
        CancellationToken cancellationToken)
    {
        if (!HasValidServiceKey(request, configuration))
            return Results.Unauthorized();

        var executives = await GetActiveUsersWithRoleAsync(
            db,
            PricingSalesExecutiveRoleName,
            cancellationToken
        );

        return Results.Ok(executives);
    }

    private static async Task<IReadOnlyList<InternalPricingUser>> GetActiveUsersWithAnyScopeAsync(
        ServiceDbContext db,
        IReadOnlyCollection<string> scopeCodes,
        CancellationToken cancellationToken)
    {
        var scopeIds = await db.Scopes
            .AsNoTracking()
            .Where(x => x.IsActive && scopeCodes.Contains(x.Code))
            .Select(x => x.Id)
            .ToArrayAsync(cancellationToken);

        if (scopeIds.Length == 0)
            return [];

        var directUserIds = db.UserScopes
            .AsNoTracking()
            .Where(x => scopeIds.Contains(x.ScopeId))
            .Select(x => x.UserId);

        var roleUserIds =
            from userRole in db.UserRoles.AsNoTracking()
            join role in db.Roles.AsNoTracking() on userRole.RoleId equals role.Id
            join roleScope in db.RoleScopes.AsNoTracking() on role.Id equals roleScope.RoleId
            where
                !role.IsDeleted
                && role.IsActive
                && scopeIds.Contains(roleScope.ScopeId)
            select userRole.UserId;

        var recipientUserIds = directUserIds.Union(roleUserIds);

        return await GetActiveUsersAsync(db, recipientUserIds, cancellationToken);
    }

    private static async Task<IReadOnlyList<InternalPricingUser>> GetActiveUsersWithRoleAsync(
        ServiceDbContext db,
        string roleName,
        CancellationToken cancellationToken)
    {
        var normalizedRoleName = roleName.Trim().ToLower();
        var roleUserIds =
            from userRole in db.UserRoles.AsNoTracking()
            join role in db.Roles.AsNoTracking() on userRole.RoleId equals role.Id
            where
                !role.IsDeleted
                && role.IsActive
                && role.Name.ToLower() == normalizedRoleName
            select userRole.UserId;

        return await GetActiveUsersAsync(db, roleUserIds, cancellationToken);
    }

    private static async Task<IReadOnlyList<InternalPricingUser>> GetActiveUsersAsync(
        ServiceDbContext db,
        IQueryable<Guid> userIds,
        CancellationToken cancellationToken)
    {
        return await db.Users
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted
                && x.IsActive
                && !x.IsLocked
                && userIds.Contains(x.Id))
            .OrderBy(x => x.DisplayName)
            .ThenBy(x => x.UserName)
            .Select(x => new InternalPricingUser(
                x.Id,
                x.Email,
                x.DisplayName,
                x.UserName
            ))
            .ToListAsync(cancellationToken);
    }

    private static bool HasValidServiceKey(HttpRequest request, IConfiguration configuration)
    {
        var headerName = configuration["InternalServices:HeaderName"]?.Trim();
        if (string.IsNullOrWhiteSpace(headerName)) headerName = "X-Dhole-Service-Key";

        var expected = configuration["InternalServices:ServiceKey"]?.Trim();
        if (string.IsNullOrWhiteSpace(expected)) return false;

        var supplied = request.Headers[headerName].FirstOrDefault()?.Trim();
        if (string.IsNullOrWhiteSpace(supplied)) return false;

        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var suppliedBytes = Encoding.UTF8.GetBytes(supplied);
        return expectedBytes.Length == suppliedBytes.Length
            && CryptographicOperations.FixedTimeEquals(expectedBytes, suppliedBytes);
    }

    private sealed record InternalPricingUser(
        Guid UserId,
        string? Email,
        string? DisplayName,
        string? UserName
    );
}
