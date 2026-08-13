using System.Security.Cryptography;
using System.Text;
using Dhole.Auth.Domain.Shared;
using Dhole.Auth.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Auth.Api.Endpoints;

public static class InternalPricingRecipientEndpoints
{
    private static readonly string[] PricingRoleNames = [
        "Pricing",
        AuthConstants.SystemRoles.SuperUser,
        AuthConstants.SystemRoles.Administrator,
    ];

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

        var normalizedRoleNames = PricingRoleNames.Select(x => x.ToLower()).ToArray();
        var roleIds = await db.Roles
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.IsActive && normalizedRoleNames.Contains(x.Name.ToLower()))
            .Select(x => x.Id)
            .ToArrayAsync(cancellationToken);

        if (roleIds.Length == 0)
            return Results.Ok(Array.Empty<object>());

        var userIds = db.UserRoles
            .AsNoTracking()
            .Where(x => roleIds.Contains(x.RoleId))
            .Select(x => x.UserId)
            .Distinct();

        var recipients = await db.Users
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.IsActive && !x.IsLocked && userIds.Contains(x.Id))
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
