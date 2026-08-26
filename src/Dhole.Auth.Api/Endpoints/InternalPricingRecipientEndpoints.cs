using System.Security.Cryptography;
using System.Text;
using Dhole.Auth.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Auth.Api.Endpoints;

public static class InternalPricingRecipientEndpoints
{
    private const string PricingReviewScopeCode = "pricing.import-fcl-rate.review";

    public static IEndpointRouteBuilder MapInternalPricingRecipientEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/internal/auth/pricing-notification-recipients", GetPricingRecipientsAsync)
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

        var reviewScopeId = await db.Scopes
            .AsNoTracking()
            .Where(x => x.IsActive && x.Code == PricingReviewScopeCode)
            .Select(x => (Guid?)x.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (!reviewScopeId.HasValue)
            return Results.Ok(Array.Empty<object>());

        var directUserIds = db.UserScopes
            .AsNoTracking()
            .Where(x => x.ScopeId == reviewScopeId.Value)
            .Select(x => x.UserId);

        var roleUserIds =
            from userRole in db.UserRoles.AsNoTracking()
            join role in db.Roles.AsNoTracking() on userRole.RoleId equals role.Id
            join roleScope in db.RoleScopes.AsNoTracking() on role.Id equals roleScope.RoleId
            where
                !role.IsDeleted
                && role.IsActive
                && roleScope.ScopeId == reviewScopeId.Value
            select userRole.UserId;

        var recipientUserIds = directUserIds.Union(roleUserIds);

        var recipients = await db.Users
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted
                && x.IsActive
                && !x.IsLocked
                && recipientUserIds.Contains(x.Id))
            .OrderBy(x => x.DisplayName)
            .Select(x => new
            {
                userId = x.Id,
                email = x.Email,
                displayName = x.DisplayName,
                userName = x.UserName,
            })
            .ToListAsync(cancellationToken);

        return Results.Ok(recipients);
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
}
